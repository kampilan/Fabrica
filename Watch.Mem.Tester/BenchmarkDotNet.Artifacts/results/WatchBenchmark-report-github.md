``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.3086/22H2/2022Update)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK=7.0.304
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2


```
|         Method |     Mean |   Error |  StdDev | Allocated |
|--------------- |---------:|--------:|--------:|----------:|
| QuietBenchmark | 279.6 ns | 0.79 ns | 0.70 ns |         - |
