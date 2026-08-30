using Dastardly.Data;

namespace Tests;

public class JunctionTest
{
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
    
}