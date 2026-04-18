using System.Collections;
using System.Runtime.CompilerServices;

namespace Dastardly.Data;

public class Lisque<T> : ILisque<T>, IEquatable<Lisque<T>>
{
    private const int DefaultCapacity = 4;

    // The plan is to rename these all at once when my Java code has been fully ported.
    
    // ReSharper disable once InconsistentNaming
    private T[] items;
    // ReSharper disable once InconsistentNaming
    private int size;
    // ReSharper disable once InconsistentNaming
    private int head;
    // ReSharper disable once InconsistentNaming
    private int tail;

    private int _version;

    public Lisque(int capacity = DefaultCapacity)
    {
        items = new T[Math.Max(1, capacity)];
        size = 0;
        head = 0;
        tail = 0;
        _version = 0;
    }

    public Lisque(IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);

        if (collection is ICollection<T> c)
        {
            int count = c.Count;
            if (count == 0)
            {
                items = new T[DefaultCapacity];
            }
            else
            {
                items = new T[count];
                c.CopyTo(items, 0);
                size = count;
                head = 0;
                tail = count - 1;
            }
        }
        else
        {
            items = new T[DefaultCapacity];
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
            var oldSize = size;
            EnsureCapacity(cs);
            if (ReferenceEquals(c, this)) {
                if (head <= tail) {
                    if (head >= oldSize)
                        Array.Copy(items, head, items, head - oldSize, oldSize);
                    else if (head > 0) {
                        Array.Copy(items, tail + 1 - head, items, 0, head);
                        Array.Copy(items, head, items, items.Length - (oldSize - head), oldSize - head);
                    } else {
                        Array.Copy(items, head, items, items.Length - oldSize, oldSize);
                    }
                } else {
                    Array.Copy(items, head, items, head - oldSize, items.Length - head);
                    Array.Copy(items, 0, items, items.Length - oldSize, tail + 1);
                }
                head -= oldSize;
                if (head < 0) head += items.Length;
                size += oldSize;
                _version += oldSize;
            } else {
                var i = EnsureGap(0, cs);
                foreach (var t in c) {
                    items[i++] = t;
                    if (i == items.Length) i = 0;
                }
                size += cs;
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
    /// Unlike <see cref="AddRangeFirst"/>, this performs in <c>O(m)</c> time, unless the capacity
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
    /// Unlike <see cref="AddRangeFirst"/>, this performs in <c>O(m)</c> time, unless the capacity
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
            var oldSize = size;
            EnsureCapacity(cs);
            if (ReferenceEquals(collection, this))
            {
                if (head <= tail) {
                    if (tail + 1 < items.Length)
                        Array.Copy(items, head, items, tail + 1, Math.Min(size, items.Length - tail - 1));
                    if (items.Length - tail - 1 < size)
                        Array.Copy(items, head + items.Length - tail - 1, items, 0, size - (items.Length - tail - 1));
                } else {
                    Array.Copy(items, head, items, tail + 1, items.Length - head);
                    Array.Copy(items, 0, items, tail + 1 + items.Length - head, tail + 1);
                }
                tail += oldSize;
                size += oldSize;
                _version += oldSize;
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

    public void Clear()
    {
        if (size <= 0) return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (head <= tail)
                Array.Clear(items, head, tail - head + 1);
            else
            {
                Array.Clear(items, head, items.Length - head);
                Array.Clear(items, 0, tail + 1);
            }
        }

        size = 0;
        head = 0;
        tail = 0;
        _version++;
    }

    public bool Contains(T item)
    {
        if (size == 0) return false;
        if (head <= tail)
        {
            return Array.IndexOf(items, item, head, size) >= 0;
        }

        return Array.IndexOf(items, item, 0, tail + 1) >= 0 ||
               Array.IndexOf(items, item, head, items.Length - head) >= 0;
    }
    
    /// <summary>
    /// Searches for an element that matches the conditions defined by the specified predicate, and returns the
    /// first occurrence within the entire lisque.
    /// </summary>
    /// <param name="match">The Predicate of T delegate that defines the conditions of the element to search for.</param>
    /// <returns>The first element that matches the conditions defined by the specified predicate, if found;
    /// otherwise, the default value for type T.</returns>
    public T? Find(Predicate<T> match)
    {
        for (int i = head, ii = 0; ii < size; ii++)
        {
            if (match(items[i]))
            {
                return items[i];
            }

            ++i;
            if (i >= items.Length) i = 0;
        }
        return default;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if (head <= tail)
            Array.Copy(items, head, array, arrayIndex, size);
        else
        {
            Array.Copy(items, head, array, arrayIndex, items.Length - head);
            Array.Copy(items, 0, array, arrayIndex + items.Length - head, tail + 1);
        }
    }

    public bool Remove(T item)
    {
        if (size == 0) return false;
        int index;

        if (head <= tail)
        {
            index = Array.IndexOf(items, item, head, size);
            if (index != -1) index -= head;
        }
        else
        {
            index = Array.IndexOf(items, item, head, items.Length - head);
            if (index != -1)
            {
                index -= head;
            }
            else
            {
                index = Array.IndexOf(items, item, 0, tail + 1);
                if (index != -1) index += items.Length - head;
            }
        }

        if (index == -1) return false;

        if (index == 0)
        {
            items[head] = default!;
            head++;
            if (head == items.Length)
            {
                head = 0;
            }

            if (--size <= 1) tail = head;
        }
        else if (index >= size - 1)
        {
            items[tail] = default!;

            if (tail == 0)
            {
                tail = items.Length - 1;
            }
            else
            {
                --tail;
            }

            if (--size <= 1) tail = head;
        }
        else
        {
            index += head;
            if (head <= tail)
            {
                // index is between head and tail.
                Array.Copy(items, index + 1, items, index, tail - index);
                items[tail] = default!;
                tail--;
                if (tail == -1) tail = items.Length - 1;
            }
            else if (index >= items.Length)
            {
                // index is between 0 and tail.
                index -= items.Length;
                Array.Copy(items, index + 1, items, index, tail - index);
                items[tail] = default!;
                tail--;
                if (tail == -1) tail = items.Length - 1;
            }
            else
            {
                // index is between head and values.Length.
                Array.Copy(items, head, items, head + 1, index - head);
                items[head] = default!;
                head++;
                if (head == items.Length)
                {
                    head = 0;
                }
            }

            size--;
        }

        _version++;
        return true;
    }

    public int Count => size;
    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        if (size == 0) return -1;
        if (head <= tail)
        {
            var idx = Array.IndexOf(items, item, head, size);
            if (idx == -1) return -1;
            return idx - head;
        }
        else
        {
            var idx = Array.IndexOf(items, item, head, items.Length - head);
            if (idx != -1) return idx - head;
            idx = Array.IndexOf(items, item, 0, tail + 1);
            if (idx == -1) return -1;
            return idx + items.Length - head;
        }
    }

    public int IndexOf(T item, int index)
    {
        return IndexOf(item, index, size - index);
    }
    
    public int IndexOf(T item, int index, int count)
    {
        if (index > size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "index argument cannot be greater than the size of the collection, or negative.");
        if (count < 0 || index > size - count)
            throw new ArgumentOutOfRangeException(nameof(count), "count argument is invalid.");
        if (size == 0) return -1;
        if (head <= tail)
        {
            var idx = Array.IndexOf(items, item, head + index, count);
            if (idx == -1) return -1;
            return idx - head;
        }
        else
        {
            var cnt = Math.Min(count, items.Length - head - index);
            var idx = Array.IndexOf(items, item, head + index, cnt);
            if (idx != -1) return idx - head;
            if (count <= cnt) return -1;
            idx = Array.IndexOf(items, item, 0, Math.Min(tail + 1, count - cnt));
            if (idx == -1) return -1;
            return idx + items.Length - head;
        }
    }

    public int LastIndexOf(T item)
    {
        return LastIndexOf(item, size - 1, size);
    }

    public int LastIndexOf(T item, int index)
    {
        return LastIndexOf(item, index, index + 1);
    }

    public int LastIndexOf(T item, int index, int count)
    {
        if (index > size || index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "index argument cannot be greater than the size of the collection, or negative.");
        if (count < 0 || count > index + 1)
            throw new ArgumentOutOfRangeException(nameof(count), "count argument is invalid.");
        if (size == 0) return -1;
        if (head <= tail)
        {
            var idx = Array.LastIndexOf(items, item, head + index, count);
            if (idx == -1) return -1;
            return idx - head;
        }
        else
        {
            var cnt = Math.Min(count, items.Length - head + index);
            var idx = Array.LastIndexOf(items, item, head + index, cnt);
            if (idx != -1) return idx - head;
            if (count <= cnt) return -1;
            idx = Array.LastIndexOf(items, item, 0, Math.Min(tail + 1, count - cnt));
            if (idx == -1) return -1;
            return idx + items.Length - head;
        }
    }

    public void Insert(int index, T item)
    {
        if (index <= 0)
            PushFirst(item);
        else if (index >= size)
            PushLast(item);
        else
        {
            if (++size > items.Length)
            {
                Resize(items.Length << 1);
            }

            if (head <= tail)
            {
                index += head;
                if (index >= items.Length) index -= items.Length;
                var after = index + 1;
                if (after >= items.Length) after = 0;

                Array.Copy(items, index, items, after, head + size - index - 1);
                items[index] = item;
                tail = head + size - 1;
                if (tail >= items.Length)
                {
                    tail = 0;
                }
            }
            else
            {
                if (head + index < items.Length)
                {
                    // backward shift
                    Array.Copy(items, head, items, head - 1, index);
                    items[head - 1 + index] = item;
                    head--;
                }
                else
                {
                    // forward shift
                    index = head + index - items.Length;
                    Array.Copy(items, index, items, index + 1, tail - index + 1);
                    items[index] = item;
                    tail++;
                }
            }

            _version++;
        }
    }

    public void RemoveAt(int index)
    {
        if (size == 0)
        {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        if (index <= 0)
        {
            items[head] = default!;
            head++;
            if (head == items.Length)
            {
                head = 0;
            }

            if (--size <= 1) tail = head;
        }
        else if (index >= size - 1)
        {
            items[tail] = default!;

            if (tail == 0)
            {
                tail = items.Length - 1;
            }
            else
            {
                --tail;
            }

            if (--size <= 1) tail = head;
        }
        else
        {
            index += head;
            if (head <= tail)
            {
                // index is between head and tail.
                Array.Copy(items, index + 1, items, index, tail - index);
                items[tail] = default!;
                tail--;
                if (tail == -1) tail = items.Length - 1;
            }
            else if (index >= items.Length)
            {
                // index is between 0 and tail.
                index -= items.Length;
                Array.Copy(items, index + 1, items, index, tail - index);
                items[tail] = default!;
                tail--;
                if (tail == -1) tail = items.Length - 1;
            }
            else
            {
                // index is between head and values.Length.
                Array.Copy(items, head, items, head + 1, index - head);
                items[head] = default!;
                head++;
                if (head == items.Length)
                {
                    head = 0;
                }
            }

            size--;
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
        var oldSize = size;
        if (oldSize <= newSize) return;
        if (head <= tail || head + oldSize - newSize < items.Length) {
            // only removing from head to head + newSize, which is contiguous
            Array.Clear(items, head, oldSize - newSize);
            head += oldSize - newSize;
            if (head >= items.Length) head -= items.Length;
        } else {
            // tail is near the start, and we are removing from head to the end and then part near start
            Array.Clear(items, head, items.Length - head);
            head = tail + 1 - newSize;
            Array.Clear(items, 0, head);
        }

        size = newSize;
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
        var oldSize = size;
        if (oldSize <= newSize) return;
        if (head <= tail) {
            // only removing from tail, near the end, toward head, near the start
            Array.Clear(items, head + newSize, tail + 1 - head - newSize);
            tail -= oldSize - newSize;
        } else if (head + newSize < items.Length) {
            // tail is near the start, but we have to remove elements through the start and into the back
            Array.Clear(items, 0, tail + 1);
            tail = head + newSize;
            Array.Clear(items, tail, items.Length - tail);
        } else {
            // tail is near the start, but we only have to remove some elements between tail and the start
            var newTail = tail - (oldSize - newSize);
            Array.Clear(items, newTail + 1, tail - newTail);
            tail = newTail;
        }

        size = newSize;
        _version += oldSize - newSize;
    }

    public void RemoveRange(int index, int count)
    {
        if (size <= 0 || index >= size || count <= 0) return;

        if (index <= 0)
        {
            TruncateFirst(size - count);
            return;
        }
        var toIndex = index + count;

        if (toIndex >= size)
        {
            TruncateLast(size - count);
            return;
        }

        if (head <= tail)
        {
            // tail is near the end, head is near the start
            var tailMinusTo = tail + 1 - (head + toIndex);
            if (tailMinusTo < 0) tailMinusTo += items.Length;
            Array.Copy(items, head + toIndex, items, head + index, tailMinusTo);
            Array.Clear(items, tail + 1 - count, count);
            tail -= count;
        }
        else if (head + toIndex < items.Length)
        {
            // head is at the end, and tail wraps around, but we are only removing items between head and end
            var headPlusFrom = head + index;
            if (headPlusFrom >= items.Length) headPlusFrom -= items.Length;
            Array.Copy(items, head, items, headPlusFrom, count);
            Array.Clear(items, head, count);
            head += count;
        }
        else if (head + toIndex - items.Length - count >= 0)
        {
            // head is at the end, and tail wraps around, but we are only removing items between start and tail
            Array.Copy(items, head + toIndex - items.Length, items, head + index - items.Length,
                tail + 1 - (head + toIndex - items.Length));
            Array.Clear(items, tail + 1 - count, count);
            tail -= count;
        }
        else
        {
            // head is at the end, tail wraps around, and we must remove items that wrap from end to start
            Array.Copy(items, head, items, items.Length - index, index);
            Array.Copy(items, head + toIndex - items.Length, items, 0,
                tail + 1 - (head + toIndex - items.Length));
            Array.Clear(items, head, items.Length - index - head);
            Array.Clear(items, tail + 1 - (head + toIndex - items.Length), head + toIndex - items.Length);
            tail -= (head + toIndex - items.Length);
            head = (items.Length - index);
        }
        size -= count;
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
        var oldSize = size;
        foreach (var t in toRemove)
        {
            Remove(t);
        }

        return size != oldSize;
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
        var wrapIndex = head;
        // Find the first item which needs to be removed.
        while (freeIndex < size && !match(items[wrapIndex]))
        {
            freeIndex++;
            wrapIndex++;
        }
        if (freeIndex >= size) return 0;

        var current = freeIndex + 1;
        var wrapCurrent = wrapIndex + 1;
        if(wrapCurrent >= items.Length) wrapCurrent = 0;
        
        while (current < size)
        {
            // Find the first item which needs to be kept.
            while (current < size && match(items[wrapCurrent]))
            {
                current++;
                wrapCurrent++;
                if(wrapCurrent >= items.Length) wrapCurrent = 0;
            }

            if (current >= size) break;
            // copy item to the free slot.
            items[wrapIndex++] = items[wrapCurrent++];
            freeIndex++;
            current++;
            if(wrapIndex >= items.Length) wrapIndex = 0;
            if(wrapCurrent >= items.Length) wrapCurrent = 0;
        }

        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if(head <= tail)
                Array.Clear(items, wrapIndex, size - freeIndex); // Clear the elements so that the gc can reclaim the references.
            else
            {
                var deleteCount = size - freeIndex;
                if(wrapIndex + deleteCount <= items.Length) 
                    Array.Clear(items, wrapIndex, deleteCount);
                else
                {
                    // The deleted range wraps.
                    Array.Clear(items, wrapIndex, items.Length - wrapIndex);
                    Array.Clear(items, 0, deleteCount - (items.Length - wrapIndex));
                }
            }
        }

        var result = size - freeIndex;
        size = freeIndex;
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
        if (head + start + length <= items.Length)
        {
            Array.Copy(items, head + start, result.items, 0, length);
        }
        else if(head + start >= items.Length)
        {
            Array.Copy(items, head + start - items.Length, result.items, 0, length);
        }
        else
        {
            Array.Copy(items, head + start, result.items, 0, items.Length - (head + start));
            Array.Copy(items, 0, result.items, items.Length - (head + start), length - (items.Length - (head + start)));
        }
        result.tail = length - 1;

        return result;
    }

    public T this[int index]
    {
        get
        {
            if (index <= 0)
                return items[head];
            if (index >= size - 1)
                return items[tail];
            var i = head + index;
            if (i >= items.Length)
                i -= items.Length;
            return items[i];
        }

        set
        {
            if (size <= 0 || index >= size)
                PushLast(value);
            else if (index < 0)
                PushFirst(value);
            else
            {
                var i = head + Math.Min(Math.Max(index, 0), size - 1);
                if (i >= items.Length)
                    i -= items.Length;
                items[i] = value;
            }
        }
    }

    public void PushFirst(T item)
    {
        if (size == items.Length)
        {
            Resize(size << 1);
        }

        head--;
        if (head == -1) head = items.Length - 1;
        items[head] = item;

        if (++size == 1) tail = head;
        _version++;
    }

    public void PushLast(T item)
    {
        if (size == items.Length)
        {
            Resize(items.Length << 1);
        }

        if (++size == 1) tail = head;
        else if (++tail == items.Length) tail = 0;
        items[tail] = item;
        _version++;

    }

    public T PopFirst()
    {
        if (size == 0)
        {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        var result = items[head];
        items[head] = default!;
        head++;
        if (head == items.Length)
        {
            head = 0;
        }

        if (--size <= 1) tail = head;
        _version++;

        return result;

    }

    public T PopLast()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("Lisque is empty.");
        }

        var result = items[tail];
        items[tail] = default!;

        if (tail == 0)
        {
            tail = items.Length - 1;
        }
        else
        {
            --tail;
        }

        if (--size <= 1) tail = head;
        _version++;

        return result;
    }

    public T PopAt(int index)
    {
        if (index <= 0)
            return PopFirst();
        if (index >= size - 1)
            return PopLast();

        index += head;
        T value;
        if (head <= tail)
        {
            // index is between head and tail.
            value = items[index];
            Array.Copy(items, index + 1, items, index, tail - index);
            items[tail] = default!;
            tail--;
            if (tail == -1) tail = items.Length - 1;
        }
        else if (index >= items.Length)
        {
            // index is between 0 and tail.
            index -= items.Length;
            value = items[index];
            Array.Copy(items, index + 1, items, index, tail - index);
            items[tail] = default!;
            tail--;
            if (tail == -1) tail = items.Length - 1;
        }
        else
        {
            // index is between head and values.Length.
            value = items[index];
            Array.Copy(items, head, items, head + 1, index - head);
            items[head] = default!;
            head++;
            if (head == items.Length)
            {
                head = 0;
            }
        }

        size--;
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
        (items[indexA], items[indexB]) = (items[indexB], items[indexA]);
    }

    /// <summary>
    /// Sorts the entire lisque using the default comparison for T.
    /// </summary>
    public void Sort()
    {
        if(size <= 1) return;
        if (head <= tail)
        {
            Array.Sort(items, head, size);
        }
        else
        {
            Array.Copy(items, head, items, tail + 1, items.Length - head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(items, size, items.Length - size);
            }
            Array.Sort(items, 0, size);
            head = 0;
            tail = size - 1;
        }
    }

    /// <summary>
    /// Sorts the entire lisque using the given IComparer of T.
    /// </summary>
    public void Sort(IComparer<T>? comparer)
    {
        if(size <= 1) return;
        if (head <= tail)
        {
            Array.Sort(items, head, size, comparer);
        }
        else
        {
            Array.Copy(items, head, items, tail + 1, items.Length - head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(items, size, items.Length - size);
            }
            Array.Sort(items, 0, size, comparer);
            head = 0;
            tail = size - 1;
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
        if(size <= 1) return;
        TrimExcess();
        Array.Sort(items, index, count, comparer);
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
        if(size <= 1) return;
        if (head <= tail)
        {
            Array.Sort(items, head, size, Comparer<T>.Create(comparer));
        }
        else
        {
            Array.Copy(items, head, items, tail + 1, items.Length - head);
            if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
            {
                Array.Clear(items, size, items.Length - size);
            }
            Array.Sort(items, 0, size, Comparer<T>.Create(comparer));
            head = 0;
            tail = size - 1;
        }
    }

    public T First
    {
        get => size == 0 ? throw new InvalidOperationException("Lisque is empty.") : items[head];
        set
        {
            if (size == 0) throw new InvalidOperationException("Lisque is empty.");
            items[head] = value;
        }
    }

    public T Last
    {
        get => size == 0 ? throw new InvalidOperationException("Lisque is empty.") : items[tail];
        set
        {
            if (size == 0) throw new InvalidOperationException("Lisque is empty.");
            items[tail] = value;
        }
    }

    public bool Equals(Lisque<T>? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if(size != other.size) return false;

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
        var index = head;

        var hash = size + 1;
        for (int s = 0; s < size; s++) {
            var value = items[index];

            hash *= 29;
            if (value != null)
                hash += value.GetHashCode();

            index++;
            if (index == items.Length)
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
        if (newSize < size)
            newSize = size;

        var newArray = new T[Math.Max(1, newSize)];

        if (size > 0)
        {
            if (head <= tail)
            {
                // Continuous
                Array.Copy(items, head, newArray, 0, tail - head + 1);
            }
            else
            {
                // Wrapped
                var rest = items.Length - head;
                Array.Copy(items, head, newArray, 0, rest);
                Array.Copy(items, 0, newArray, rest, tail + 1);
            }

            head = 0;
            tail = size - 1;
        }

        items = newArray;
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
        var needed = size + additional;
        if (items.Length < needed) {
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
		if (index > size) {
			var oldSize = size;
			EnsureCapacity(gapSize);
			return oldSize;
		}
		if (size == 0) {
			head = 0;
			tail = gapSize - 1;
			if (items.Length < gapSize) {
				items = new T[gapSize];
			}
			return 0;
		}

        if (size == 1)
        {
            if (items.Length < gapSize + size) {
                var item = items[head];
                items = new T[gapSize + size];
                if (index == 0) {
                    items[gapSize] = item;
                    head = 0;
                    tail = gapSize;
                    return 0;
                }

                items[0] = item;
                head = 0;
                tail = gapSize;
                return 1;
            }

            if (index == 0) {
                if (head != gapSize) {
                    items[gapSize] = items[head];
                    items[head] = default!;
                }
                head = 0;
                tail = gapSize;
                return 0;
            }

            if (head != 0) {
                items[0] = items[head];
                items[head] = default!;
            }
            head = 0;
            tail = gapSize;
            return 1;
        }

        var headOld = head;
        var tailOld = tail;
        var newSize = Math.Max(size + gapSize, items.Length);
        if (newSize == items.Length)
        {
            // keep the same array because there is enough room to form the gap.
            if (headOld <= tailOld)
            {
                if (headOld != 0)
                {
                    if (index > 0)
                        Array.Copy(items, headOld, items, 0, index);
                    head = 0;
                }

                Array.Copy(items, headOld + index, items, index + gapSize, size - head - index);
                tail += gapSize - (headOld - head);
                return index;
            }

            if (headOld + index <= items.Length)
            {
                if (headOld - gapSize >= 0)
                {
                    Array.Copy(items, headOld, items, headOld - gapSize, index);
                    head -= gapSize;
                }
                else
                {
                    Array.Copy(items, headOld + index, items, headOld + index + gapSize,
                        items.Length - (headOld + index + gapSize));
                    tail += gapSize;
                }

                return head + index;
            }

            var wrapped = headOld + index - items.Length;
            Array.Copy(items, wrapped, items, wrapped + gapSize, tailOld + 1 - wrapped);
            tail += gapSize;
            return wrapped;
        }

        var newArray = new T[newSize];

        if (headOld <= tailOld)
        {
            // Continuous
            if (index > 0)
                Array.Copy(items, headOld, newArray, 0, index);
            head = 0;
            Array.Copy(items, headOld + index, newArray, index + gapSize, size - headOld - index);
            tail += gapSize;
        }
        else
        {
            // Wrapped
            var headPart = items.Length - headOld;
            if (index < headPart)
            {
                if (index > 0)
                    Array.Copy(items, headOld, newArray, 0, index);
                head = 0;
                Array.Copy(items, headOld + index, newArray, index + gapSize, headPart - index);
                Array.Copy(items, 0, newArray, index + gapSize + headPart - index, tailOld + 1);
            }
            else
            {
                Array.Copy(items, headOld, newArray, 0, headPart);
                var wrapped = index - headPart; // same as: head + index - values.Length;
                Array.Copy(items, 0, newArray, headPart, wrapped);
                Array.Copy(items, wrapped, newArray, headPart + wrapped + gapSize, tailOld + 1 - wrapped);
            }

            tail = size + gapSize - 1;
        }

        items = newArray;
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
        if (size >= items.Length) return;
        _version++;
        if (head <= tail) {
            var next = new T[size];
            Array.Copy(items, head, next, 0, size);
            items = next;
        } else {
            var next = new T[size];
            Array.Copy(items, head, next, 0, items.Length - head);
            Array.Copy(items, 0, next, items.Length - head, tail + 1);
            items = next;
        }
        head = 0;
        tail = items.Length - 1;
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

            if ((uint)_index < (uint)localLisque.size)
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