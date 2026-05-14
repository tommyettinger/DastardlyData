
using Dastardly.Data;

namespace Benchmarks {

    /// <summary> Throughput is a measurement of how much data we can push through the system at once. </summary>
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
