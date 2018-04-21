using Dapper;
using Dapper.Contrib.Extensions;
using DapperTesting.Models;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DapperTesting.Tests
{
    [TestClass]
    public class ContribTest
    {
        [TestCleanup]
        public void CleanUp()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                var deleteSql = @"delete from [Order Details]
                                  delete from orders
                                  delete from customers
                                  delete from products
                                  delete from categories                                  
                                  DBCC CHECKIDENT ('orders', RESEED, 0)
                                  DBCC CHECKIDENT ('products', RESEED, 0)
                                  DBCC CHECKIDENT ('categories', RESEED, 0)";
                conn.Execute(deleteSql);
            }
        }

        [TestMethod]
        public void GetAll_Should_Return_All_Results()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                conn.Insert(new Customer { CustomerID = "Kenny", CompanyName = "Kenny G" });
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.GetAll<Customer>().ToList();

                result.Count().Should().Be(2);
                result[0].CustomerID.Should().BeEquivalentTo("Brett");
                result[1].CustomerID.Should().BeEquivalentTo("Kenny");
            }
        }

        [TestMethod]
        public void Update_Should_Save_Modified_Entity_With_Writable_Columns()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Execute(@"insert into customers (CustomerId, CompanyName, Fax) 
                               values ('Brett', 'Brett Yu', '123')");
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                var customer = conn.Get<Customer>("Brett");
                customer.CompanyName = "Little Gray";
                customer.Fax = "999";
                var success = conn.Update(customer);

                success.Should().BeTrue();
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.Get<Customer>("Brett");

                result.Fax.Should().BeEquivalentTo("123");
                result.CompanyName.Should().BeEquivalentTo("Little Gray");
            }
        }

        [TestMethod]
        public void Insert_With_Identity_Key_Should_Be_Inserted_Successfully_And_Return_New_Id()
        {
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                var orderId = conn.Insert(new Order { CustomerID = "Brett", OrderDate = DateTime.Now });
                orderId.Should().Be(1);

                var result = conn.GetAll<Order>().ToList();
                result.Count().Should().Be(1);
                result[0].CustomerID = "Brett";
            }
        }

        [TestMethod]
        public void Insert_With_Composite_Key_Should_Be_Inserted_Successfully()
        {
            int appleId;
            int bananaId;
            int orderId;
            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" });
                appleId = (int)conn.Insert(new Product { ProductName = "Apple" });
                bananaId = (int)conn.Insert(new Product { ProductName = "Banana" });
                orderId = (int)conn.Insert(new Order { CustomerID = "Brett", OrderDate = DateTime.Now });
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                conn.Insert(new OrderDetail { OrderId = orderId, ProductId = appleId, Quantity = 100 });
                conn.Insert(new OrderDetail { OrderId = orderId, ProductId = bananaId, Quantity = 200 });

                //not able to use this api for composite keys entity
                //var result = conn.GetAll<OrderDetail>().ToList();

                var result = conn.Query<OrderDetail>("select OrderId, ProductId, Quantity from [Order Details]").ToList();
                result[0].Quantity.Should().Be(100);
                result[1].Quantity.Should().Be(200);
            }
        }

        [TestMethod]
        public void Transaction_Should_Rollback_When_Exception_Occurred()
        {
            try
            {
                using (var conn = ConnectionFactory.GetConnection())
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" }, tran);
                        conn.Insert(new Customer { CustomerID = "Brett", CompanyName = "Brett Yu" }, tran);

                        tran.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.GetType().Name.Should().BeEquivalentTo("SqlException");
            }

            using (var conn = ConnectionFactory.GetConnection())
            {
                var result = conn.GetAll<Customer>();

                result.Count().Should().Be(0);
            }
        }
    }
}