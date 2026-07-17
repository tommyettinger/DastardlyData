using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Dastardly.Data;

public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ILisque<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dict;
    private readonly Lisque<KeyValuePair<TKey, TValue>> _lisque;
    
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _lisque.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_lisque).GetEnumerator();
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        _lisque.Add(item);
    }

    public void Clear()
    {
        _lisque.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _lisque.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _lisque.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return _lisque.Remove(item);
    }

    public int Count => _lisque.Count;

    public bool IsReadOnly => _lisque.IsReadOnly;

    public int IndexOf(KeyValuePair<TKey, TValue> item)
    {
        return _lisque.IndexOf(item);
    }

    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        _lisque.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        _lisque.RemoveAt(index);
    }

    KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
    {
        get => _lisque[index];
        set => _lisque[index] = value;
    }

    public void PushFirst(KeyValuePair<TKey, TValue> item)
    {
        _lisque.PushFirst(item);
    }

    public void PushLast(KeyValuePair<TKey, TValue> item)
    {
        _lisque.PushLast(item);
    }

    public KeyValuePair<TKey, TValue> PopFirst()
    {
        return _lisque.PopFirst();
    }

    public KeyValuePair<TKey, TValue> PopLast()
    {
        return _lisque.PopLast();
    }

    public KeyValuePair<TKey, TValue> PopAt(int index)
    {
        return _lisque.PopAt(index);
    }

    public KeyValuePair<TKey, TValue> First
    {
        get => _lisque.First;
        set => _lisque.First = value;
    }

    public KeyValuePair<TKey, TValue> Last
    {
        get => _lisque.Last;
        set => _lisque.Last = value;
    }

    public void Add(TKey key, TValue value)
    {
        int oldCount = _dict.Count;
        _dict.Add(key, value);
        if(oldCount < _dict.Count)
            _lisque.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    public bool ContainsKey(TKey key)
    {
        return _dict.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        if (!_dict.Remove(key)) return false;
        _lisque.RemoveAll(pair => pair.Key.Equals(key));
        return true;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        return _dict.TryGetValue(key, out value);
    }

    public TValue this[TKey key]
    {
        get => _dict[key];
        set
        {
            int oldCount = _dict.Count;
            _dict[key] = value;
            if(oldCount < _dict.Count)
                _lisque.Add(new KeyValuePair<TKey, TValue>(key, value));
        }
    }

    public ICollection<TKey> Keys => new Lisque<TKey>(_lisque.Select(pair => pair.Key));

    public ICollection<TValue> Values => new Lisque<TValue>(_lisque.Select(pair => pair.Value));
}