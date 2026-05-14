
using Dastardly.Data;

namespace Benchmarks {

    /// <summary> Throughput is a measurement of how much data we can push through the system at once. </summary>
    public class Throughput
    {

        private Lisque<int> _lisque = null!;

        [GlobalSetup(Target = nameof(LisqueAdd))]
        public void LisqueAddSetup() => _lisque = new(16);
        [Benchmark]
        public void LisqueAdd() {
            _lisque.Add(1);
        }

        [GlobalSetup(Target = nameof(LisqueInsertStart))]
        public void LisqueInsertStartSetup() => _list = new(16);
        [Benchmark]
        public void LisqueInsertStart() {
            _list.Insert(0, 1);
        }

        private List<int> _list = null!;

        [GlobalSetup(Target = nameof(ListAdd))]
        public void ListAddSetup() => _list = new(16);
        [Benchmark]
        public void ListAdd() {
            _list.Add(1);
        }

        [GlobalSetup(Target = nameof(ListInsertStart))]
        public void ListInsertStartSetup() => _list = new(16);
        [Benchmark]
        public void ListInsertStart() {
            _list.Insert(0, 1);
        }

        private LinkedList<int> _linkedList = null!;

        [GlobalSetup(Target = nameof(LinkedListAdd))]
        public void LinkedListAddSetup() => _linkedList = new();
        [Benchmark]
        public void LinkedListAdd() {
            _linkedList.AddLast(1);
        }

        [GlobalSetup(Target = nameof(LinkedListInsertStart))]
        public void LinkedListInsertStartSetup() => _linkedList = new();
        [Benchmark]
        public void LinkedListInsertStart() {
            _linkedList.AddFirst(1);
        }
    }
}
