using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Dastardly.Data;

public class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ILisque<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, TValue> _dict;
    private readonly Lisque<KeyValuePair<TKey, TValue>> _lisque;

    public IndexedDictionary(IEqualityComparer<TKey>? comparer = null)
    {
        _dict = new Dictionary<TKey, TValue>(comparer);
        _lisque = new Lisque<KeyValuePair<TKey, TValue>>();
    }
    
    public IndexedDictionary(int capacity, IEqualityComparer<TKey>? comparer = null)
    {
        _dict = new Dictionary<TKey, TValue>(capacity, comparer);
        _lisque = new Lisque<KeyValuePair<TKey, TValue>>(capacity);
    }

    public IndexedDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey>? comparer = null) :
        this(dictionary.Count, comparer)
    {
        foreach (var pair in dictionary)
        {
            Add(pair.Key, pair.Value);
        }
    }

    public IndexedDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection,
        IEqualityComparer<TKey>? comparer = null) :
        this((collection as ICollection<KeyValuePair<TKey, TValue>>)?.Count ?? 0, comparer)
    {
        foreach (var pair in collection)
        {
            Add(pair.Key, pair.Value);
        }
    }

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
        if(_dict.TryAdd(item.Key, item.Value))
            _lisque.Add(item);
    }

    public void Clear()
    {
        _dict.Clear();
        _lisque.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _dict.TryGetValue(item.Key, out var o) && ((o is not null && o.Equals(item.Value)) || o is null && item.Value is null);
        
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _lisque.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return _dict.Remove(item.Key) && _lisque.Remove(item);
    }

    public int Count => _lisque.Count;

    public bool IsReadOnly => _lisque.IsReadOnly;

    public int IndexOf(KeyValuePair<TKey, TValue> item)
    {
        return _lisque.IndexOf(item);
    }

    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        if(_dict.TryAdd(item.Key, item.Value)) 
            _lisque.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        var pair = _lisque.PopAt(index);
        _dict.Remove(pair.Key);
    }

    KeyValuePair<TKey, TValue> IList<KeyValuePair<TKey, TValue>>.this[int index]
    {
        get => _lisque[index];
        set => _lisque[index] = value;
    }

    public void PushFirst(KeyValuePair<TKey, TValue> item)
    {
        if(_dict.TryAdd(item.Key, item.Value)) 
            _lisque.PushFirst(item);
    }

    public void PushLast(KeyValuePair<TKey, TValue> item)
    {
        if(_dict.TryAdd(item.Key, item.Value))
            _lisque.PushLast(item);
    }

    public KeyValuePair<TKey, TValue> PopFirst()
    {
        KeyValuePair<TKey, TValue> pair = _lisque.PopFirst();
        _dict.Remove(pair.Key);
        return pair;
    }

    public KeyValuePair<TKey, TValue> PopLast()
    {
        KeyValuePair<TKey, TValue> pair = _lisque.PopLast();
        _dict.Remove(pair.Key);
        return pair;
    }

    public KeyValuePair<TKey, TValue> PopAt(int index)
    {
        KeyValuePair<TKey, TValue> pair = _lisque.PopAt(index);
        _dict.Remove(pair.Key);
        return pair;
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
        if(_dict.TryAdd(key, value))
            _lisque.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    public bool ContainsKey(TKey key)
    {
        return _dict.ContainsKey(key);
    }

    public bool Remove(TKey key)
    {
        if (!_dict.Remove(key)) return false;
        _lisque.RemoveFirst(pair => _dict.Comparer.Equals(pair.Key, key));
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
            _dict[key] = value;
            var index = _lisque.FindIndex(match => match.Key.Equals(key));
            _lisque[index] = new KeyValuePair<TKey, TValue>(key, value);
        }
    }

    public ICollection<TKey> Keys => new Lisque<TKey>(_lisque.Select(pair => pair.Key));

    public ICollection<TValue> Values => new Lisque<TValue>(_lisque.Select(pair => pair.Value));
}