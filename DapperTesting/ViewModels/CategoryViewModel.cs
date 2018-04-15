using DapperTesting.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DapperTesting.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }        

        public List<Product> Products { get; set; }
    }
}
