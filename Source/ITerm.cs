namespace Dastardly.Data;

/// <summary>
/// Mostly internal; meant to be used by Junction to form filters for ICollections of string.
/// </summary>
public interface ITerm : IEquatable<ITerm>, IComparable<ITerm>
{
    public bool Match(ICollection<string> coll);
    
    public IList<string> Remove(IList<string> coll);
    
    public IList<string> Filter(IList<string> coll);

    public string? Item { get; }
    
    public ITerm Canonicalize();
    
    public char Symbol { get; }
    
    public string Name { get; }

    public string ToString();
}
