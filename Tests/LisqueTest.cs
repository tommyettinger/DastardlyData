using Dastardly.Data;

namespace Tests;

public class Tests {
    
    [SetUp]
    public void Setup() {

    }

    [Test]
    public void TestPushAndGet()
    {
        Lisque<string> lisque = new(16);
        lisque.PushLast("beta");
        lisque.PushFirst("alpha");
        lisque.PushLast("gamma");
        Assert.That(lisque[0], Is.EqualTo("alpha"));
        Assert.That(lisque[1], Is.EqualTo("beta"));
        Assert.That(lisque[2], Is.EqualTo("gamma"));
    }

    [Test]
    public void TestHeadWraps()
    {
        Lisque<string> lisque = new(4);
        lisque.PushFirst("gamma");
        lisque.PushFirst("beta");
        lisque.PushFirst("alpha");
        Assert.That(lisque[0], Is.EqualTo("alpha"));
        Assert.That(lisque[1], Is.EqualTo("beta"));
        Assert.That(lisque[2], Is.EqualTo("gamma"));
        Assert.That(lisque.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(lisque.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(lisque.IndexOf("gamma"), Is.EqualTo(2));
        Assert.That(lisque.IndexOf("not here"), Is.EqualTo(-1));

        var received = lisque.PopAt(1);
        Assert.That(received, Is.EqualTo("beta"));
        Assert.That(lisque[1], Is.EqualTo("gamma"));
        Assert.That(lisque.IndexOf("beta"), Is.EqualTo(-1));
        Assert.That(lisque.IndexOf("gamma"), Is.EqualTo(1));
    }
    [Test]
    public void TestEnumerator()
    {
        Lisque<string> lisque = new(4);
        lisque.PushLast("beta");
        lisque.PushFirst("alpha");
        lisque.PushLast("gamma");
        lisque.PushLast("delta");
        lisque.PushLast("epsilon");
        lisque.PushLast("eta");
        lisque.PushFirst("OOPS");
        lisque.PopFirst();
        lisque.Insert(5, "zeta");
        lisque.Insert(5, "BEFORE ZETA");
        lisque.RemoveAt(5);
        Assert.That(lisque, Is.EquivalentTo(["alpha", "beta", "gamma",  "delta", "epsilon", "zeta", "eta"]));
        foreach (var letter in lisque)
        {
            Console.WriteLine(letter);
        }
    }
}