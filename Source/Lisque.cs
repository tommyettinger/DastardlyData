using System.Collections;
using System.Runtime.CompilerServices;

namespace Dastardly.Data;

public class Lisque<T> : ILisque<T>
{
    private const int DefaultCapacity = 4;

    private T[] _items;
    private int _size;
    private int _head;
    private int _tail;

    private int _version;

    public Lisque(int capacity = DefaultCapacity)
    {
        _items = new T[Math.Max(1, capacity)];
        _size = 0;
        _head = 0;
        _tail = 0;
        _version = 0;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    public void Add(T item)
    {
        PushLast(item);
    }

    public void Clear()
    {
        if (_size <= 0) return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (_head <= _tail)
                Array.Clear(_items, _head, _tail - _head + 1);
            else
            {
                Array.Clear(_items, _head, _items.Length - _head);
                Array.Clear(_items, 0, _tail + 1);
            }
        }

        _size = 0;
        _head = 0;
        _tail = 0;
        _version++;
    }

    public bool Contains(T item)
    {
        if (_size == 0) return false;
        if (_head <= _tail)
        {
            return Array.IndexOf(_items, item, _head, _size) >= 0;
        }

        return Array.IndexOf(_items, item, 0, _tail + 1) >= 0 ||
               Array.IndexOf(_items, item, _head, _items.Length - _head) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (_head <= _tail)
            Array.Copy(_items, _head, array, arrayIndex, _size);
        else
        {
            Array.Copy(_items, _head, array, arrayIndex, _items.Length - _head);
            Array.Copy(_items, 0, array, arrayIndex + _items.Length - _head, _tail + 1);
        }
    }

    public bool Remove(T item)
    {
        if (_size == 0) return false;
        int index;

        if (_head <= _tail)
        {
            index = Array.IndexOf(_items, item, _head, _size);
            if (index != -1) index -= _head;
        }
        else
        {
            index = Array.IndexOf(_items, item, _head, _items.Length - _head);
            if (index != -1)
            {
                index -= _head;
            }
            else
            {
                index = Array.IndexOf(_items, item, 0, _tail + 1);
                if (index != -1) index += _items.Length - _head;
            }
        }

        if (index == -1) return false;

        if (index == 0)
        {
            _items[_head] = default!;
            _head++;
            if (_head == _items.Length)
            {
                _head = 0;
            }

            if (--_size <= 1) _tail = _head;
        }
        else if (index >= _size - 1)
        {
            _items[_tail] = default!;

            if (_tail == 0)
            {
                _tail = _items.Length - 1;
            }
            else
            {
                --_tail;
            }

            if (--_size <= 1) _tail = _head;
        }
        else
        {
            index += _head;
            if (_head <= _tail)
            {
                // index is between head and tail.
                Array.Copy(_items, index + 1, _items, index, _tail - index);
                _items[_tail] = default!;
                _tail--;
                if (_tail == -1) _tail = _items.Length - 1;
            }
            else if (index >= _items.Length)
            {
                // index is between 0 and tail.
                index -= _items.Length;
                Array.Copy(_items, index + 1, _items, index, _tail - index);
                _items[_tail] = default!;
                _tail--;
                if (_tail == -1) _tail = _items.Length - 1;
            }
            else
            {
                // index is between head and values.length.
                Array.Copy(_items, _head, _items, _head + 1, index - _head);
                _items[_head] = default!;
                _head++;
                if (_head == _items.Length)
                {
                    _head = 0;
                }
            }

            _size--;
        }

        _version++;
        return true;
    }

    public int Count => _size;
    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        if (_size == 0) return -1;
        if (_head <= _tail)
        {
            var index = Array.IndexOf(_items, item, _head, _size);
            if (index == -1) return -1;
            return index - _head;
        }
        else
        {
            var index = Array.IndexOf(_items, item, _head, _items.Length - _head);
            if (index != -1) return index - _head;
            index = Array.IndexOf(_items, item, 0, _tail + 1);
            if (index == -1) return -1;
            return index + _items.Length - _head;
        }
    }

    public void Insert(int index, T item)
    {
        if (index <= 0)
            PushFirst(item);
        else if (index >= _size)
            PushLast(item);
        else
        {
            if (++_size > _items.Length)
            {
                Resize(_items.Length << 1);
            }

            if (_head <= _tail)
            {
                index += _head;
                if (index >= _items.Length) index -= _items.Length;
                var after = index + 1;
                if (after >= _items.Length) after = 0;

                Array.Copy(_items, index, _items, after, _head + _size - index - 1);
                _items[index] = item;
                _tail = _head + _size - 1;
                if (_tail >= _items.Length)
                {
                    _tail = 0;
                }
            }
            else
            {
                if (_head + index < _items.Length)
                {
                    // backward shift
                    Array.Copy(_items, _head, _items, _head - 1, index);
                    _items[_head - 1 + index] = item;
                    _head--;
                }
                else
                {
                    // forward shift
                    index = _head + index - _items.Length;
                    Array.Copy(_items, index, _items, index + 1, _tail - index + 1);
                    _items[index] = item;
                    _tail++;
                }
            }

            _version++;
        }
    }

    public void RemoveAt(int index)
    {
        if (_size == 0)
        {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        if (index <= 0)
        {
            _items[_head] = default!;
            _head++;
            if (_head == _items.Length)
            {
                _head = 0;
            }

            if (--_size <= 1) _tail = _head;
        }
        else if (index >= _size - 1)
        {
            _items[_tail] = default!;

            if (_tail == 0)
            {
                _tail = _items.Length - 1;
            }
            else
            {
                --_tail;
            }

            if (--_size <= 1) _tail = _head;
        }
        else
        {
            index += _head;
            if (_head <= _tail)
            {
                // index is between head and tail.
                Array.Copy(_items, index + 1, _items, index, _tail - index);
                _items[_tail] = default!;
                _tail--;
                if (_tail == -1) _tail = _items.Length - 1;
            }
            else if (index >= _items.Length)
            {
                // index is between 0 and tail.
                index -= _items.Length;
                Array.Copy(_items, index + 1, _items, index, _tail - index);
                _items[_tail] = default!;
                _tail--;
                if (_tail == -1) _tail = _items.Length - 1;
            }
            else
            {
                // index is between head and values.length.
                Array.Copy(_items, _head, _items, _head + 1, index - _head);
                _items[_head] = default!;
                _head++;
                if (_head == _items.Length)
                {
                    _head = 0;
                }
            }

            _size--;
        }

        _version++;
    }

    public T this[int index]
    {
        get
        {
            if (index <= 0)
                return _items[_head];
            if (index >= _size - 1)
                return _items[_tail];
            var i = _head + index;
            if (i >= _items.Length)
                i -= _items.Length;
            return _items[i];
        }

        set
        {
            if (_size <= 0 || index >= _size)
                PushLast(value);
            else if (index < 0)
                PushFirst(value);
            else
            {
                var i = _head + Math.Min(Math.Max(index, 0), _size - 1);
                if (i >= _items.Length)
                    i -= _items.Length;
                _items[i] = value;
            }
        }
    }

    public void PushFirst(T item)
    {
        if (_size == _items.Length)
        {
            Resize(_size << 1);
        }

        _head--;
        if (_head == -1) _head = _items.Length - 1;
        _items[_head] = item;

        if (++_size == 1) _tail = _head;
        _version++;
    }

    public void PushLast(T item)
    {
        if (_size == _items.Length)
        {
            Resize(_items.Length << 1);
        }

        if (++_size == 1) _tail = _head;
        else if (++_tail == _items.Length) _tail = 0;
        _items[_tail] = item;
        _version++;

    }

    public T PopFirst()
    {
        if (_size == 0)
        {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        var result = _items[_head];
        _items[_head] = default!;
        _head++;
        if (_head == _items.Length)
        {
            _head = 0;
        }

        if (--_size <= 1) _tail = _head;
        _version++;

        return result;

    }

    public T PopLast()
    {
        if (_size == 0)
        {
            throw new InvalidOperationException("Lisque is empty.");
        }

        var result = _items[_tail];
        _items[_tail] = default!;

        if (_tail == 0)
        {
            _tail = _items.Length - 1;
        }
        else
        {
            --_tail;
        }

        if (--_size <= 1) _tail = _head;
        _version++;

        return result;
    }

    public T PopAt(int index)
    {
        if (index <= 0)
            return PopFirst();
        if (index >= _size - 1)
            return PopLast();

        index += _head;
        T value;
        if (_head <= _tail)
        {
            // index is between head and tail.
            value = _items[index];
            Array.Copy(_items, index + 1, _items, index, _tail - index);
            _items[_tail] = default!;
            _tail--;
            if (_tail == -1) _tail = _items.Length - 1;
        }
        else if (index >= _items.Length)
        {
            // index is between 0 and tail.
            index -= _items.Length;
            value = _items[index];
            Array.Copy(_items, index + 1, _items, index, _tail - index);
            _items[_tail] = default!;
            _tail--;
            if (_tail == -1) _tail = _items.Length - 1;
        }
        else
        {
            // index is between head and values.length.
            value = _items[index];
            Array.Copy(_items, _head, _items, _head + 1, index - _head);
            _items[_head] = default!;
            _head++;
            if (_head == _items.Length)
            {
                _head = 0;
            }
        }

        _size--;
        _version++;
        return value;
    }

    public T First
    {
        get => _size == 0 ? throw new InvalidOperationException("Lisque is empty.") : _items[_head];
        set
        {
            if (_size == 0) throw new InvalidOperationException("Lisque is empty.");
            _items[_head] = value;
        }
    }

    public T Last
    {
        get => _size == 0 ? throw new InvalidOperationException("Lisque is empty.") : _items[_tail];
        set
        {
            if (_size == 0) throw new InvalidOperationException("Lisque is empty.");
            _items[_tail] = value;
        }
    }

    /// <summary>
    /// Resizes the backing array. 
    /// </summary>
    /// <remarks>
    /// The newSize should be greater than the current size; otherwise, newSize will be set to
    /// size and the resize to the same size will (for most purposes) be wasted effort. If this is not empty, this will
    /// rearrange the items internally to be linear and have the head at index 0, with the tail at {@code size - 1}.
    /// This always allocates a new internal backing array.
    /// </remarks>
    /// <param name="newSize">The new capacity for the backing array.</param>
    protected void Resize(int newSize)
    {
        if (newSize < _size)
            newSize = _size;

        var newArray = new T[Math.Max(1, newSize)];

        if (_size > 0)
        {
            if (_head <= _tail)
            {
                // Continuous
                Array.Copy(_items, _head, newArray, 0, _tail - _head + 1);
            }
            else
            {
                // Wrapped
                var rest = _items.Length - _head;
                Array.Copy(_items, _head, newArray, 0, rest);
                Array.Copy(_items, 0, newArray, rest, _tail + 1);
            }

            _head = 0;
            _tail = _size - 1;
        }

        _items = newArray;
        _version++;
    }

    public struct Enumerator : IEnumerator<T>, IEnumerator
    {
        private readonly Lisque<T> _lisque;
        private readonly int _version;

        private int _index;
        private T? _current;

        internal Enumerator(Lisque<T> lisque)
        {
            _lisque = lisque;
            _version = lisque._version;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            Lisque<T> localLisque = _lisque;

            if (_version != _lisque._version)
            {
                throw new InvalidOperationException("Collection was modified after enumerator was created.");
            }

            if ((uint)_index < (uint)localLisque._size)
            {
                _current = localLisque[_index];
                _index++;
                return true;
            }

            _current = default;
            _index = -1;
            return false;
        }

        public T Current => _current!;

        object? IEnumerator.Current
        {
            get
            {
                if (_index <= 0)
                {
                    throw new InvalidOperationException("Enumerator has completed; another item cannot be retrieved.");
                }

                return _current;
            }
        }

        void IEnumerator.Reset()
        {
            if (_version != _lisque._version)
            {
                throw new InvalidOperationException("Collection was modified after enumerator was created.");
            }

            _index = 0;
            _current = default;
        }
    }
}