``` ini

BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.345 (1803/April2018Update/Redstone4)
Intel Core i7-5820K CPU 3.30GHz (Broadwell), 1 CPU, 12 logical and 6 physical cores
Frequency=3215210 Hz, Resolution=311.0217 ns, Timer=TSC
.NET Core SDK=2.1.500
  [Host]     : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT  [AttachedDebugger]
  DefaultJob : .NET Core 2.1.6 (CoreCLR 4.6.27019.06, CoreFX 4.6.27019.05), 64bit RyuJIT


```
|                 Method |       Mean |     Error |     StdDev |
|----------------------- |-----------:|----------:|-----------:|
|       Android6Chrome46 | 1,065.6 us | 21.237 us |  26.858 us |
|           AndroidPhone | 1,750.5 us | 36.633 us |  98.411 us |
|              Googlebot |   648.5 us | 14.750 us |  40.872 us |
| GoogleBotMobileAndroid | 1,534.2 us | 68.634 us | 198.025 us |
|           GoogleAdsBot |   571.5 us | 11.342 us |  25.369 us |
|     GoogleAdsBotMobile |   971.3 us | 19.397 us |  42.983 us |
|                 IPhone |   867.5 us | 14.128 us |  11.797 us |
|      IPhoneFacebookApp | 1,594.6 us | 36.958 us |  95.401 us |
|                   IPad |   862.4 us | 16.916 us |  35.681 us |
|               Win7ie11 |   826.4 us | 16.057 us |  19.719 us |
|            Win10Edge13 |   934.0 us | 20.133 us |  48.238 us |
|          Win10Chrome51 |   834.9 us | 15.642 us |  13.866 us |
|              Win10IE11 |   869.8 us | 16.628 us |  16.331 us |
|              HackerSQL |   506.9 us |  6.243 us |   5.839 us |
|       HackerShellShock |   480.6 us |  9.559 us |  10.625 us |
