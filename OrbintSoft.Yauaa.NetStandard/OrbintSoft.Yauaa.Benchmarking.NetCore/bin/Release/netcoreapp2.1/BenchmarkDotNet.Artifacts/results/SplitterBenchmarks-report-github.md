``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17763
Intel Core i7-5820K CPU 3.30GHz (Broadwell), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|       Method |       Mean |      Error |     StdDev |
|------------- |-----------:|-----------:|-----------:|
| DirectRange1 |   100.9 ns |  0.5914 ns |  0.5243 ns |
| SplitLRange1 |   313.1 ns |  5.5401 ns |  4.9111 ns |
| DirectRange2 |   277.5 ns |  1.2873 ns |  1.2041 ns |
| SplitLRange2 |   337.2 ns |  6.4316 ns |  5.3707 ns |
| DirectRange3 |   449.4 ns |  8.4861 ns |  7.5227 ns |
| SplitLRange3 |   360.4 ns |  6.3951 ns |  5.9820 ns |
| DirectRange4 |   664.4 ns |  5.8273 ns |  5.4508 ns |
| SplitLRange4 |   394.9 ns |  6.6369 ns |  6.2082 ns |
| DirectRange5 |   957.5 ns | 12.9137 ns | 11.4476 ns |
| SplitLRange5 |   406.7 ns | 11.1250 ns |  9.8620 ns |
| DirectRange6 | 1,178.4 ns |  6.1149 ns |  4.7741 ns |
| SplitLRange6 |   437.0 ns |  8.1143 ns |  9.6594 ns |
| DirectRange7 | 1,412.7 ns | 10.7806 ns |  8.4168 ns |
| SplitLRange7 |   452.9 ns |  1.9882 ns |  1.8598 ns |
| DirectRange8 | 1,508.4 ns | 10.2294 ns |  7.9865 ns |
| SplitLRange8 |   484.1 ns |  9.6127 ns |  8.9917 ns |
| DirectRange9 | 1,615.0 ns | 29.0072 ns | 27.1334 ns |
| SplitLRange9 |   504.2 ns |  9.9911 ns | 10.2601 ns |
