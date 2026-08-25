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

    public Lisque<string> Remove(Lisque<string> coll)
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
/// <summary>
/// Simply matches a single String value, with no additional terms involved.
/// </summary>
public class Not(ITerm term) : ITerm
{
    public ITerm Term = term;

    public string? Item => null;

    public char Symbol => '~';
    
    public string Name => "not";

    public bool Match(ICollection<string> coll)
    {
        return !Term.Match(coll);
    }

    public Lisque<string> Remove(Lisque<string> coll)
    {
			Lisque<string> lisque = [.. coll];
			Term.Remove(lisque);
			coll.RemoveEach(lisque);
			return coll;
    }

    public ITerm Canonicalize()
    {
        return Term is Not t ? t.Term : this;
    }

    public bool Equals(ITerm? other)
    {
        if (other is Not o) return Term.Equals(o.Term);
        return false;
    }
}