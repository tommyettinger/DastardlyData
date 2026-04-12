using System.Collections;

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
        Items = new T[capacity];
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
        throw new NotImplementedException();
    }

    public bool Contains(T item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item)
    {
        throw new NotImplementedException();
    }

    public int Count => Size;
    public bool IsReadOnly => false;

    public int IndexOf(T item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, T item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
    }

    public T PopLast()
    {
        throw new NotImplementedException();
    }

    public T PopAt(int index)
    {
        throw new NotImplementedException();
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
        var head = Head;
        var tail = Tail;

        var newArray = new T[Math.Max(1, newSize)];

        if (Size > 0)
        {
            if (head <= tail)
            {
                // Continuous
                Array.Copy(Items, head, newArray, 0, tail - head + 1);
            }
            else
            {
                // Wrapped
                var rest = Items.Length - head;
                Array.Copy(Items, head, newArray, 0, rest);
                Array.Copy(Items, 0, newArray, rest, tail + 1);
            }

            Head = 0;
            Tail = Size - 1;
        }

        Items = newArray;
    }
}