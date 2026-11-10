using Dastardly.Data;

namespace Tests;

public class JunctionTest
{
	[Test]
	public void TestMatch()
	{
		Junction j = Junction.Parse("((~cardiac-arrest&fumble)|(block&miss))");
		var possible =
			new IndexedDictionary<string, Lisque<string>>()
			{
				{ "miss", ["miss"] },
				{ "blocking miss", ["miss", "block"] },
				{ "blocking miss and counter", ["miss", "block", "counter"] },
				{ "fumble", ["fumble"] },
				{ "fumble with miss", ["miss", "fumble"] }
			};

		Console.Out.WriteLine("Checking Junction: " + j);
		foreach (var e in possible)
		{
			Console.Out.WriteLine(e.Key + " matches [" + string.Join(", ", e.Value) + "] ? " + j.Match(e.Value));
		}
		Assert.That(!j.Match(new Lisque<string>{"miss"}));
		Assert.That(j.Match(new Lisque<string>{"block", "miss"}));
	}

	[Test]
	public void TestParse() {
		string text = "((~cardiac-arrest&fumble)|(block&miss))";
		Console.Out.WriteLine("Ideal:  " + text);
		Junction manual = new Junction(new Any(new All(new Just("miss"), new Just("block")), new All(new Just("fumble"), new Not(new Just("cardiac-arrest")))));
		Console.Out.WriteLine("Manual: " + manual);
		Junction alternate = new Junction(new Any(new All(new Just("fumble"), new Not(new Just("cardiac-arrest"))), new All(new Just("miss"), new Just("block"))));
		Console.Out.WriteLine("Alt:    " + alternate);
		Junction junction = Junction.Parse(text);
		Console.Out.WriteLine("Parsed: " + junction);
	}

    [Test]
    public void TestCanonicalize() {
        ITerm manual = new All(
            new Any(new Just("a"), new Just("c")),
            new All(
                new Just("a"),
                new Any(
                    new Just("a"),
                    new Just("b"))));
        Console.Out.WriteLine(manual);
        manual.Canonicalize();
        Console.Out.WriteLine(manual);
        Assert.That(manual.Match(new Lisque<string>{"a"}));
    }
    /*
     * Checking Junction: ((block&miss)|(fumble&~cardiac-arrest))
     * miss Removing from [miss] : miss
     * blocking miss Removing from [miss, block] :
     * blocking miss and counter Removing from [miss, block, counter] : counter
     * fumble Removing from [fumble] :
     * fumble with miss Removing from [miss, fumble] :
     */ 
    [Test]
    public void TestRemove()
    {
	    Junction j = Junction.Parse("((~cardiac-arrest&fumble)|(block&miss))");
	    var possible =
		    new IndexedDictionary<string, Lisque<string>>()
		    {
			    { "miss", ["miss"] },
			    { "blocking miss", ["miss", "block"] },
			    { "blocking miss and counter", ["miss", "block", "counter"] },
			    { "fumble", ["fumble"] },
			    { "fumble with miss", ["miss", "fumble"] }
		    };

	    Console.Out.WriteLine("Checking Junction: " + j);
	    foreach (var e in possible)
	    {
		    Console.Out.WriteLine(e.Key + " Removing from [" + string.Join(", ", e.Value) + "] : " + string.Join(", ", j.Remove([.. e.Value])));
	    }
	    // Assert.That(!j.Match(new Lisque<string>{"miss"}));
	    // Assert.That(j.Match(new Lisque<string>{"block", "miss"}));
    }
    
}