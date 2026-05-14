
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
    ///   Job-QBACWG : .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
    /// 
    /// RunStrategy=Throughput  UnrollFactor=256
    /// 
    /// | Method                | Mean         | Error      | StdDev     |
    /// |---------------------- |-------------:|-----------:|-----------:|
    /// | LisqueAdd             |     3.175 ns |  0.0546 ns |  0.0511 ns |
    /// | LisqueInsertStart     |     3.185 ns |  0.0595 ns |  0.0556 ns |
    /// | LisqueInsertRandom    | 3,271.960 ns | 15.8430 ns | 14.0444 ns |
    /// | ListAdd               |     2.138 ns |  0.0343 ns |  0.0304 ns |
    /// | ListInsertStart       | 6,465.797 ns | 28.4104 ns | 26.5751 ns |
    /// | ListInsertRandom      | 3,235.046 ns | 11.6599 ns | 10.9067 ns |
    /// | LinkedListAdd         |    67.200 ns |  1.3713 ns |  2.7700 ns |
    /// | LinkedListInsertStart |    65.960 ns |  1.3467 ns |  3.0397 ns |
    /// 
    /// // * Hints *
    /// Outliers
    ///   Throughput.LisqueInsertRandom: RunStrategy=Throughput, UnrollFactor=256    -> 1 outlier  was  removed, 2 outliers were detected (3.24 us, 3.40 us)
    ///   Throughput.ListAdd: RunStrategy=Throughput, UnrollFactor=256               -> 1 outlier  was  removed (3.48 ns)
    ///   Throughput.ListInsertRandom: RunStrategy=Throughput, UnrollFactor=256      -> 1 outlier  was  detected (3.22 us)
    ///   Throughput.LinkedListAdd: RunStrategy=Throughput, UnrollFactor=256         -> 1 outlier  was  detected (61.37 ns)
    ///   Throughput.LinkedListInsertStart: RunStrategy=Throughput, UnrollFactor=256 -> 1 outlier  was  removed, 3 outliers were detected (60.35 ns, 60.59 ns, 76.48 ns)
    /// 
    /// // * Legends *
    ///   Mean   : Arithmetic mean of all measurements
    ///   Error  : Half of 99.9% confidence interval
    ///   StdDev : Standard deviation of all measurements
    ///   1 ns   : 1 Nanosecond (0.000000001 sec)
    /// </code>
    /// </remarks>
    public class ThroughputAdd
    {

        private Random? _random;
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

        [IterationSetup(Target = nameof(LisqueInsertRandom))]
        public void LisqueInsertRandomSetup()
        {
            _lisque = new Lisque<int>(16) { 2, 3, 4 };
            _random = new Random();
        }
        [IterationCleanup(Target = nameof(LisqueInsertRandom))]
        public void LisqueInsertRandomCleanup()
        {
            _lisque!.Clear();
            _random = null;
        }
        [Benchmark]
        public void LisqueInsertRandom() {
            _lisque!.Insert(_random!.Next(_lisque!.Count), 1);
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

        [IterationSetup(Target = nameof(ListInsertRandom))]
        public void ListInsertRandomSetup()
        {
            _list = new List<int>(16) { 2, 3, 4};
            _random = new Random();
        }
        [IterationCleanup(Target = nameof(ListInsertRandom))]
        public void ListInsertRandomCleanup()
        {
            _list!.Clear();
            _random = null;
        }
        [Benchmark]
        public void ListInsertRandom() {
            _list!.Insert(_random!.Next(_list!.Count), 1);
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
    
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Run with: <c>Benchmarks.exe  --strategy Throughput --unrollFactor 1024</c>
    /// <code>
    /// BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3880/23H2/2023Update/SunValley3)
    /// 12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
    /// .NET SDK 10.0.103
    ///   [Host]     : .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
    ///   Job-RMIZRO : .NET 8.0.24 (8.0.2426.7010), X64 RyuJIT AVX2
    /// 
    /// RunStrategy=Throughput  UnrollFactor=1024
    /// 
    /// | Method             | Mean      | Error     | StdDev    | Median    |
    /// |------------------- |----------:|----------:|----------:|----------:|
    /// | LisqueGetStart     | 0.1086 ns | 0.0083 ns | 0.0069 ns | 0.1116 ns |
    /// | LisqueGetEnd       | 0.1482 ns | 0.0078 ns | 0.0073 ns | 0.1496 ns |
    /// | LisqueGetRandom    | 4.3559 ns | 0.0457 ns | 0.0428 ns | 4.3712 ns |
    /// | ListGetStart       | 0.0008 ns | 0.0018 ns | 0.0017 ns | 0.0000 ns |
    /// | ListGetEnd         | 0.0076 ns | 0.0074 ns | 0.0066 ns | 0.0079 ns |
    /// | ListGetRandom      | 1.9822 ns | 0.0416 ns | 0.0369 ns | 1.9834 ns |
    /// | LinkedListGetStart | 0.0005 ns | 0.0014 ns | 0.0012 ns | 0.0000 ns |
    /// | LinkedListGetEnd   | 0.0390 ns | 0.0207 ns | 0.0194 ns | 0.0315 ns |
    /// 
    /// // * Warnings *
    /// MultimodalDistribution
    ///   ThroughputGet.LisqueGetEnd: RunStrategy=Throughput, UnrollFactor=1024 -> It seems that the distribution is bimodal (mValue = 3.67)
    /// ZeroMeasurement
    ///   ThroughputGet.ListGetStart: RunStrategy=Throughput, UnrollFactor=1024       -> The method duration is indistinguishable from the empty method duration
    ///   ThroughputGet.ListGetEnd: RunStrategy=Throughput, UnrollFactor=1024         -> The method duration is indistinguishable from the empty method duration
    ///   ThroughputGet.LinkedListGetStart: RunStrategy=Throughput, UnrollFactor=1024 -> The method duration is indistinguishable from the empty method duration
    /// 
    /// // * Hints *
    /// Outliers
    ///   ThroughputGet.LisqueGetStart: RunStrategy=Throughput, UnrollFactor=1024     -> 2 outliers were removed (1.43 ns, 1.59 ns)
    ///   ThroughputGet.ListGetEnd: RunStrategy=Throughput, UnrollFactor=1024         -> 1 outlier  was  removed (1.31 ns)
    ///   ThroughputGet.ListGetRandom: RunStrategy=Throughput, UnrollFactor=1024      -> 1 outlier  was  removed (3.64 ns)
    ///   ThroughputGet.LinkedListGetStart: RunStrategy=Throughput, UnrollFactor=1024 -> 1 outlier  was  removed (1.28 ns)
    ///   ThroughputGet.LinkedListGetEnd: RunStrategy=Throughput, UnrollFactor=1024   -> 1 outlier  was  removed (1.34 ns)
    /// 
    /// // * Legends *
    ///   Mean   : Arithmetic mean of all measurements
    ///   Error  : Half of 99.9% confidence interval
    ///   StdDev : Standard deviation of all measurements
    ///   Median : Value separating the higher half of all measurements (50th percentile)
    ///   1 ns   : 1 Nanosecond (0.000000001 sec)
    /// </code>
    /// </remarks>
    public class ThroughputGet
    {

        private Random? _random;
        private Lisque<int>? _lisque;

        [IterationSetup(Target = nameof(LisqueGetStart))]
        public void LisqueGetStartSetup()
        {
            _lisque = new Lisque<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
        [IterationCleanup(Target = nameof(LisqueGetStart))]
        public void LisqueGetStartCleanup()
        {
            _lisque!.Clear();
        }
        [Benchmark]
        public void LisqueGetStart() {
            int item = _lisque![0];
        }

        [IterationSetup(Target = nameof(LisqueGetEnd))]
        public void LisqueGetEndSetup()
        {
            _lisque = new Lisque<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
        [IterationCleanup(Target = nameof(LisqueGetEnd))]
        public void LisqueGetEndCleanup()
        {
            _lisque!.Clear();
        }
        [Benchmark]
        public void LisqueGetEnd() {
            int item = _lisque![9];
        }

        [IterationSetup(Target = nameof(LisqueGetRandom))]
        public void LisqueGetRandomSetup()
        {
            _lisque = new Lisque<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _random = new Random();
        }
        [IterationCleanup(Target = nameof(LisqueGetRandom))]
        public void LisqueGetRandomCleanup()
        {
            _lisque!.Clear();
            _random = null;
        }
        [Benchmark]
        public void LisqueGetRandom() {
            int item = _lisque![_random!.Next(_lisque!.Count)];
        }

        private List<int>? _list;

        [IterationSetup(Target = nameof(ListGetStart))]
        public void ListGetStartSetup()
        {
            _list = new List<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
        [IterationCleanup(Target = nameof(ListGetStart))]
        public void ListGetStartCleanup()
        {
            _list!.Clear();
        }
        [Benchmark]
        public void ListGetStart() {
            int item = _list![0];
        }

        [IterationSetup(Target = nameof(ListGetEnd))]
        public void ListGetEndSetup()
        {
            _list = new List<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
        [IterationCleanup(Target = nameof(ListGetEnd))]
        public void ListGetEndCleanup()
        {
            _list!.Clear();
        }
        [Benchmark]
        public void ListGetEnd() {
            int item = _list![9];
        }

        [IterationSetup(Target = nameof(ListGetRandom))]
        public void ListGetRandomSetup()
        {
            _list = new List<int>(16) { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            _random = new Random();
        }
        [IterationCleanup(Target = nameof(ListGetRandom))]
        public void ListGetRandomCleanup()
        {
            _list!.Clear();
            _random = null;
        }
        [Benchmark]
        public void ListGetRandom() {
            int item = _list![_random!.Next(_list!.Count)];
        }

        private LinkedList<int>? _linkedList;

        [IterationSetup(Target = nameof(LinkedListGetStart))]
        public void LinkedListGetStartSetup()
        {
            _linkedList = new LinkedList<int>();
            _linkedList.AddLast(0);
            _linkedList.AddLast(1);
            _linkedList.AddLast(2);
            _linkedList.AddLast(3);
            _linkedList.AddLast(4);
            _linkedList.AddLast(5);
            _linkedList.AddLast(6);
            _linkedList.AddLast(7);
            _linkedList.AddLast(8);
            _linkedList.AddLast(9);
        }
        [IterationCleanup(Target = nameof(LinkedListGetStart))]
        public void LinkedListGetStartCleanup()
        {
            _linkedList!.Clear();
        }
        [Benchmark]
        public void LinkedListGetStart() {
            int item = _linkedList!.First.Value;
        }

        [IterationSetup(Target = nameof(LinkedListGetEnd))]
        public void LinkedListGetEndSetup()
        {
            _linkedList = new LinkedList<int>();
            _linkedList.AddLast(0);
            _linkedList.AddLast(1);
            _linkedList.AddLast(2);
            _linkedList.AddLast(3);
            _linkedList.AddLast(4);
            _linkedList.AddLast(5);
            _linkedList.AddLast(6);
            _linkedList.AddLast(7);
            _linkedList.AddLast(8);
            _linkedList.AddLast(9);
        }
        [IterationCleanup(Target = nameof(LinkedListGetEnd))]
        public void LinkedListGetEndCleanup()
        {
            _linkedList!.Clear();
        }
        [Benchmark]
        public void LinkedListGetEnd() {
            int item = _linkedList!.Last.Value;
        }
    }
}
