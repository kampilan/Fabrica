``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 10 (10.0.19045.3086/22H2/2022Update)
11th Gen Intel Core i7-11700 2.50GHz, 1 CPU, 4 logical and 4 physical cores
.NET SDK=7.0.304
  [Host]     : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.7 (7.0.723.27404), X64 RyuJIT AVX2


```
|         Method |     Mean |   Error |  StdDev |   Gen0 |   Gen1 | Allocated |
|--------------- |---------:|--------:|--------:|-------:|-------:|----------:|
| QuietBenchmark | 943.5 ns | 3.29 ns | 2.57 ns | 0.0496 | 0.0477 |     448 B |
