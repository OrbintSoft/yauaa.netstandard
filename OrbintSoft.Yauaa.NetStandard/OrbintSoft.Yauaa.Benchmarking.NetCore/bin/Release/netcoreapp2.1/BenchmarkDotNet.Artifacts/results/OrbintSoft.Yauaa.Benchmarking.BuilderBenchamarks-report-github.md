``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17763
Intel Core i7-5820K CPU 3.30GHz (Broadwell), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|     Method |    Mean |    Error |   StdDev |
|----------- |--------:|---------:|---------:|
| NewBuilder | 9.410 s | 0.0219 s | 0.0194 s |
