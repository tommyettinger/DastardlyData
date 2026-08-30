using System.Collections.Frozen;
using System.Text;

namespace Dastardly.Data;

/// <summary>
/// Matches potentially more than one string in different ways against a supplied ICollection of string. 
/// </summary>
/// <remarks>
/// This is inspired by the Junction type in <a href="https://docs.raku.org/type/Junction">Raku</a>, but
/// isn't equivalent. A Junction is the outermost parent of its hierarchy, and contains a <see cref="ITerm"/> node.
/// use <see cref="Match"/> to check if a Junction matches an ICollection.
/// </remarks>
/// <param name="root">The root term for the Junction.</param>
public class Junction(ITerm root) : ITerm
{
    public ITerm Root = root;
    
    public string? Item => null;

    public char Symbol => '@';
    
    public string Name => "junction";

    public bool Match(ICollection<string> coll)
    {
        return Root.Match(coll);
    }

    public Lisque<string> Remove(Lisque<string> coll)
    {
        return Root.Remove(coll);
    }

    public ITerm Canonicalize()
    {
        return Root.Canonicalize();
    }

    public bool Equals(ITerm? other)
    {
        if (other is Junction o) return Root.Equals(o.Root);
        return false;
    }

    public int CompareTo(ITerm? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        return Root.CompareTo(other);
    }

    public override string ToString()
    {
        return Root.ToString();
    }

    public Junction Negate()
    {
        if (Root is Not n)
        {
            Root = n.Term;
        }
        else
        {
            Root = new Not(Root);
        }

        return this;
    }
    
    /// <summary>
    /// * Tokenizes a range of length chars in the string text starting at start. Used internally during parsing.
    /// </summary>
    /// <remarks>
    /// Returns a Lisque of strings; they should be considered "operator-like" if they are one of
    /// <c>()|&^~</c> and otherwise are names.
    /// </remarks>
    /// <param name="text">The string to tokenize.</param>
    /// <param name="start">The first index to read from, inclusive.</param>
    /// <param name="length">The length of the substring of text to read.</param>
    /// <returns>A Lisque of the tokenized strings.</returns>
    protected static Lisque<string> Lex(string text, int start, int length) {
        Lisque<string> deque = new Lisque<string>(length >> 1);
        StringBuilder sb = new StringBuilder(32);
        for (int i = start, idx = 0; idx < length; i++, idx++) {
            char current = text[i];
            switch (current) {
                case '~':
                    if (sb.Length > 0)
                        deque.Add(sb.ToString());
                    deque.Add("");
                    deque.Add(current.ToString());
                    sb.Clear();
                    break;
                case '(':
                case ')':
                case '|':
                case '&':
                case '^':
                    if (sb.Length > 0)
                        deque.Add(sb.ToString());
                    deque.Add(current.ToString());
                    sb.Clear();
                    break;
                default:
                    sb.Append(current);
                    break;
            }
        }
        if (sb.Length > 0)
            deque.Add(sb.ToString());
        return deque;
    }
    
    private static readonly FrozenDictionary<string, int> Operators = new Dictionary<string, int>
    {
        {"~", 27}, {"^", 9}, {"&", 6}, {"|", 3}
    }.ToFrozenDictionary();
    
    /// <summary>
    /// Checks if the operator precedence of other is greater than or equal to opPrecedence.
    /// If other is not a known operator, this returns false unless opPrecedence is negative.
    /// </summary>
    /// <remarks>
    /// Used internally by <see cref="ShuntingYard"/>.</remarks>
    /// <param name="opPrecedence">the precedence level to compare to.</param>
    /// <param name="other">The operator string to look up.</param>
    /// <returns>true if other represents a known operator with precedence greater than or equal to opPrecedence.</returns>
    private static bool CheckPrecedence(int opPrecedence, string other) {
        return Operators.GetValueOrDefault(other, 0) >= opPrecedence;
    }

    /// <summary>
    /// Implements the <a href="https://en.wikipedia.org/wiki/Shunting_yard_algorithm">Shunting yard algorithm</a>.
    /// </summary>
    /// <remarks>
    /// <a href="https://eddmann.com/posts/shunting-yard-implementation-in-java/">Credit to Edd Mann</a>.
    /// Edd's implementation operates on a StringBuilder, whereas we output another Lisque, so the order needed
    /// some work.
    /// </remarks>
    /// <param name="tokens">A Lisque typically produced by <see cref="Lex"/>.</param>
    /// <returns>The tokens, rearranged in postfix order and with parentheses removed.</returns>
    protected static Lisque<string> ShuntingYard(Lisque<string> tokens) {
        Lisque<string> output = new(tokens.Count), stack = new(16);

        foreach (var token in tokens)
        {
            if (Operators.ContainsKey(token)) {
                var opPrecedence = Operators[token];
                while (stack.Count > 0 && CheckPrecedence(opPrecedence, stack.First))
                    output.Add(stack.PopFirst());
                stack.PushFirst(token);
            } else if (token == "(") {
                stack.PushFirst(token);
            } else if (token == ")") {
                while ("(" != stack.First)
                    output.PushLast(stack.PopFirst());
                stack.PopFirst();
            } else {
                output.Add(token);
            }
        }

        while (stack.Count > 0)
            output.Add(stack.PopFirst());

        return output;
    }

    /// <summary>
    /// Parses the string text into one Junction.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <returns>The resulting Junction.</returns>
    public static Junction Parse(string text) {
        return Parse(text, 0, text.Length);
    }

    /// <summary>
    /// Parses a substring of text into one Junction. This reads length chars from text, starting at start.
    /// </summary>
    /// <param name="text">The string to parse.</param>
    /// <param name="start">The first index to read from, inclusive</param>
    /// <param name="length">The length of the substring of text to read.</param>
    /// <returns>The resulting Junction.</returns>
    public static Junction Parse(string text, int start, int length) {
        var tokens = Lex(text, start, length);
        tokens = ShuntingYard(tokens);
        Lisque<ITerm> terms = new (tokens.Count);
        foreach (var tok in tokens)
        {
            if (Operators.ContainsKey(tok)) {
                ITerm right = terms.PopLast(), left = terms.PopLast();
                switch (tok[0]) {
                    case '~':
                        terms.PushLast(new Not(right));
                        break;
                    case '^':
                        terms.PushLast(new One(left, right));
                        break;
                    case '&':
                        terms.PushLast(new All(left, right));
                        break;
                    case '|':
                        terms.PushLast(new Any(left, right));
                        break;
                }
            } else {
                terms.PushLast(new Just(tok));
            }
        }
        if (terms.Count == 0) terms.PushLast(new Any());
        return new Junction(terms.Last);
    }

}

/// <summary>
/// Simply matches a single string value, with no additional terms involved.
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

    public override string ToString()
    {
        return Item ?? "null";
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

    public override string ToString()
    {
        return '~' + Term.ToString();
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

    public override string ToString()
    {
        return '(' + string.Join('|', Terms) + ')';
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

    public override string ToString()
    {
        return '(' + string.Join('&', Terms) + ')';
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

    public override string ToString()
    {
        return '(' + string.Join('^', Terms) + ')';
    }
}