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

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return (other is Just just) ? string.Compare(Data, just.Data, StringComparison.InvariantCulture) :  (Symbol - other.Symbol);;
    }
}

/// <summary>
/// Matches when the term it holds doesn't match.
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

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return other.CompareTo(Term);
        
    }
}

/// <summary>
/// Takes one or more terms and matches if any of those terms match.
/// </summary>
/// <param name="terms">One or more ITerm items that this can match any of.</param>
public class Any(params ITerm[] terms) : ITerm
{
    public readonly Lisque<ITerm> Terms = new(terms);

    public string? Item => null;

    public char Symbol => '|';
    
    public string Name => "any";

    public bool Match(ICollection<string> coll)
    {
        foreach (var t in Terms)
        {
            if(t.Match(coll)) return true;
        }

        return false;
    }

    public Lisque<string> Remove(Lisque<string> coll)
    {
        foreach (var t in Terms)
        {
            t.Remove(coll);
        }

        return coll;
    }

    public ITerm Canonicalize()
    {
        for (int i = 0, n = Terms.Count; i < n; i++)
        {
            ITerm child = Terms[i];
            if (child is Any any)
            {
                Terms.RemoveAt(i--);
                n--;
                Terms.AddRange(any.Terms);
            }
        }

        foreach (var t in Terms)
        {
            t.Canonicalize();
        }

        Terms.Sort();
        return this;
    }

    public bool Equals(ITerm? other)
    {
        if (other is Any o)
        {
            if (Terms.Count != o.Terms.Count) return false;

            for (var i = 0; i < Terms.Count; i++)
            {
                if (!Terms[i].Equals(o.Terms[i])) return false;
            }

            return true;
        }

        return false;
    }

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (other is Any a)
        {
            if (Terms.Count != a.Terms.Count)
                return Terms.Count - a.Terms.Count;
            for (int i = 0; i < Terms.Count; i++)
            {
                var comp = Terms[i].CompareTo(a.Terms[i]);
                if (comp != 0) return comp;
            }
        }

        return Symbol - other.Symbol;
    }
}

/// <summary>
/// Takes one or more terms and matches only if all those terms match.
/// </summary>
/// <param name="terms">One or more ITerm items that this must match all of to match.</param>
public class All(params ITerm[] terms) : ITerm
{
    public readonly Lisque<ITerm> Terms = new(terms);

    public string? Item => null;

    public char Symbol => '&';
    
    public string Name => "all";

    public bool Match(ICollection<string> coll)
    {
        foreach (var t in Terms)
        {
            if(!t.Match(coll)) return false;
        }

        return true;
    }

    public Lisque<string> Remove(Lisque<string> coll)
    {
        foreach (var t in Terms)
        {
            if(!t.Match(coll)) return coll;
        }
        
        foreach (var t in Terms)
        {
            t.Remove(coll);
        }

        return coll;
    }

    public ITerm Canonicalize()
    {
        for (int i = 0, n = Terms.Count; i < n; i++)
        {
            ITerm child = Terms[i];
            if (child is All all)
            {
                Terms.RemoveAt(i--);
                n--;
                Terms.AddRange(all.Terms);
            }
        }

        foreach (var t in Terms)
        {
            t.Canonicalize();
        }

        Terms.Sort();
        return this;
    }

    public bool Equals(ITerm? other)
    {
        if (other is All o)
        {
            if (Terms.Count != o.Terms.Count) return false;

            for (var i = 0; i < Terms.Count; i++)
            {
                if (!Terms[i].Equals(o.Terms[i])) return false;
            }

            return true;
        }

        return false;
    }

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (other is All a)
        {
            if (Terms.Count != a.Terms.Count)
                return Terms.Count - a.Terms.Count;
            for (int i = 0; i < Terms.Count; i++)
            {
                var comp = Terms[i].CompareTo(a.Terms[i]);
                if (comp != 0) return comp;
            }
        }

        return Symbol - other.Symbol;
    }
}

/// <summary>
/// Takes one or more terms and matches if exactly one of those terms matches.
/// </summary>
/// <param name="terms">One or more ITerm items that this must match exactly one of.</param>
public class One(params ITerm[] terms) : ITerm
{
    public readonly Lisque<ITerm> Terms = new(terms);

    public string? Item => null;

    public char Symbol => '^';
    
    public string Name => "one";

    public bool Match(ICollection<string> coll)
    {
        var count = 0;
        for (var i = 0; i < Terms.Count && count <= 1; i++)
        {
            if (Terms[i].Match(coll)) count++;
        }

        return count == 1;
    }

    public Lisque<string> Remove(Lisque<string> coll)
    {
        foreach (var t in Terms)
        {
            if(t.Match(coll)) t.Remove(coll);
        }

        return coll;
    }

    public ITerm Canonicalize()
    {
        for (int i = 0, n = Terms.Count; i < n; i++)
        {
            ITerm child = Terms[i];
            if (child is One one)
            {
                Terms.RemoveAt(i--);
                n--;
                Terms.AddRange(one.Terms);
            }
        }

        foreach (var t in Terms)
        {
            t.Canonicalize();
        }

        Terms.Sort();
        return this;
    }

    public bool Equals(ITerm? other)
    {
        if (other is One o)
        {
            if (Terms.Count != o.Terms.Count) return false;

            for (var i = 0; i < Terms.Count; i++)
            {
                if (!Terms[i].Equals(o.Terms[i])) return false;
            }

            return true;
        }

        return false;
    }

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (other is One o)
        {
            if (Terms.Count != o.Terms.Count)
                return Terms.Count - o.Terms.Count;
            for (int i = 0; i < Terms.Count; i++)
            {
                var comp = Terms[i].CompareTo(o.Terms[i]);
                if (comp != 0) return comp;
            }
        }

        return Symbol - other.Symbol;
    }
}