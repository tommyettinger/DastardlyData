using System.Collections;
using System.Runtime.CompilerServices;

namespace Dastardly.Data;

public class Lisque<T> : ILisque<T>
{
    private const int DefaultCapacity = 4;

    protected T[] Items;
    protected int Size;
    protected int Head;
    protected int Tail;

    internal int Version;

    public Lisque(int capacity = DefaultCapacity)
    {
        Items = new T[Math.Max(1, capacity)];
        Size = 0;
        Head = 0;
        Tail = 0;
        Version = 0;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public void Add(T item)
    {
        PushLast(item);
    }

    public void Clear()
    {
        if (Size <= 0) return;
        if (RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (Head <= Tail)
                Array.Clear(Items, Head, Tail - Head + 1);
            else
            {
                Array.Clear(Items, Head, Items.Length - Head);
                Array.Clear(Items, 0, Tail + 1);
            }
        }

        Size = 0;
        Head = 0;
        Tail = 0;
        Version++;
    }

    public bool Contains(T item)
    {
        if (Size == 0) return false;
        if (Head <= Tail)
        {
            return Array.IndexOf(Items, item, Head, Size) >= 0;
        }

        return Array.IndexOf(Items, item, 0, Tail + 1) >= 0 ||
               Array.IndexOf(Items, item, Head, Items.Length - Head) >= 0;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        if(Head <= Tail) 
            Array.Copy(Items, Head, array, arrayIndex, Size);
        else
        {
            Array.Copy(Items, Head, array, arrayIndex, Items.Length - Head);
            Array.Copy(Items, 0, array, arrayIndex + Items.Length - Head, Tail + 1);
        }
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public int Count => Size;
    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        if (Size == 0) return -1;
        if (Head <= Tail)
        {
            var index = Array.IndexOf(Items, item, Head, Size);
            if (index == -1) return -1;
            return index - Head;
        }
        else
        {
            var index = Array.IndexOf(Items, item, Head, Items.Length - Head);
            if (index != -1) return index - Head;
            index = Array.IndexOf(Items, item, 0, Tail + 1);
            if (index == -1) return -1;
            return index + Items.Length - Head;
        }
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        if (Size == 0) {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        if (index <= 0)
        {
            Items[Head] = default!;
            Head++;
            if (Head == Items.Length) {
                Head = 0;
            }
            if (--Size <= 1) Tail = Head;
        }
        else if (index >= Size)
        {
            Items[Tail] = default!;

            if (Tail == 0) {
                Tail = Items.Length - 1;
            } else {
                --Tail;
            }
            if (--Size <= 1) Tail = Head;
        }
        else
        {
            index += Head;
            if (Head <= Tail)
            {
                // index is between head and tail.
                Array.Copy(Items, index + 1, Items, index, Tail - index);
                Items[Tail] = default!;
                Tail--;
                if (Tail == -1) Tail = Items.Length - 1;
            }
            else if (index >= Items.Length)
            {
                // index is between 0 and tail.
                index -= Items.Length;
                Array.Copy(Items, index + 1, Items, index, Tail - index);
                Items[Tail] = default!;
                Tail--;
                if (Tail == -1) Tail = Items.Length - 1;
            }
            else
            {
                // index is between head and values.length.
                Array.Copy(Items, Head, Items, Head + 1, index - Head);
                Items[Head] = default!;
                Head++;
                if (Head == Items.Length)
                {
                    Head = 0;
                }
            }

            Size--;
        }

        Version++;
    }

    public T this[int index]
    {
        get
        {
            if (index <= 0)
                return Items[Head];
            if (index >= Size - 1)
                return Items[Tail];
            var i = Head + index;
            if (i >= Items.Length)
                i -= Items.Length;
            return Items[i];
        }

        set
        {
            if (Size <= 0 || index >= Size)
                PushLast(value);
            else if (index < 0)
                PushFirst(value);
            else
            {
                var i = Head + Math.Min(Math.Max(index, 0), Size - 1);
                if (i >= Items.Length)
                    i -= Items.Length;
                Items[i] = value;
            }
        }
    }

    public void PushFirst(T item)
    {
        if (Size == Items.Length) {
            Resize(Size << 1);
        }
        var head = this.Head - 1;
        if (head == -1) head = Items.Length - 1;
        Items[head] = item;

        Head = head;
        if (++Size == 1) Tail = head;
        Version++;
    }

    public void PushLast(T item)
    {
        if (Size == Items.Length)
        {
            Resize(Items.Length << 1);
        }

        if (++Size == 1) Tail = Head;
        else if (++Tail == Items.Length) Tail = 0;
        Items[Tail] = item;
        Version++;

    }

    public T PopFirst()
    {
        if (Size == 0) {
            // Underflow
            throw new InvalidOperationException("Lisque is empty.");
        }

        var result = Items[Head];
        Items[Head] = default!;
        Head++;
        if (Head == Items.Length) {
            Head = 0;
        }
        if (--Size <= 1) Tail = Head;
        Version++;

        return result;

    }

    public T PopLast()
    {
        if (Size == 0) {
            throw new InvalidOperationException("Lisque is empty.");
        }
        
        var result = Items[Tail];
        Items[Tail] = default!;

        if (Tail == 0) {
            Tail = Items.Length - 1;
        } else {
            --Tail;
        }
        if (--Size <= 1) Tail = Head;
        Version++;

        return result;
    }

    public T PopAt(int index)
    {
        if (index <= 0)
            return PopFirst();
        if (index >= Size)
            return PopLast();

        index += Head;
        T value;
        if (Head <= Tail) { // index is between head and tail.
            value = Items[index];
            Array.Copy(Items, index + 1, Items, index, Tail - index);
            Items[Tail] = default!;
            Tail--;
            if (Tail == -1) Tail = Items.Length - 1;
        } else if (index >= Items.Length) { // index is between 0 and tail.
            index -= Items.Length;
            value = Items[index];
            Array.Copy(Items, index + 1, Items, index, Tail - index);
            Items[Tail] = default!;
            Tail--;
            if (Tail == -1) Tail = Items.Length - 1;
        } else { // index is between head and values.length.
            value = Items[index];
            Array.Copy(Items, Head, Items, Head + 1, index - Head);
            Items[Head] = default!;
            Head++;
            if (Head == Items.Length) {
                Head = 0;
            }
        }
        Size--;
        Version++;
        return value;
    }

    public T First
    {
        get => Size == 0 ? throw new InvalidOperationException("Lisque is empty.") : Items[Head];
        set
        {
            if (Size == 0) throw new InvalidOperationException("Lisque is empty.");
            Items[Head] = value;
        }
    }

    public T Last
    {
        get => Size == 0 ? throw new InvalidOperationException("Lisque is empty.") : Items[Tail];
        set
        {
            if (Size == 0) throw new InvalidOperationException("Lisque is empty.");
            Items[Tail] = value;
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
        if (newSize < Size)
            newSize = Size;

        var newArray = new T[Math.Max(1, newSize)];

        if (Size > 0)
        {
            if (Head <= Tail)
            {
                // Continuous
                Array.Copy(Items, Head, newArray, 0, Tail - Head + 1);
            }
            else
            {
                // Wrapped
                var rest = Items.Length - Head;
                Array.Copy(Items, Head, newArray, 0, rest);
                Array.Copy(Items, 0, newArray, rest, Tail + 1);
            }

            Head = 0;
            Tail = Size - 1;
        }

        Items = newArray;
        Version++;
    }
}