using System.Collections;

namespace Dastardly.Data;

public class IndexedSet<T> : ISet<T>, ILisque<T> where T : notnull
{
    private HashSet<T> _set;
    private Lisque<T> _lisque;
    
    public bool IsReadOnly => false;

    public IEnumerator<T> GetEnumerator()
    {
        return _lisque.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_lisque).GetEnumerator();
    }

    public void Add(T item)
    {
        if(_set.Add(item)) 
            _lisque.Add(item);
    }

    public void Clear()
    {
        _set.Clear();
        _lisque.Clear();
    }

    public bool Contains(T item)
    {
        return _set.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        _lisque.CopyTo(array, arrayIndex);
    }

    public bool Remove(T item)
    {
        return _set.Remove(item) && _lisque.Remove(item);
    }

    public int Count => _lisque.Count;
    
    public int IndexOf(T item)
    {
        return _lisque.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        if(_set.Add(item)) 
            _lisque.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _set.Remove(_lisque.PopAt(index));
    }

    public T this[int index]
    {
        get => _lisque[index];
        set => _lisque[index] = value;
    }

    public void PushFirst(T item)
    {
        if(_set.Add(item)) 
            _lisque.PushFirst(item);
    }

    public void PushLast(T item)
    {
        if(_set.Add(item)) 
            _lisque.PushLast(item);
    }

    public T PopFirst()
    {
        var popped = _lisque.PopFirst();
        _set.Remove(popped);
        return popped;
    }

    public T PopLast()
    {
        var popped = _lisque.PopLast();
        _set.Remove(popped);
        return popped;
    }

    public T PopAt(int index)
    {
        var popped = _lisque.PopAt(index);
        _set.Remove(popped);
        return popped;

    }

    public T First
    {
        get => _lisque.First;
        set
        {
            _set.Remove(_lisque.First);
            if(_set.Add(value)) 
                _lisque.First = value;
            else _lisque.PopFirst();
        }
    }

    public T Last
    {
        get => _lisque.Last;
        set
        {
            _set.Remove(_lisque.Last);
            if (_set.Add(value))
                _lisque.Last = value;
            else _lisque.PopLast();
        }

    }
}