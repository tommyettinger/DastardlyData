using System.Collections;

namespace Dastardly.Data;

public class IndexedSet<T> : ISet<T>, ILisque<T> where T : notnull
{
    private readonly HashSet<T> _set;
    private readonly Lisque<T> _lisque;

    /// <summary>
    /// Creates a new empty IndexedSet that may use a specified IEqualityComparer.
    /// </summary>
    /// <param name="comparer">May be null to use the default ordering of T; otherwise, used to compare T items.</param>
    public IndexedSet(IEqualityComparer<T>? comparer = null)
    {
        _set = new HashSet<T>(comparer);
        _lisque = new Lisque<T>();
    }
    
    /// <summary>
    /// Creates a new IndexedSet that contains the unique items from <c>collection</c>, and may use a specified
    /// IEqualityComparer.
    /// </summary>
    /// <param name="collection">Any IEnumerable of T; often an <see cref="ICollection{T}"/>.</param>
    /// <param name="comparer">May be null to use the default ordering of T; otherwise, used to compare T items.</param>
    public IndexedSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer = null) : this(comparer)
    {
        foreach (var t in collection)
        {
            Add(t);
        }
    }

    /// <summary>
    /// Creates a new IndexedSet that can hold at least <c>capacity</c> items and may use a specified IEqualityComparer.
    /// </summary>
    /// <param name="capacity">The minimum number of items this should be able to hold without resizing.</param>
    /// <param name="comparer">May be null to use the default ordering of T; otherwise, used to compare T items.</param>
    public IndexedSet(int capacity, IEqualityComparer<T>? comparer = null)
    {
        _set = new HashSet<T>(capacity, comparer);
        _lisque = new Lisque<T>(capacity);
    }

    /// <summary>
    /// Shallow-copies an IndexedSet into a new IndexedSet; the contents, order, and comparer will be the same.
    /// </summary>
    /// <param name="other">An IndexedSet to shallow-copy; the contents, order, and comparer will be the same.</param>
    public IndexedSet(IndexedSet<T> other)
    {
        _set = new HashSet<T>(other._set, other.Comparer);
        _lisque = new Lisque<T>(other._lisque);
    }

    /// <summary>
    /// Shallow-copies a HashSet into a new IndexedSet; the order is undefined but will not have duplicates.
    /// </summary>
    /// <param name="other">A HashSet to shallow-copy; its <see cref="HashSet{T}.Comparer"/> will be used here.</param>
    public IndexedSet(HashSet<T> other)
    {
        _set = new HashSet<T>(other, other.Comparer);
        _lisque = new Lisque<T>(other);
    }
    
    public bool IsReadOnly => false;
    
    public IEqualityComparer<T> Comparer => _set.Comparer;

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

    public void ExceptWith(IEnumerable<T> other)
    {
        _set.ExceptWith(other);
        _lisque.RemoveAll(t => !_set.Contains(t));
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        _set.IntersectWith(other);
        _lisque.RemoveAll(t => !_set.Contains(t));
        
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        return _set.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return _set.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
        return _set.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return _set.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<T> other)
    {
        return _set.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<T> other)
    {
        return _set.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        var s = other.ToHashSet();
        _set.SymmetricExceptWith(s);
        s.IntersectWith(_set);
        _lisque.AddRange(s);
        _lisque.RemoveAll(t => !_set.Contains(t));
    }

    public void UnionWith(IEnumerable<T> other)
    {
        var s = other.ToHashSet();
        s.ExceptWith(_set);
        _set.UnionWith(s);
        _lisque.AddRange(s);
    }

    public int RemoveWhere(Predicate<T> match)
    {
        var removed = _set.RemoveWhere(match);
        _lisque.RemoveAll(t => !_set.Contains(t));
        return removed;
    }

    public void EnsureCapacity(int capacity)
    {
        _set.EnsureCapacity(capacity);
        _lisque.EnsureCapacity(capacity);
    }
    
    bool ISet<T>.Add(T item)
    {
        if (!_set.Add(item)) return false;
        _lisque.Add(item);
        return true;
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

    public void Reverse()
    {
        _lisque.Reverse();
    }
    
    public void Sort()
    {
        _lisque.Sort();
    }
    
    public void Sort(IComparer<T> comparer)
    {
        _lisque.Sort(comparer);
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

    public void Swap(int indexA, int indexB)
    {
        _lisque.Swap(indexA, indexB);
    }

    public IndexedSet<T> Slice(int start, int length)
    {
        return new IndexedSet<T>(_lisque.Slice(start, length), _set.Comparer);
    }

    public void TrimExcess()
    {
        _set.TrimExcess();
        _lisque.TrimExcess();
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return SetEquals((IndexedSet<T>)obj);
    }

    // ReSharper disable method NonReadonlyMemberInGetHashCode
    public override int GetHashCode()
    {
        var hash = _lisque.Count + 1;
        foreach (var value in _lisque)
        {
            hash += value.GetHashCode();
        }
        return hash;
    }

    public static bool operator ==(IndexedSet<T>? left, IndexedSet<T>? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(IndexedSet<T>? left, IndexedSet<T>? right)
    {
        return !Equals(left, right);
    }

}