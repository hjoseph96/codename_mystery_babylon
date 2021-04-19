using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace  Tazdraperm.Utility
{ 
    public struct PriorityQueue<T> 
    {
        public int Length { get; private set; }

        private int _capacity;
        private readonly Dictionary<T, int> _indexer;
        private Elem[] _minHeap;

        public struct Elem
        {
            public T Value;
            public float Priority;
        }

        // No safety checks!
        public float this[T key]
        {
            get => _minHeap[_indexer[key]].Priority;

            set
            {
                var position = _indexer[key];
                var old = _minHeap[position];
                var oldPriority = old.Priority;

                old.Priority = value;
                _minHeap[position] = old;

                if (oldPriority < value)
                {
                    SiftDown(position);
                }
                else
                {
                    SiftUp(position);
                }
            }
        }

        public PriorityQueue(int capacity)
        {
            _capacity = math.max(capacity, 2);
            Length = 0;

            _minHeap = new Elem[_capacity];
            _indexer = new Dictionary<T, int>();
        }

        public void Enqueue(T elem, float priority)
        {
            if (_capacity == Length)
            {
                var newHeap = new Elem[_capacity * 2];
                Array.Copy(_minHeap, newHeap, _capacity);

                _minHeap = newHeap;
                _capacity *= 2;
            }

            _minHeap[Length] = new Elem { Value = elem, Priority = priority };
            _indexer.Add(elem, Length);
            SiftUp(Length++);
        }

        public T Dequeue()
        {
            var result = _minHeap[0].Value;
            _indexer.Remove(result);
            _minHeap[0] = _minHeap[Length - 1];
            Length--;

            SiftDown(0);

            return result;
        }

        public bool Contains(T elem)
        {
            return _indexer.ContainsKey(elem);
        }

        private void SiftUp(int position)
        {
            while (position > 0)
            {
                var parent = (position + 1) / 2 - 1;
                if (_minHeap[position].Priority < _minHeap[parent].Priority)
                {
                    var temp = _minHeap[position];

                    _minHeap[position] = _minHeap[parent];
                    _minHeap[parent] = temp;

                    _indexer[_minHeap[position].Value] = position;
                    _indexer[_minHeap[parent].Value] = parent;

                    position = parent;
                }
                else
                {
                    break;
                }
            }
        }

        private void SiftDown(int position)
        {
            int child;

            while ((child = (position + 1) * 2 - 1) < Length)
            {
                if (child + 1 < Length && _minHeap[child + 1].Priority < _minHeap[child].Priority)
                {
                    child++;
                }

                if (_minHeap[position].Priority > _minHeap[child].Priority)
                {
                    var temp = _minHeap[position];

                    _minHeap[position] = _minHeap[child];
                    _minHeap[child] = temp;

                    _indexer[_minHeap[position].Value] = position;
                    _indexer[_minHeap[child].Value] = child;

                    position = child;
                }
                else
                {
                    break;
                }
            }
        }
    }
}