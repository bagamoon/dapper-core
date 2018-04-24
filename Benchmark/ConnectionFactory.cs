using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Benchmark
{
    public class ConnectionFactory
    {
        public static IDbConnection GetConnection()
        {
            return new SqlConnection("Server=localhost;Database=northwind;User Id=sa;Password=pass1234");
        }
    }
}
