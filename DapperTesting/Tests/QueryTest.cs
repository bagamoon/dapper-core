using Dapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using FluentAssertions;
using Dapper.Contrib.Extensions;
using DapperTesting.Models;
using DapperTesting.ViewModels;

namespace DapperTesting.Tests
{    
    [TestClass]
    public class QueryTest
    {
        [TestCleanup]
        public void CleanUp()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var deleteSql = @"delete from orders
                                  delete from customers
                                  delete from products
                                  delete from categories
                                  delete from EmployeeTerritories
                                  delete from Territories
                                  delete from Employees
                                  DBCC CHECKIDENT ('orders', RESEED, 0)
                                  DBCC CHECKIDENT ('products', RESEED, 0)
                                  DBCC CHECKIDENT ('categories', RESEED, 0)
                                  DBCC CHECKIDENT ('Employees', RESEED, 0)";
                conn.Execute(deleteSql);
            }
        }

        
        [TestMethod()]
        public void Query_Without_Conditions_Should_Return_All_Results()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = "select CustomerId, CompanyName from Customers";

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.Query<Customer>(sql).ToList();

                result.Count().Should().Be(2);
                result[0].CustomerID.Should().BeEquivalentTo("Brett");
                result[1].CustomerID.Should().BeEquivalentTo("Kenny");
            }
        }

        [TestMethod]
        public void QueryFirst_With_Condition_Should_Return_Correct_Result()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = @"select CustomerId, CompanyName 
                        from Customers 
                        where CustomerId = 'Brett'";

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.QueryFirst<Customer>(sql);

                result.CustomerID.Should().BeEquivalentTo("Brett");
                result.CompanyName.Should().BeEquivalentTo("Brett Yu");
            }
        }

        [TestMethod]
        public void QueryFirst_With_Parameter_Should_Return_Correct_Result()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = @"select CustomerId, CompanyName from Customers 
                        where CustomerId = @customerId";

            using (var conn = ConnectionFactory.GetConnection())
            {
                //IsAnsi: true -> varchar, false -> nvarchar
                var result = conn.QueryFirst<Customer>(sql,
                    new { customerId = new DbString { Value = "Brett", IsAnsi = false } });

                result.CustomerID.Should().BeEquivalentTo("Brett");
                result.CompanyName.Should().BeEquivalentTo("Brett Yu");
            }
        }

        [TestMethod]
        public void Query_With_Sql_Builder_Should_Return_Correct_Result()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Alpha", CompanyName = "Alpha Chen", City = "Taipei" });
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu", City = "Taipei" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G", City = "Taipei" });
            }

            var builder = new SqlBuilder();

            //please note that keyword of template must be lower case: /**select**/ /**where**/
            var template = builder.AddTemplate("Select /**select**/ from Customers /**where**/ ");
            builder.Select("CustomerID");
            builder.Where("City = @city", new { city = new DbString { Value = "Taipei", IsAnsi = false } });
            builder.Where("CustomerID = @customerId", new { customerId = new DbString { Value = "Brett", IsAnsi = false } });

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.QueryFirst<Customer>(template.RawSql, template.Parameters);
                result.CustomerID.Should().BeEquivalentTo("Brett");
            }
        }

        [TestMethod]
        public void Query_With_DynamicType_Should_Return_Corresponded_Columns()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
            }

            var sql = "select CustomerId, CompanyName from Customers";

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.QueryFirst(sql);

                Assert.AreEqual("Brett", result.CustomerId);
                Assert.AreEqual("Brett Yu", result.CompanyName);
            }
        }

        [TestMethod]
        public void Query_Return_One_To_Many_Join_Result_Should_Map_With_Nested_Object()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var fruitsId = conn.Insert(new Category { CategoryName = "Fruits" });
                var drinksId = conn.Insert(new Category { CategoryName = "Drinks" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Apple" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Orange" });
                conn.Insert(new Product { CategoryID = (int)drinksId, ProductName = "Soda" });
            }

            string sql = @"select c.CategoryId, c.CategoryName, p.ProductId, p.ProductName 
                           from Categories c 
                           left join Products p on c.CategoryId = p.CategoryId";

            using (var conn = ConnectionFactory.GetConnection())
            {
                var categoryVmLookup = new Dictionary<int, CategoryViewModel>();
                var result = conn.Query<Category, Product, CategoryViewModel>(
                                sql,
                                (category, product) =>
                                    {
                                        CategoryViewModel vm;

                                        if (!categoryVmLookup.TryGetValue(category.CategoryID, out vm))
                                        {
                                            vm = new CategoryViewModel()
                                            {
                                                CategoryID = category.CategoryID,
                                                CategoryName = category.CategoryName,
                                                Products = new List<Product>()
                                            };

                                            categoryVmLookup.Add(category.CategoryID, vm);
                                        }

                                        vm.Products.Add(product);

                                        return vm;
                                    },
                                splitOn: "ProductId")
                                .Distinct()
                                .ToList();

                result.Count().Should().Be(2);
                result[0].Products.Count().Should().Be(2);
                result[1].Products.Count().Should().Be(1);
            }
        }

        [TestMethod]
        public void Query_Different_Product_Should_Be_Able_To_Map_To_Corresponded_Type()
        {           
            using (var conn = ConnectionFactory.GetConnection())
            {
                var fruitsId = conn.Insert(new Category { CategoryName = "Fruits" });
                var drinksId = conn.Insert(new Category { CategoryName = "Drinks" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Apple" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Orange" });
                conn.Insert(new Product { CategoryID = (int)drinksId, ProductName = "Soda" });
            }

            var sql = @"select p.ProductId, p.ProductName, c.CategoryName 
                        from Products p 
                        inner join Categories c on p.CategoryId = c.CategoryId";

            using (var conn = ConnectionFactory.GetConnection())
            {
                var products = new List<Product>();
                using (var reader = conn.ExecuteReader(sql))
                {
                    var defaultParser = reader.GetRowParser<Product>();
                    var fruitParser = reader.GetRowParser<FruitProduct>();
                    var drinkParser = reader.GetRowParser<DrinkProduct>();

                    while (reader.Read())
                    {
                        Product p;
                        switch(reader["CategoryName"].ToString())
                        {
                            case "Fruits":
                                p = fruitParser(reader);
                                break;

                            case "Drinks":
                                p = drinkParser(reader);
                                break;
                            default:
                                p = defaultParser(reader);
                                break;
                        }
                        products.Add(p);
                    }
                }

                (products[0] is FruitProduct).Should().BeTrue();
                (products[1] is FruitProduct).Should().BeTrue();
                (products[2] is DrinkProduct).Should().BeTrue();
            }

        }

        [TestMethod]
        public void QueryMultiple_Return_Multiple_RecordSet_Should_Map_With_Corresponded_Collections()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var fruitsId = conn.Insert(new Category { CategoryName = "Fruits" });
                var drinksId = conn.Insert(new Category { CategoryName = "Drinks" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Apple" });
                conn.Insert(new Product { CategoryID = (int)fruitsId, ProductName = "Orange" });
                conn.Insert(new Product { CategoryID = (int)drinksId, ProductName = "Soda" });
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.QueryMultiple("exec usp_GetCategoriesAndProducts");

                var categories = result.Read<Category>();
                var products = result.Read<Product>();

                categories.Count().Should().Be(2);
                products.Count().Should().Be(3);
            }
        }
    }
}
