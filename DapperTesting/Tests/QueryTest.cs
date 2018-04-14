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

namespace DapperTesting.Tests
{
    [TestClass]
    public class QueryTest
    {
        [TestCleanup]
        public void CleanUp()
        {
            using (var conn = GetConnection())
            {
                var deleteSql = @"delete from customers";
                conn.Execute(deleteSql);
            }
        }

        [TestMethod]
        public void Query_Without_Conditions_Should_Return_All_Results()
        {
            using (var conn = GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = "select CustomerId, CompanyName from Customers";

            using (var conn = GetConnection())
            {
                var result = conn.Query<Customer>(sql);

                result.Count().Should().Be(2);
                result.First().CustomerID.Should().BeEquivalentTo("Brett");
                result.Last().CustomerID.Should().BeEquivalentTo("Kenny");
            }
        }

        [TestMethod]
        public void Query_First_With_Condition_Should_Return_Correct_Result()
        {
            using (var conn = GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = "select CustomerId, CompanyName from Customers where CustomerId = 'Brett'";

            using (var conn = GetConnection())
            {
                var result = conn.QueryFirst<Customer>(sql);

                result.CustomerID.Should().BeEquivalentTo("Brett");
                result.CompanyName.Should().BeEquivalentTo("Brett Yu");
            }
        }

        [TestMethod]
        public void Query_With_Parameter_Should_Return_Correct_Result()
        {
            using (var conn = GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            var sql = @"select CustomerId, CompanyName from Customers 
                        where CustomerId = @customerId";

            using (var conn = GetConnection())
            {
                var result = conn.QueryFirst<Customer>(sql,
                    new { customerId = new DbString { Value = "Brett" } });

                result.CustomerID.Should().BeEquivalentTo("Brett");
                result.CompanyName.Should().BeEquivalentTo("Brett Yu");
            }
        }

        [TestMethod]
        public void Query_With_Sql_Builder_Should_Return_Correct_Result()
        {
            using (var conn = GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Alpha", CompanyName = "Alpha Chen", City = "Taipei" });
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu", City = "Taipei" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G", City = "Taipei" });                
            }

            var builder = new SqlBuilder();
            var selectSupplierIdBuilder = builder.AddTemplate("Select /**select**/ from Customers /**where**/ ");
            builder.Select("CustomerID");            
            builder.Where("City = @city", new { city = new DbString { Value = "Taipei" } });
            builder.Where("CustomerID = @customerId", new { customerId = new DbString { Value = "Brett" } });

            using (var conn = GetConnection())
            {
                var result = conn.QueryFirst<Customer>(selectSupplierIdBuilder.RawSql, selectSupplierIdBuilder.Parameters);
                result.CustomerID.Should().BeEquivalentTo("Brett");
            }
        }

        [TestMethod]
        public void Query_With_DynamicType_Should_Return_Corresponded_Columns()
        {
            using (var conn = GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
            }

            var sql = "select CustomerId, CompanyName from Customers";

            using (var conn = GetConnection())
            {
                var result = conn.QueryFirst(sql);

                Assert.AreEqual("Brett", result.CustomerId);
                Assert.AreEqual("Brett Yu", result.CompanyName);
            }
        }

        public IDbConnection GetConnection()
        {
            return new SqlConnection("Server=localhost;Database=northwind;User Id=sa;Password=pass1234");
        }
    }
}
