
using Dastardly.Data;

namespace Benchmarks {

    /// <summary> Throughput is a measurement of how much data we can push through the system at once. </summary>
    public class Throughput
    {

        private Lisque<int>? _lisque;

        [GlobalSetup(Target = nameof(LisqueAdd))]
        public void LisqueAddSetup()
        {
            if(_lisque == null) 
                _lisque = new(16);
            else if(_lisque.Count >= 10000)
                _lisque.Clear();
        }

        [Benchmark]
        public void LisqueAdd() {
            _lisque!.Add(1);
        }

        [GlobalSetup(Target = nameof(LisqueInsertStart))]
        public void LisqueInsertStartSetup()
        {
            if(_lisque == null) 
                _lisque = new(16);
            else if(_lisque.Count >= 10000)
                _lisque.Clear();
        }
        [Benchmark]
        public void LisqueInsertStart() {
            _lisque!.Insert(0, 1);
        }

        private List<int>? _list;

        [GlobalSetup(Target = nameof(ListAdd))]
        public void ListAddSetup()
        {
            if(_list == null) 
                _list = new(16);
            else if(_list.Count >= 10000)
                _list.Clear();
        }

        [Benchmark]
        public void ListAdd() {
            _list!.Add(1);
        }

        [GlobalSetup(Target = nameof(ListInsertStart))]
        public void ListInsertStartSetup()
        {
            if(_list == null) 
                _list = new(16);
            else if(_list.Count >= 10000)
                _list.Clear();
        }
        [Benchmark]
        public void ListInsertStart() {
            _list!.Insert(0, 1);
        }

        private LinkedList<int>? _linkedList;

        [GlobalSetup(Target = nameof(LinkedListAdd))]
        public void LinkedListAddSetup()
        {
            if(_linkedList == null) 
                _linkedList = new();
            else if(_linkedList.Count >= 10000)
                _linkedList.Clear();
        }
        [Benchmark]
        public void LinkedListAdd() {
            _linkedList!.AddLast(1);
        }

        [GlobalSetup(Target = nameof(LinkedListInsertStart))]
        public void LinkedListInsertStartSetup()
        {
            if(_linkedList == null) 
                _linkedList = new();
            else if(_linkedList.Count >= 10000)
                _linkedList.Clear();
        }

        [Benchmark]
        public void LinkedListInsertStart() {
            _linkedList!.AddFirst(1);
        }
    }
}
