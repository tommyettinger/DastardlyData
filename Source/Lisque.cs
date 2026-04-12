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
        throw new NotImplementedException();
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
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public void PushFirst(T item)
    {
        throw new NotImplementedException();
    }

    public void PushLast(T item)
    {
        throw new NotImplementedException();
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
            if(Size == 0) throw new InvalidOperationException("Lisque is empty.");
            Items[Head] = value;
        }
    }

    public T Last
    {
        get => Size == 0 ? throw new InvalidOperationException("Lisque is empty.") : Items[Tail];
        set
        {
            if(Size == 0) throw new InvalidOperationException("Lisque is empty.");
            Items[Tail] = value;
        }

    }
}