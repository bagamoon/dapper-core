using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace Benchmark
{
    public class ConnectionFactory
    {
        public readonly static string ConnectionString = "Server=localhost\\sqlexpress;Database=northwind;User Id=sa;Password=pass1234";

        public static IDbConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
    }
}
