namespace Dastardly.Data;

/// <summary>
/// Mostly internal; meant to be used by Junction to form filters for ICollections of string.
/// </summary>
public interface ITerm : IEquatable<ITerm>, IComparable<ITerm>
{
    public bool Match(ICollection<string> coll);
    
    public Lisque<string> Remove(Lisque<string> coll);

    public string? Item { get; }
    
    public ITerm Canonicalize();
    
    public char Symbol { get; }
    
    public string Name { get; }
}
