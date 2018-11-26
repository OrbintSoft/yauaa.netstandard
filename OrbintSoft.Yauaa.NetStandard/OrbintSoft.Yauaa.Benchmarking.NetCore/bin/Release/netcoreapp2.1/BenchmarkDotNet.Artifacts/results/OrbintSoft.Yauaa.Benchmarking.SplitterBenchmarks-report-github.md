``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17763
Intel Core i7-5820K CPU 3.30GHz (Broadwell), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|       Method |        Mean |     Error |    StdDev |
|------------- |------------:|----------:|----------:|
| DirectRange1 |    97.80 ns |  1.989 ns |  3.324 ns |
| SplitLRange1 |   300.77 ns |  2.485 ns |  2.324 ns |
| DirectRange2 |   259.67 ns |  5.080 ns |  5.647 ns |
| SplitLRange2 |   318.95 ns |  3.825 ns |  3.391 ns |
| DirectRange3 |   398.28 ns |  4.553 ns |  4.036 ns |
| SplitLRange3 |   338.50 ns |  1.906 ns |  1.782 ns |
| DirectRange4 |   597.74 ns |  2.186 ns |  2.045 ns |
| SplitLRange4 |   367.98 ns |  2.770 ns |  2.591 ns |
| DirectRange5 |   866.25 ns |  5.484 ns |  4.579 ns |
| SplitLRange5 |   380.89 ns |  2.142 ns |  2.003 ns |
| DirectRange6 | 1,084.16 ns |  7.153 ns |  5.585 ns |
| SplitLRange6 |   402.58 ns |  1.968 ns |  1.841 ns |
| DirectRange7 | 1,287.14 ns | 12.344 ns | 10.943 ns |
| SplitLRange7 |   444.21 ns |  8.558 ns |  8.005 ns |
| DirectRange8 | 1,381.30 ns | 20.759 ns | 18.402 ns |
| SplitLRange8 |   453.28 ns |  7.868 ns |  7.360 ns |
| DirectRange9 | 1,474.35 ns |  7.801 ns |  6.916 ns |
| SplitLRange9 |   470.89 ns |  3.615 ns |  3.204 ns |
