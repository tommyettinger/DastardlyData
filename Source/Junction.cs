namespace Dastardly.Data;

public class Junction
{
    
}

/// <summary>
/// Simply matches a single String value, with no additional terms involved.
/// </summary>
public class Just(string data = "") : ITerm
{
    public string? Data = data;

    public string? Item => Data;

    public char Symbol => '=';
    
    public string Name => "just";

    public bool Match(ICollection<string> coll)
    {
        return Data != null && coll.Contains(Data);
    }

    public ICollection<string> Remove(ICollection<string> coll)
    {
        if(Data != null) coll.Remove(Data);
        return coll;
    }

    public ITerm Canonicalize()
    {
        return this;
    }

    public bool Equals(ITerm? other)
    {
        if (other is not Just) return false;
        return Data == other.Item;
    }
}