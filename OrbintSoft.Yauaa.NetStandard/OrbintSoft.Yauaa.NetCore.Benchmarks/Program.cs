using BenchmarkDotNet.Running;

namespace OrbintSoft.Yauaa.NetCore.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AnalyzerBenchmarks>();
        }
    }
}
