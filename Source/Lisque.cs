using System.Collections;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Dastardly.Data;

public class Lisque<T> : ILisque<T>, IEquatable<Lisque<T>>
{
    private const int DefaultCapacity = 4;

    private T[] _items;
    private int _size;
    private int _head;
    private int _tail;

    private int _version;

    /// <summary>
    /// Initializes a new instance of the Lisque{T} class that is empty and has the given capacity,
    /// or a capacity of 4 if not supplied.
    /// </summary>
    /// <param name="capacity">How many elements the lisque will be able to hold before resizing.</param>
    public Lisque(int capacity = DefaultCapacity)
    {
        _items = new T[Math.Max(1, capacity)];
        _size = 0;
        _head = 0;
        _tail = 0;
        _version = 0;
    }

    /// <summary>
    /// Initializes a new instance of the Lisque{T} class that contains elements copied from the specified
    /// collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new lisque.</param>
    /// <exception cref="ArgumentNullException">collection is null.</exception>
    public Lisque(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if (count == 0)
            {
                _items = new T[DefaultCapacity];
            }
            else
            {
                _items = new T[count];
                c.CopyTo(_items, 0);
                _size = count;
                _head = 0;
                _tail = count - 1;
            }
        }
        else
        {
            _items = new T[DefaultCapacity];
            foreach (var t in collection)
            {
                Add(t);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return new Enumerator(this);
    }

    /// <summary>
    /// Adds an item to the lisque at the end. This is equivalent to <see cref="PushLast(T)"/>.
    /// </summary>
    /// <param name="item">The object to be added to the end of the lisque.</param>
    public void Add(T item)
    {
        PushLast(item);
    }

    /// <summary>
    /// Adds every T in the given IEnumerable to this Lisque at the beginning, keeping the same order.
    /// </summary>
    /// <remarks>
    /// This is significantly more efficient when the given parameter implements ICollection.
    /// For an ICollection parameter, this operates in <c>O(n + m)</c> time, where n is the size of
    /// this Lisque and m is the Count of collection. If the parameter only implements IEnumerable,
    /// performance degrades to <c>O(nm)</c> time because this doesn't know how many T items there
    /// will be in the IEnumerable.
    /// </remarks>
    /// <param name="collection">An IEnumerable of T or, preferably, an ICollection of T.</param>
    public void AddRangeFirst(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is ICollection<T> c)
        {
            var cs = c.Count;
            if (cs == 0) return;
            var oldSize = _size;
            EnsureCapacity(cs);
            if (ReferenceEquals(c, this)) {
                if (_head <= _tail) {
                    if (_head >= oldSize)
                        Array.Copy(_items, _head, _items, _head - oldSize, oldSize);
                    else if (_head > 0) {
                        Array.Copy(_items, _tail + 1 - _head, _items, 0, _head);
                        Array.Copy(_items, _head, _items, _items.Length - (oldSize - _head), oldSize - _head);
                    } else {
                        Array.Copy(_items, _head, _items, _items.Length - oldSize, oldSize);
                    }
                } else {
                    Array.Copy(_items, _head, _items, _head - oldSize, _items.Length - _head);
                    Array.Copy(_items, 0, _items, _items.Length - oldSize, _tail + 1);
                }
                _head -= oldSize;
                if (_head < 0) _head += _items.Length;
                _size += oldSize;
                _version += oldSize;
            } else {
                var i = EnsureGap(0, cs);
                foreach (var t in c) {
                    _items[i++] = t;
                    if (i == _items.Length) i = 0;
                }
                _size += cs;
                _version += cs;
            }
        }
        else
        {
            var index = 0;
            foreach (var t in collection)
            {
                Insert(index++, t);
            }
        }
    }

    /// <summary>
    /// Adds every T in the given IEnumerable to this Lisque at the end, keeping the same order.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="AddRangeFirst"/>, this performs in <c>O(m)</c> time,
    /// where n is the size of this Lisque and m is the Count of collection, unless the capacity
    /// must be increased. Then, it performs in <c>O(n + m)</c> time. This simply calls
    /// <see cref="AddRange"/> with the same parameter.
    /// </remarks>
    /// <param name="collection">An IEnumerable of T or, preferably, an ICollection of T.</param>
    public void AddRangeLast(IEnumerable<T> collection)
    {
        AddRange(collection);
    }
    
    /// <summary>
    /// Adds every T in the given IEnumerable to this Lisque at the end, keeping the same order.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="AddRangeFirst"/>, this performs in <c>O(m)</c> time,
    /// where n is the size of this Lisque and m is the Count of collection, unless the capacity
    /// must be increased. Then, it performs in <c>O(n + m)</c> time.
    /// </remarks>
    /// <param name="collection">An IEnumerable of T or, preferably, an ICollection of T.</param>
    public void AddRange(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is ICollection<T> c)
        {
            var cs = c.Count;
            if (cs == 0) return;
            var oldSize = _size;
            EnsureCapacity(cs);
            if (ReferenceEquals(collection, this))
            {
                if (_head <= _tail) {
                    if (_tail + 1 < _items.Length)
                        Array.Copy(_items, _head, _items, _tail + 1, Math.Min(_size, _items.Length - _tail - 1));
                    if (_items.Length - _tail - 1 < _size)
                        Array.Copy(_items, _head + _items.Length - _tail - 1, _items, 0, _size - (_items.Length - _tail - 1));
                } else {
                    Array.Copy(_items, _head, _items, _tail + 1, _items.Length - _head);
                    Array.Copy(_items, 0, _items, _tail + 1 + _items.Length - _head, _tail + 1);
                }
                _tail += oldSize;
                _size += oldSize;
                _version += cs;
            } else {
                foreach (var t in c)
                {
                    Add(t);
                }
            }
        }
        else
        {
            foreach (var t in collection)
            {
                Add(t);
            }

        }
    }
    
    /// <summary>
    /// Inserts the elements of a collection into the lisque at the specified index.
    /// </summary>
    /// <remarks>
    /// Like <see cref="AddRangeFirst"/>, this performs much better when given an ICollection, not just an IEnumerable.
    /// For an ICollection parameter, this operates in <c>O(n + m)</c> time,
    /// where n is the size of this Lisque and m is the Count of collection.
    /// If the parameter only implements IEnumerablem performance degrades to <c>O(nm)</c> time because this doesn't
    /// know how many T items there will be in the IEnumerable.
    /// </remarks>
    /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
    /// <param name="collection">An IEnumerable of T or, preferably, an ICollection of T.</param>
    /// <exception cref="ArgumentNullException">The given collection is null.</exception>
    public void InsertRange(int index, IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var oldSize = _size;
        if (index <= 0)
            AddRangeFirst(collection);
        else if (index >= oldSize)
            AddRangeLast(collection);
        else
        {
            if (collection is ICollection<T> c)
            {
                var cs = c.Count;
                if (cs == 0) return;
                var place = EnsureGap(index, cs);
                if (ReferenceEquals(collection, this))
                {
                    Array.Copy(_items, _head, _items, place, place - _head);
                    Array.Copy(_items, place + cs, _items, place + place - _head, _tail + 1 - place - cs);
                } else {
                    foreach (var item in c)
                    {
                        _items[place++] = item;
                        if (place >= _items.Length) place -= _items.Length;
                    }
                }
                _size += cs;
                _version += cs;
            }
            else
            {
                foreach (var item in collection)
                {
                    Insert(index++, item);
                }
            }
        }
    }

    /// <summary>
    /// Removes all items from this lisque and sets its size to 0. Its capacity does not change.
    /// </summary>
    /// <remarks>
    /// If the item type is a reference type, this performs in O(n) time.
    /// If the item type is a value type, this performs in O(1) time.
    /// </remarks>
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

    /// <summary>
    /// Returns true if this lisque contains the given item, or false otherwise.
    /// </summary>
    /// <remarks>
    /// This runs in O(n) time. It uses <see cref="Array.IndexOf(Array, object?, int, int)"/> to find the item,
    /// if present.
    /// </remarks>
    /// <param name="item">The item to search for.</param>
    /// <returns>true if the given item is present in this lisque, or false otherwise.</returns>
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

    /// <summary>
    /// Returns true if any item exists in this lisque that matches the given Predicate, or false if none match it.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>true if any item returns true for the given Predicate, or false if none return true for it.</returns>
    public bool Exists(Predicate<T> match)
        => FindIndex(match) != -1;
    
    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the
    /// first occurrence within the entire lisque.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The first element that matches the conditions defined by the specified predicate, if found;
    /// otherwise, the default value for type T.</returns>
    public T? Find(Predicate<T> match)
    {
        for (int i = _head, ii = 0; ii < _size; ii++)
        {
            if (match(_items[i]))
            {
                return _items[i];
            }

            ++i;
            if (i >= _items.Length) i = 0;
        }
        return default;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the
    /// zero-based index of the first occurrence within the entire lisque, or -1 if no items match the predicate.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first matching item, or -1 if no item matches.</returns>
    public int FindIndex(Predicate<T> match)
        => FindIndex(0, _size, match);

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate at or after the given
    /// startIndex. Returns the zero-based index of the first occurrence within the entire lisque, or -1 if no items
    /// match the predicate in the specified range.
    /// </summary>
    /// <param name="startIndex">The zero-based first index to start the search from.</param>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first matching item, or -1 if no item matches.</returns>
    public int FindIndex(int startIndex, Predicate<T> match)
        => FindIndex(startIndex, _size - startIndex, match);

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate at or after the given
    /// startIndex, searching up to the provided count of items. Returns the zero-based index of the first occurrence
    /// within the entire lisque, or -1 if no items match the predicate in the specified range.
    /// </summary>
    /// <param name="startIndex">The zero-based first index to start the search from.</param>
    /// <param name="count">How many items to search through, at most.</param>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the first matching item, or -1 if no item matches.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If startIndex is larger than the Count of this lisque, if the
    /// count parameter is negative, or there are not enough items remaining to satisfy the count.</exception>
    public int FindIndex(int startIndex, int count, Predicate<T> match)
    {
        if ((uint)startIndex > (uint)_size)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is too large.");
        }

        if (count < 0 || startIndex > _size - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "count is invalid.");
        }

        var wrap = startIndex + _head;
        if (wrap >= _items.Length) wrap -= _items.Length;
        var end = startIndex + count;
        for (int i = wrap, ii = startIndex; ii < end; ii++)
        {
            if (match(_items[i]))
            {
                return ii;
            }

            ++i;
            if (i >= _items.Length) i = 0;
        }
        return -1;
    }
    
    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the
    /// last occurrence within the entire lisque.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The last element that matches the conditions defined by the specified predicate, if found;
    /// otherwise, the default value for type T.</returns>
    public T? FindLast(Predicate<T> match)
    {
        for (int i = _tail, ii = 0; ii < _size; ii++)
        {
            if (match(_items[i]))
            {
                return _items[i];
            }

            --i;
            if (i < 0) i = _items.Length - 1;
        }
        return default;
    }

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the
    /// zero-based index of the last occurrence within the entire lisque, or -1 if no items match the predicate.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last matching item, or -1 if no item matches.</returns>
    public int FindLastIndex(Predicate<T> match)
        => FindLastIndex(_size - 1, _size, match);
    
    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate at or before the given
    /// startIndex. Returns the zero-based index of the last occurrence within the entire lisque, or -1 if no items
    /// match the predicate in the specified range.
    /// </summary>
    /// <param name="startIndex">The zero-based last index to start the end-to-start search from.</param>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last matching item, or -1 if no item matches.</returns>
    public int FindLastIndex(int startIndex, Predicate<T> match)
        => FindLastIndex(startIndex, startIndex + 1, match);

    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate at or before the given
    /// startIndex, searching up to the provided count of items. Returns the zero-based index of the last occurrence
    /// within the entire lisque, or -1 if no items match the predicate in the specified range.
    /// </summary>
    /// <param name="startIndex">The zero-based last index to start the end-to-start search from.</param>
    /// <param name="count">How many items to search through, at most.</param>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The zero-based index of the last matching item, or -1 if no item matches.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If startIndex is larger than the Count of this lisque, if the
    /// count parameter is negative, or there are not enough items remaining to satisfy the count.</exception>
    public int FindLastIndex(int startIndex, int count, Predicate<T> match)
    {
        if ((uint)startIndex >= (uint)_size)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), "startIndex is too large.");
        }

        if (count < 0 || startIndex > _size - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "count is invalid.");
        }

        var wrap = startIndex + _tail;
        if (wrap >= _items.Length) wrap -= _items.Length;
        var end = startIndex - count;
        for (int i = wrap, ii = startIndex; ii > end; ii--)
        {
            if (match(_items[i]))
            {
                return ii;
            }

            --i;
            if (i < 0) i = _items.Length - 1;
        }
        return -1;
    }

    /// <summary>
    /// Retrieves all the elements that match the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the elements to search for.</param>
    /// <returns>A lisque of T containing all the elements that match the conditions defined by the specified
    /// predicate, if found; otherwise, an empty lisque.</returns>
    public Lisque<T> FindAll(Predicate<T> match)
    {
        var lisque = new Lisque<T>();
        for (int i = _head, ii = 0; ii < _size; ii++)
        {
            if (match(_items[i]))
            {
                lisque.Add(_items[i]);
            }

            ++i;
            if (i >= _items.Length) i = 0;
        }

        return lisque;
    }

    /// <summary>
    /// Copies the entire lisque to a compatible one-dimensional array, starting
    /// at the specified index of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from
    /// this lisque. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
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

    /// <summary>
    /// Copies a range of elements from the List{T} to a compatible one-dimensional array, starting
    /// at the specified index of the target array.
    /// </summary>
    /// <param name="index">The zero-based index in the source lisque at which copying begins.</param>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied
    /// from this lisque. The Array must have zero-based indexing.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    /// <param name="count">The number of elements to copy.</param>
    public void CopyTo(int index, T[] array, int arrayIndex, int count)
    {
        if (_head <= _tail)
            Array.Copy(_items, _head + index, array, arrayIndex, count);
        else
        {
            var headCount = Math.Min(count, _items.Length - _head - index);
            var tailCount = count - headCount;
            if (_head + index < _items.Length)
            {
                Array.Copy(_items, _head + index, array, arrayIndex, headCount);
                if (tailCount > 0)
                    Array.Copy(_items, 0, array, arrayIndex + _items.Length - _head, tailCount);
            }
            else
            {
                Array.Copy(_items, _head + index - _items.Length, array, arrayIndex, count);
            }
        }
    }

    /// <summary>
    /// Copies the entire lisque to a compatible one-dimensional array, starting
    /// at the beginning of the target array.
    /// </summary>
    /// <param name="array">The one-dimensional Array that is the destination of the elements copied from
    /// this lisque. The Array must have zero-based indexing.</param>
    public void CopyTo(T[] array) => CopyTo(array, 0);

    /// <summary>
    /// Copies the elements of the lisque to a new array.
    /// </summary>
    /// <returns>An array containing copies of the elements of the lisque.</returns>
    public T[] ToArray()
    {
        var array = new T[_size];
        if (_head <= _tail)
            Array.Copy(_items, _head, array, 0, _size);
        else
        {
            Array.Copy(_items, _head, array, 0, _items.Length - _head);
            Array.Copy(_items, 0, array, 0 + _items.Length - _head, _tail + 1);
        }

        return array;
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
                // index is between head and values.Length.
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


    /// <summary>
    /// Removes the last occurrence of a specific object from the lisque.
    /// </summary>
    /// <param name="item">The object to remove from the lisque.</param>
    /// <returns>true if item was successfully removed from the lisque; otherwise, false.
    /// This method also returns false if item is not found in the original lisque.</returns>
    public bool RemoveLast(T item)
    {
        if (_size == 0) return false;
        var index = LastIndexOf(item);
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
                // index is between head and values.Length.
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

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire
    /// lisque, or -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <returns>The zero-based index of the first occurrence of the given item, or -1 if not found.</returns>
    public int IndexOf(T item)
    {
        if (_size == 0) return -1;
        if (_head <= _tail)
        {
            var idx = Array.IndexOf(_items, item, _head, _size);
            if (idx == -1) return -1;
            return idx - _head;
        }
        else
        {
            var idx = Array.IndexOf(_items, item, _head, _items.Length - _head);
            if (idx != -1) return idx - _head;
            idx = Array.IndexOf(_items, item, 0, _tail + 1);
            if (idx == -1) return -1;
            return idx + _items.Length - _head;
        }
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the lisque that extends from the specified index to the last element, or -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <param name="index">The zero-based first index to search in.</param>
    /// <returns>The zero-based index of the first occurrence of the given item, or -1 if not found.</returns>
    public int IndexOf(T item, int index)
    {
        return IndexOf(item, index, _size - index);
    }
    
    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the first occurrence within the range
    /// of elements in the lisque that extends from the specified index for the specified count of items, or
    /// -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <param name="index">The zero-based first index to search in.</param>
    /// <param name="count">How many items to search through.</param>
    /// <returns>The zero-based index of the first occurrence of the given item, or -1 if not found.</returns>
    public int IndexOf(T item, int index, int count)
    {
        if (index > _size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "index argument cannot be greater than the size of the collection, or negative.");
        if (count < 0 || index > _size - count)
            throw new ArgumentOutOfRangeException(nameof(count), "count argument is invalid.");
        if (_size == 0) return -1;
        if (_head <= _tail)
        {
            var idx = Array.IndexOf(_items, item, _head + index, count);
            if (idx == -1) return -1;
            return idx - _head;
        }
        else
        {
            var h = _head + index;
            if (h >= _items.Length) h -= _items.Length;
            var cnt = Math.Min(count, _items.Length - h);
            var idx = Array.IndexOf(_items, item, h, cnt);
            if (idx != -1)
            {
                idx -= _head;
                if(idx < 0) idx += _items.Length;
                return idx;
            }
            if (count <= cnt) return -1;
            idx = Array.IndexOf(_items, item, 0, Math.Min(_tail + 1, count - cnt));
            if (idx == -1) return -1;
            return idx + _items.Length - _head;
        }
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the last occurrence within the entire
    /// lisque, or -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <returns>The zero-based index of the last occurrence of the given item, or -1 if not found.</returns>
    public int LastIndexOf(T item)
    {
        return LastIndexOf(item, _size - 1, _size);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the last occurrence within the range
    /// of elements in the lisque that extends from the specified index to the first element, or -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <param name="index">The zero-based last index to search in, going toward the start of the lisque.</param>
    /// <returns>The zero-based index of the last occurrence of the given item, or -1 if not found.</returns>
    public int LastIndexOf(T item, int index)
    {
        return LastIndexOf(item, index, index + 1);
    }

    /// <summary>
    /// Searches for the specified object and returns the zero-based index of the last occurrence within the range
    /// of elements in the lisque that extends from the specified index for the specified count of items toward the
    /// start of the lisque, or -1 if not found.
    /// </summary>
    /// <param name="item">The object to locate in the lisque.</param>
    /// <param name="index">The zero-based last index to search in, going toward the start of the lisque.</param>
    /// <param name="count">How many items to search through, from index going toward the start of the lisque.</param>
    /// <returns>The zero-based index of the last occurrence of the given item, or -1 if not found.</returns>
    public int LastIndexOf(T item, int index, int count)
    {
        if (index > _size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "index argument cannot be greater than the size of the collection, or negative.");
        if (count < 0 || count > index + 1)
            throw new ArgumentOutOfRangeException(nameof(count), "count argument is invalid.");
        if (_size == 0) return -1;
        if (_head <= _tail)
        {
            var idx = Array.LastIndexOf(_items, item, _head + index, count);
            if (idx == -1) return -1;
            return idx - _head;
        }
        else
        {
            var i = _head + index;
            if(i >= _items.Length) i -= _items.Length;
            var cnt = i - count;
            int idx;
            if(cnt <= 0)
            {
                idx = Array.LastIndexOf(_items, item, i, i);
                if (idx != -1) return idx + _items.Length - _head;
                idx = Array.LastIndexOf(_items, item, _items.Length - 1, count - i);
                if (idx == -1) return -1;
                return idx - _head;
            }

            idx = Array.LastIndexOf(_items, item, i, count);
            if (idx != -1) return idx + _items.Length - _head;
            return -1;
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
                // index is between head and values.Length.
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

    /// <summary>
    /// Reduces the size of the lisque to the specified size by bulk-removing from the head.
    /// If the lisque is already smaller than the specified size, no action is taken.
    /// </summary>
    /// <param name="newSize">The size this lisque should have after this call completes, if smaller than the current size.</param>
    public void TruncateFirst(int newSize) {
        if (newSize <= 0) {
            Clear();
            return;
        }
        var oldSize = _size;
        if (oldSize <= newSize) return;
        if (_head <= _tail || _head + oldSize - newSize < _items.Length) {
            // only removing from head to head + newSize, which is contiguous
            Array.Clear(_items, _head, oldSize - newSize);
            _head += oldSize - newSize;
            if (_head >= _items.Length) _head -= _items.Length;
        } else {
            // tail is near the start, and we are removing from head to the end and then part near start
            Array.Clear(_items, _head, _items.Length - _head);
            _head = _tail + 1 - newSize;
            Array.Clear(_items, 0, _head);
        }

        _size = newSize;
        _version += oldSize - newSize;
    }


    /// <summary>
    /// Reduces the size of the lisque to the specified size by bulk-removing from the tail.
    /// If the lisque is already smaller than the specified size, no action is taken.
    /// </summary>
    /// <param name="newSize">The size this lisque should have after this call completes, if smaller than the current size.</param>
    public void TruncateLast(int newSize)
    {
        if (newSize <= 0) {
            Clear();
            return;
        }
        var oldSize = _size;
        if (oldSize <= newSize) return;
        if (_head <= _tail) {
            // only removing from tail, near the end, toward head, near the start
            Array.Clear(_items, _head + newSize, _tail + 1 - _head - newSize);
            _tail -= oldSize - newSize;
        } else if (_head + newSize < _items.Length) {
            // tail is near the start, but we have to remove elements through the start and into the back
            Array.Clear(_items, 0, _tail + 1);
            _tail = _head + newSize;
            Array.Clear(_items, _tail, _items.Length - _tail);
        } else {
            // tail is near the start, but we only have to remove some elements between tail and the start
            var newTail = _tail - (oldSize - newSize);
            Array.Clear(_items, newTail + 1, _tail - newTail);
            _tail = newTail;
        }

        _size = newSize;
        _version += oldSize - newSize;
    }

    public void RemoveRange(int index, int count)
    {
        if (_size <= 0 || index >= _size || count <= 0) return;

        if (index <= 0)
        {
            TruncateFirst(_size - count);
            return;
        }
        var toIndex = index + count;

        if (toIndex >= _size)
        {
            TruncateLast(_size - count);
            return;
        }

        if (_head <= _tail)
        {
            // tail is near the end, head is near the start
            var tailMinusTo = _tail + 1 - (_head + toIndex);
            if (tailMinusTo < 0) tailMinusTo += _items.Length;
            Array.Copy(_items, _head + toIndex, _items, _head + index, tailMinusTo);
            Array.Clear(_items, _tail + 1 - count, count);
            _tail -= count;
        }
        else if (_head + toIndex < _items.Length)
        {
            // head is at the end, and tail wraps around, but we are only removing items between head and end
            var headPlusFrom = _head + index;
            if (headPlusFrom >= _items.Length) headPlusFrom -= _items.Length;
            Array.Copy(_items, _head, _items, headPlusFrom, count);
            Array.Clear(_items, _head, count);
            _head += count;
        }
        else if (_head + toIndex - _items.Length - count >= 0)
        {
            // head is at the end, and tail wraps around, but we are only removing items between start and tail
            Array.Copy(_items, _head + toIndex - _items.Length, _items, _head + index - _items.Length,
                _tail + 1 - (_head + toIndex - _items.Length));
            Array.Clear(_items, _tail + 1 - count, count);
            _tail -= count;
        }
        else
        {
            // head is at the end, tail wraps around, and we must remove items that wrap from end to start
            Array.Copy(_items, _head, _items, _items.Length - index, index);
            Array.Copy(_items, _head + toIndex - _items.Length, _items, 0,
                _tail + 1 - (_head + toIndex - _items.Length));
            Array.Clear(_items, _head, _items.Length - index - _head);
            Array.Clear(_items, _tail + 1 - (_head + toIndex - _items.Length), _head + toIndex - _items.Length);
            _tail -= (_head + toIndex - _items.Length);
            _head = (_items.Length - index);
        }
        _size -= count;
        _version += count;
    }

    /// <summary>
    /// Removes from this lisque each element as it appears in toRemove, with duplicates in toRemove
    /// getting (attempted to be) removed multiple times.
    /// </summary>
    /// <param name="toRemove">Any IEnumerable of T items to remove from this lisque, with duplicates getting removed multiple times.</param>
    /// <returns>If any item was removed, true, otherwise false.</returns>
    public bool RemoveEach(IEnumerable<T> toRemove)
    {
        var oldSize = _size;
        foreach (var t in toRemove)
        {
            Remove(t);
        }

        return _size != oldSize;
    }
    
    /// <summary>
    /// Removes all the elements that match the conditions defined by the specified predicate.
    /// If match returns true given a T item, that item will be removed.
    /// </summary>
    /// <remarks>
    /// This operates in O(n) time.
    /// </remarks>
    /// <param name="match">The Predicate delegate that defines the conditions of the elements to remove.</param>
    /// <returns>The number of elements removed from the lisque.</returns>
    public int RemoveAll(Predicate<T> match)
    {
        var freeIndex = 0;   // the first free slot in items array
        var wrapIndex = _head;
        // Find the first item which needs to be removed.
        while (freeIndex < _size && !match(_items[wrapIndex]))
        {
            freeIndex++;
            wrapIndex++;
        }
        if (freeIndex >= _size) return 0;

        var current = freeIndex + 1;
        var wrapCurrent = wrapIndex + 1;
        if(wrapCurrent >= _items.Length) wrapCurrent = 0;
        
        while (current < _size)
        {
            // Find the first item which needs to be kept.
            while (current < _size && match(_items[wrapCurrent]))
            {
                current++;
                wrapCurrent++;
                if(wrapCurrent >= _items.Length) wrapCurrent = 0;
            }

            if (current >= _size) break;
            // copy item to the free slot.
            _items[wrapIndex++] = _items[wrapCurrent++];
            freeIndex++;
            current++;
            if(wrapIndex >= _items.Length) wrapIndex = 0;
            if(wrapCurrent >= _items.Length) wrapCurrent = 0;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if(_head <= _tail)
                Array.Clear(_items, wrapIndex, _size - freeIndex); // Clear the elements so that the gc can reclaim the references.
            else
            {
                var deleteCount = _size - freeIndex;
                if(wrapIndex + deleteCount <= _items.Length) 
                    Array.Clear(_items, wrapIndex, deleteCount);
                else
                {
                    // The deleted range wraps.
                    Array.Clear(_items, wrapIndex, _items.Length - wrapIndex);
                    Array.Clear(_items, 0, deleteCount - (_items.Length - wrapIndex));
                }
            }
        }

        var result = _size - freeIndex;
        _size = freeIndex;
        _version++;
        return result;
    }

    /// <summary>
    /// Creates a shallow copy of a range of elements in the source lisque.
    /// </summary>
    /// <param name="start">The zero-based index at which the range starts.</param>
    /// <param name="length">The length of the range.</param>
    /// <returns>A shallow copy of a range of elements in the source lisque.</returns>
    public Lisque<T> Slice(int start, int length)
    {
        var result = new Lisque<T>(length);
        if (_head + start + length <= _items.Length)
        {
            Array.Copy(_items, _head + start, result._items, 0, length);
        }
        else if(_head + start >= _items.Length)
        {
            Array.Copy(_items, _head + start - _items.Length, result._items, 0, length);
        }
        else
        {
            Array.Copy(_items, _head + start, result._items, 0, _items.Length - (_head + start));
            Array.Copy(_items, 0, result._items, _items.Length - (_head + start), length - (_items.Length - (_head + start)));
        }
        result._tail = length - 1;

        return result;
    }
    /// <summary>
    /// Creates a shallow copy of a range of elements in the source lisque.
    /// </summary>
    /// <param name="index">The zero-based index at which the range starts.</param>
    /// <param name="count">The length of the range.</param>
    /// <returns>A shallow copy of a range of elements in the source lisque.</returns>
    public Lisque<T> GetRange(int index, int count) => Slice(index, count);

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
            // index is between head and values.Length.
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

    /// <summary>
    /// Swaps the contents of two 0-based indices in the lisque.
    /// </summary>
    /// <param name="indexA">The first index to swap.</param>
    /// <param name="indexB">The second index to swap.</param>
    public void Swap(int indexA, int indexB)
    {
        indexA += _head;
        if(indexA >= _items.Length) indexA -= _items.Length;
        indexB += _head;
        if(indexB >= _items.Length) indexB -= _items.Length;
        (_items[indexA], _items[indexB]) = (_items[indexB], _items[indexA]);
    }

    public void Reverse() => Reverse(0, _size);

    public void Reverse(int start, int length)
    {
        if(start < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if(length < 0) throw new ArgumentOutOfRangeException(nameof(start));
        if(_size - start < length) throw new ArgumentException("Not enough elements available after start for the requested length.");
        var halfLength = length / 2;
        for (int i = start, j = start + length - 1, c = 0; c < halfLength; i++, j--, c++)
        {
            Swap(i, j);
        }
    }

    /// <summary>
    /// Sorts the entire lisque using the default comparison for T.
    /// </summary>
    public void Sort()
    {
        if(_size <= 1) return;
        if (_head <= _tail)
        {
            Array.Sort(_items, _head, _size);
        }
        else
        {
            Array.Copy(_items, _head, _items, _tail + 1, _items.Length - _head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, _size, _items.Length - _size);
            }
            Array.Sort(_items, 0, _size);
            _head = 0;
            _tail = _size - 1;
        }
    }

    /// <summary>
    /// Sorts the entire lisque using the given IComparer of T.
    /// </summary>
    public void Sort(IComparer<T>? comparer)
    {
        if(_size <= 1) return;
        if (_head <= _tail)
        {
            Array.Sort(_items, _head, _size, comparer);
        }
        else
        {
            Array.Copy(_items, _head, _items, _tail + 1, _items.Length - _head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, _size, _items.Length - _size);
            }
            Array.Sort(_items, 0, _size, comparer);
            _head = 0;
            _tail = _size - 1;
        }
    }

    /// <summary>
    /// Sorts a subregion of the lisque using the given IComparer of T.
    /// </summary>
    /// <remarks>
    /// This performs an O(n) operation, <see cref="TrimExcess"/>, and then can perform a sort simply using
    /// <see cref="Array.Sort(Array, int, int, IComparer?"/>, which takes O(n log(n)) time in the expected case.
    /// </remarks>
    public void Sort(int index, int count, IComparer<T>? comparer)
    {
        if(_size <= 1) return;
        TrimExcess();
        Array.Sort(_items, index, count, comparer);
    }

    /// <summary>
    /// Sorts the entire lisque using the given Comparison of T.
    /// </summary>
    /// <remarks>
    /// This is the same as the <see cref="Sort(IComparer{T})"/> method except that it creates an IComparer
    /// from the given Comparison if it needs to sort at all.
    /// </remarks>
    public void Sort(Comparison<T> comparer)
    {
        if(_size <= 1) return;
        if (_head <= _tail)
        {
            Array.Sort(_items, _head, _size, Comparer<T>.Create(comparer));
        }
        else
        {
            Array.Copy(_items, _head, _items, _tail + 1, _items.Length - _head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(_items, _size, _items.Length - _size);
            }
            Array.Sort(_items, 0, _size, Comparer<T>.Create(comparer));
            _head = 0;
            _tail = _size - 1;
        }
    }

    public int BinarySearch(int index, int count, T item, IComparer<T>? comparer)
    {
        
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));
        if (_size - index < count)
            throw new ArgumentException("index + count is too large for the size of the lisque."); 

        if(_head <= _tail || _head + index + count <= _items.Length)
        {
            var found = Array.BinarySearch(_items, index + _head, count, item, comparer);
            if(found >= 0) return found - _head;
            return found + _head;
        } 
        if(_head + index >= _items.Length)
        {
            var found = Array.BinarySearch(_items, index - (_items.Length - _head), count, item, comparer);
            if(found >= 0) return found + (_items.Length - _head);
            return found - (_items.Length - _head);
        }
        else
        {
            var found = Array.BinarySearch(_items, index + _head, _items.Length - _head, item, comparer);
            if(found >= 0) return found - _head;
            found = Array.BinarySearch(_items, 0, count - (_items.Length - _head), item, comparer);
            if(found >= 0) return found + (_items.Length - _head);
            return found - (_items.Length - _head);
        }
    }
    
    public int BinarySearch(T item)
        => BinarySearch(0, Count, item, null);

    public int BinarySearch(T item, IComparer<T>? comparer)
        => BinarySearch(0, Count, item, comparer);
    
    /// <summary>
    /// Determines whether every element in the lisque matches the conditions defined by the specified predicate.
    /// </summary>
    /// <param name="match">The Predicate delegate that defines the conditions to check against the elements.</param>
    /// <returns>true if every element in the lisque matches the conditions defined by the specified predicate;
    /// otherwise, false. If the lisque has no elements, the return value is true.</returns>
    public bool TrueForAll(Predicate<T> match)
    {
        if(_head <= _tail)
        {
            for (var i = _head; i <= _tail; i++)
            {
                if (!match(_items[i]))
                {
                    return false;
                }
            }
        }
        else
        {
            for (var i = _head; i < _items.Length; i++)
            {
                if (!match(_items[i]))
                {
                    return false;
                }
            }
            for (var i = 0; i <= _tail; i++)
            {
                if (!match(_items[i]))
                {
                    return false;
                }

            }
        }
        return true;
    }

    /// <summary>
    /// Converts the elements in the current lisque to another type, and returns a lisque containing
    /// the converted elements.
    /// </summary>
    /// <param name="converter">A Converter{TInput,TOutput} delegate that converts each element from one type
    /// to another type.</param>
    /// <typeparam name="TOutput">The type of the elements of the target lisque.</typeparam>
    /// <returns>A Lisque{TOutput} of the target type containing the converted elements from the current Lisque{T}.</returns>
    public Lisque<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
    {
        var lisque = new Lisque<TOutput>(_size);
        for (var i = 0; i < _size; i++)
        {
            lisque._items[i] = converter(this[i]);
        }

        lisque._size = _size;
        lisque._tail = _size - 1;
        return lisque;
    }

    /// <summary>
    /// Performs the specified action on each element of the lisque.
    /// </summary>
    /// <param name="action">The Action{T} delegate to perform on each element of the lisque.</param>
    /// <exception cref="InvalidOperationException">An element in the collection has been modified.</exception>
    public void ForEach(Action<T> action)
    {
        var version = _version;

        for (var i = 0; i < _size; i++)
        {
            if (version != _version)
            {
                break;
            }
            action(this[i]);
        }

        if (version != _version)
            throw new InvalidOperationException("Lisque was modified externally during ForEach() call."); 
    }
    
    /// <summary>
    /// Returns a read-only ReadOnlyCollection wrapper for the current collection.
    /// </summary>
    /// <returns>An object that acts as a read-only wrapper around the current lisque.</returns>
    public ReadOnlyCollection<T> AsReadOnly()
        => new(this);
    
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

    public bool Equals(Lisque<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if(_size != other._size) return false;

        using var mine = GetEnumerator();
        using var them = other.GetEnumerator();
        while (mine.MoveNext() && them.MoveNext())
        {
            if(!Equals(mine.Current, them.Current)) return false;
        }

        return true;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Lisque<T>)obj);
    }

    // ReSharper disable method NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        var index = _head;

        var hash = _size + 1;
        for (int s = 0; s < _size; s++) {
            var value = _items[index];

            hash *= 29;
            if (value != null)
                hash += value.GetHashCode();

            index++;
            if (index == _items.Length)
                index = 0;
        }

        return hash;
    }

    public static bool operator ==(Lisque<T>? left, Lisque<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Lisque<T>? left, Lisque<T>? right)
    {
        return !Equals(left, right);
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
    private void Resize(int newSize)
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
    
    /*
 * 
 */
    /// <summary>
    /// Increases the size of the backing array to accommodate the specified number of additional items.
    /// </summary>
    /// <remarks>
    /// Useful before adding many items to avoid multiple backing array resizes.
    /// </remarks>
    /// <param name="additional">How many additional items should become available in the capacity.</param>
    public void EnsureCapacity(int additional) {
        var needed = _size + additional;
        if (_items.Length < needed) {
            Resize(needed);
        }
    }
    
    /// <summary>
    /// Make sure there is a "gap" of exactly gapSize values starting at index.
    /// </summary>
    /// <remarks>
    /// This can resize the backing array to achieve this goal. If possible, this will keep the same backing array and
    /// modify it in-place. The "gap" is not assigned null, and may contain old/duplicate references; calling code must
    /// overwrite the entire gap with additional values to ensure GC correctness.
    /// </remarks>
    /// <param name="index">The 0-based index in the iteration order where the gap will be present.</param>
    /// <param name="gapSize">The number of items that will need filling in the gap, and can be filled without issues.</param>
    /// <returns>The position in the array where the gap will begin, which is unrelated to the index.</returns>
	private int EnsureGap(int index, int gapSize) {
		if (gapSize <= 0) return 0;
		if (index < 0) index = 0;
		if (index > _size) {
			var oldSize = _size;
			EnsureCapacity(gapSize);
			return oldSize;
		}
		if (_size == 0) {
			_head = 0;
			_tail = gapSize - 1;
			if (_items.Length < gapSize) {
				_items = new T[gapSize];
			}
			return 0;
		}

        if (_size == 1)
        {
            if (_items.Length < gapSize + _size) {
                var item = _items[_head];
                _items = new T[gapSize + _size];
                if (index == 0) {
                    _items[gapSize] = item;
                    _head = 0;
                    _tail = gapSize;
                    return 0;
                }

                _items[0] = item;
                _head = 0;
                _tail = gapSize;
                return 1;
            }

            if (index == 0) {
                if (_head != gapSize) {
                    _items[gapSize] = _items[_head];
                    _items[_head] = default!;
                }
                _head = 0;
                _tail = gapSize;
                return 0;
            }

            if (_head != 0) {
                _items[0] = _items[_head];
                _items[_head] = default!;
            }
            _head = 0;
            _tail = gapSize;
            return 1;
        }

        var headOld = _head;
        var tailOld = _tail;
        var newSize = Math.Max(_size + gapSize, _items.Length);
        if (newSize == _items.Length)
        {
            // keep the same array because there is enough room to form the gap.
            if (headOld <= tailOld)
            {
                if (headOld != 0)
                {
                    if (index > 0)
                        Array.Copy(_items, headOld, _items, 0, index);
                    _head = 0;
                }

                Array.Copy(_items, headOld + index, _items, index + gapSize, _size - _head - index);
                _tail += gapSize - (headOld - _head);
                return index;
            }

            if (headOld + index <= _items.Length)
            {
                if (headOld - gapSize >= 0)
                {
                    Array.Copy(_items, headOld, _items, headOld - gapSize, index);
                    _head -= gapSize;
                }
                else
                {
                    Array.Copy(_items, headOld + index, _items, headOld + index + gapSize,
                        _items.Length - (headOld + index + gapSize));
                    _tail += gapSize;
                }

                return _head + index;
            }

            var wrapped = headOld + index - _items.Length;
            Array.Copy(_items, wrapped, _items, wrapped + gapSize, tailOld + 1 - wrapped);
            _tail += gapSize;
            return wrapped;
        }

        var newArray = new T[newSize];

        if (headOld <= tailOld)
        {
            // Continuous
            if (index > 0)
                Array.Copy(_items, headOld, newArray, 0, index);
            _head = 0;
            Array.Copy(_items, headOld + index, newArray, index + gapSize, _size - headOld - index);
            _tail += gapSize;
        }
        else
        {
            // Wrapped
            var headPart = _items.Length - headOld;
            if (index < headPart)
            {
                if (index > 0)
                    Array.Copy(_items, headOld, newArray, 0, index);
                _head = 0;
                Array.Copy(_items, headOld + index, newArray, index + gapSize, headPart - index);
                Array.Copy(_items, 0, newArray, index + gapSize + headPart - index, tailOld + 1);
            }
            else
            {
                Array.Copy(_items, headOld, newArray, 0, headPart);
                var wrapped = index - headPart; // same as: head + index - values.Length;
                Array.Copy(_items, 0, newArray, headPart, wrapped);
                Array.Copy(_items, wrapped, newArray, headPart + wrapped + gapSize, tailOld + 1 - wrapped);
            }

            _tail = _size + gapSize - 1;
        }

        _items = newArray;
        return index;
    }
    
    /// <summary>
    /// Sets the capacity to the actual number of elements in the lisque.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="List{T}.TrimExcess()"/>, this doesn't have a threshold value.
    /// It always sets the capacity to the actual size.
    /// </remarks>
    public void TrimExcess()
    {
        if (_size >= _items.Length) return;
        _version++;
        if (_head <= _tail) {
            var next = new T[_size];
            Array.Copy(_items, _head, next, 0, _size);
            _items = next;
        } else {
            var next = new T[_size];
            Array.Copy(_items, _head, next, 0, _items.Length - _head);
            Array.Copy(_items, 0, next, _items.Length - _head, _tail + 1);
            _items = next;
        }
        _head = 0;
        _tail = _items.Length - 1;
    }
    
    public int Capacity
    {
        get => _items.Length;
        set => Resize(value);
    }

    private struct Enumerator : IEnumerator<T>
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