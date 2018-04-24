using BenchmarkDotNet.Running;
using System;

namespace Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {            
            BenchmarkRunner.Run(typeof(EfCore2VsDapper));

            Console.WriteLine("done");
            Console.ReadKey();
        }
    }
}