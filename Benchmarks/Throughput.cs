
using Dastardly.Data;

namespace Benchmarks {

    /// <summary>
    /// Throughput is a measurement of how much data we can push through the system at once.
    /// </summary>
    /// <remarks>
    /// Run with: <c>Benchmarks.exe  --strategy Throughput --unrollFactor 256</c>
    /// <code>
    /// BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
    /// 12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
    /// .NET SDK 10.0.103
    ///   [Host]     : .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
    ///   Job-BCRUGC : .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
    /// 
    /// RunStrategy=Throughput  UnrollFactor=256
    /// 
    /// | Method                | Mean         | Error      | StdDev     |
    /// |---------------------- |-------------:|-----------:|-----------:|
    /// | LisqueAdd             |     3.197 ns |  0.0643 ns |  0.0601 ns |
    /// | LisqueInsertStart     |     3.219 ns |  0.0722 ns |  0.0675 ns |
    /// | ListAdd               |     2.282 ns |  0.0447 ns |  0.0418 ns |
    /// | ListInsertStart       | 6,451.106 ns | 21.3884 ns | 18.9602 ns |
    /// | LinkedListAdd         |    63.922 ns |  1.3215 ns |  3.8966 ns |
    /// | LinkedListInsertStart |    67.163 ns |  1.3561 ns |  3.0884 ns |
    /// 
    /// // * Hints *
    /// Outliers
    ///   Throughput.ListInsertStart: RunStrategy=Throughput, UnrollFactor=256 -> 1 outlier  was  removed, 2 outliers were detected (6.41 us, 7.06 us)
    /// 
    /// // * Legends *
    ///   Mean   : Arithmetic mean of all measurements
    ///   Error  : Half of 99.9% confidence interval
    ///   StdDev : Standard deviation of all measurements
    ///   1 ns   : 1 Nanosecond (0.000000001 sec)
    /// </code>
    /// </remarks>
    public class Throughput
    {

        private Lisque<int>? _lisque;

        [IterationSetup(Target = nameof(LisqueAdd))]
        public void LisqueAddSetup()
        {
            _lisque = new(16);
        }
        [IterationCleanup(Target = nameof(LisqueAdd))]
        public void LisqueAddCleanup()
        {
            _lisque!.Clear();
        }
        [Benchmark]
        public void LisqueAdd() {
            _lisque!.Add(1);
        }

        [IterationSetup(Target = nameof(LisqueInsertStart))]
        public void LisqueInsertStartSetup()
        {
            _lisque = new(16);
        }
        [IterationCleanup(Target = nameof(LisqueInsertStart))]
        public void LisqueInsertStartCleanup()
        {
            _lisque!.Clear();
        }
        [Benchmark]
        public void LisqueInsertStart() {
            _lisque!.Insert(0, 1);
        }

        private List<int>? _list;

        [IterationSetup(Target = nameof(ListAdd))]
        public void ListAddSetup()
        {
            _list = new(16);
        }
        [IterationCleanup(Target = nameof(ListAdd))]
        public void ListAddCleanup()
        {
            _list!.Clear();
        }
        [Benchmark]
        public void ListAdd() {
            _list!.Add(1);
        }

        [IterationSetup(Target = nameof(ListInsertStart))]
        public void ListInsertStartSetup()
        {
            _list = new(16);
        }
        [IterationCleanup(Target = nameof(ListInsertStart))]
        public void ListInsertStartCleanup()
        {
            _list!.Clear();
        }
        [Benchmark]
        public void ListInsertStart() {
            _list!.Insert(0, 1);
        }

        private LinkedList<int>? _linkedList;

        [IterationSetup(Target = nameof(LinkedListAdd))]
        public void LinkedListAddSetup()
        {
            _linkedList = new();
        }
        [IterationCleanup(Target = nameof(LinkedListAdd))]
        public void LinkedListAddCleanup()
        {
            _linkedList!.Clear();
        }
        [Benchmark]
        public void LinkedListAdd() {
            _linkedList!.AddLast(1);
        }

        [IterationSetup(Target = nameof(LinkedListInsertStart))]
        public void LinkedListInsertStartSetup()
        {
            _linkedList = new();
        }
        [IterationCleanup(Target = nameof(LinkedListInsertStart))]
        public void LinkedListInsertStartCleanup()
        {
            _linkedList!.Clear();
        }
        [Benchmark]
        public void LinkedListInsertStart() {
            _linkedList!.AddFirst(1);
        }
    }
}
