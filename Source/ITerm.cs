namespace Dastardly.Data;

/// <summary>
/// Mostly internal; meant to be used by Junction to form filters for ICollections of string.
/// </summary>
public interface ITerm : IEquatable<ITerm>
{
    public bool Match(ICollection<string> coll);
    
    public ICollection<string> Remove(ICollection<string> coll);

    public string? Item { get; protected set; }
    
    public ITerm Canonicalize();
    
    public char Symbol { get; protected set; }
    
    public string Name { get; protected set; }
}
