using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperTesting.Models
{    
    public class Customer
    {
        [ExplicitKey]
        public string CustomerID { get; set; }

        public string CompanyName { get; set; }

        public string ContactName { get; set; }

        public string ContactTitle { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        //declare an existing column is no need to insert/update
        [Write(false)]        
        public string Fax { get; set; }        
        
        //declare a non-existing column should be ignored to insert/update
        [Computed]
        public string ReadOnlyProp { get { return string.Empty; } }
    }
}
