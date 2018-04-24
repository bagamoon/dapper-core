using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace Benchmark
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            Add(new MemoryDiagnoser());                       
            //Add(Job.Default
            //    .WithUnrollFactor(50)
            //    //.WithIterationTime(new TimeInterval(500, TimeUnit.Millisecond))
            //    .WithLaunchCount(1)
            //    .WithWarmupCount(0)
            //    .WithTargetCount(5)
            //    .WithRemoveOutliers(true)
            //);
        }
    }
}