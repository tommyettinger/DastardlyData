using Dastardly.Data;

namespace Tests;

public class Tests {
    private static Lisque<string> GenerateFull() 
    {
        Lisque<string> lisque = new(8);
        lisque.Add("alpha");
        lisque.Add("beta");
        lisque.Add("gamma");
        lisque.Add("delta");
        lisque.Add("epsilon");
        lisque.Add("zeta");
        lisque.Add("eta");
        lisque.Add("theta");
        return lisque;
    }

    private static Lisque<string> GeneratePartial() 
    {
        Lisque<string> lisque = new(8);
        lisque.Add("alpha");
        lisque.Add("beta");
        lisque.Add("gamma");
        lisque.Add("delta");
        lisque.Add("epsilon");
        lisque.Add("zeta");
        return lisque;
    }

    private static Lisque<string> GeneratePartialMissingFirst() 
    {
        Lisque<string> lisque = new(8);
        lisque.Add("");
        lisque.Add("alpha");
        lisque.Add("beta");
        lisque.Add("gamma");
        lisque.Add("delta");
        lisque.Add("epsilon");
        lisque.Add("zeta");
        lisque.RemoveAt(0);
        return lisque;
    }

    private static Lisque<string> GeneratePartialWrapped() 
    {
        Lisque<string> lisque = new(8);
        lisque.Add("gamma");
        lisque.Add("delta");
        lisque.Add("epsilon");
        lisque.Add("zeta");
        lisque.PushFirst("beta");
        lisque.PushFirst("alpha");
        return lisque;
    }
    
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

    [Test]
    public void TestIndexOf()
    {
        Lisque<string> lisque = new([
            "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta",
            "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta",
            "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta"
        ]);
        Assert.That(lisque.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(lisque.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(lisque.IndexOf("beta", 2), Is.EqualTo(8));
        Assert.That(lisque.IndexOf("beta", 2, 4), Is.EqualTo(-1));
        Assert.That(lisque.IndexOf("beta", 15, 4), Is.EqualTo(15));
        Assert.That(lisque.LastIndexOf("alpha"), Is.EqualTo(14));
        Assert.That(lisque.LastIndexOf("beta"), Is.EqualTo(15));
        Assert.That(lisque.LastIndexOf("beta", 7), Is.EqualTo(1));
        Assert.That(lisque.LastIndexOf("beta", 20, 4), Is.EqualTo(-1));
        Assert.That(lisque.LastIndexOf("beta", 20, 20), Is.EqualTo(15));
    }

    [Test]
    public void TestIndexOfWrapping()
    {
        Lisque<string> lisque = new([                 "gamma",  "delta", "epsilon", "zeta", "eta",
                                     "alpha", "beta", "gamma",  "delta", "epsilon", "zeta", "eta",
                                     "alpha", "beta", "gamma",  "delta", "epsilon", "zeta", "eta"]);
        // makes head wrap around.
        lisque.AddRangeFirst(["alpha", "beta"]);
        Assert.That(lisque.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(lisque.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(lisque.IndexOf("beta", 2), Is.EqualTo(8));
        Assert.That(lisque.IndexOf("beta", 2, 4), Is.EqualTo(-1));
        Assert.That(lisque.IndexOf("beta", 15, 4), Is.EqualTo(15));
        Assert.That(lisque.LastIndexOf("alpha"), Is.EqualTo(14));
        Assert.That(lisque.LastIndexOf("beta"), Is.EqualTo(15));
        Assert.That(lisque.LastIndexOf("beta", 7), Is.EqualTo(1));
        Assert.That(lisque.LastIndexOf("beta", 20, 4), Is.EqualTo(-1));
        Assert.That(lisque.LastIndexOf("beta", 20, 20), Is.EqualTo(15));
    }
}