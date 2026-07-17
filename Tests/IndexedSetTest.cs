using Dastardly.Data;

namespace Tests;

public class IndexedSetTest
{
    private static IndexedSet<string> GenerateFull() 
    {
        IndexedSet<string> indexedSet = new(8);
        indexedSet.Add("alpha");
        indexedSet.Add("beta");
        indexedSet.Add("gamma");
        indexedSet.Add("delta");
        indexedSet.Add("epsilon");
        indexedSet.Add("zeta");
        indexedSet.Add("eta");
        indexedSet.Add("theta");
        return indexedSet;
    }

    private static IndexedSet<string> GeneratePartial() 
    {
        IndexedSet<string> indexedSet = new(8);
        indexedSet.Add("alpha");
        indexedSet.Add("beta");
        indexedSet.Add("gamma");
        indexedSet.Add("delta");
        indexedSet.Add("epsilon");
        indexedSet.Add("zeta");
        return indexedSet;
    }

    private static IndexedSet<string> GeneratePartialMissingFirst() 
    {
        IndexedSet<string> indexedSet = new(8);
        indexedSet.Add("");
        indexedSet.Add("alpha");
        indexedSet.Add("beta");
        indexedSet.Add("gamma");
        indexedSet.Add("delta");
        indexedSet.Add("epsilon");
        indexedSet.Add("zeta");
        indexedSet.RemoveAt(0);
        return indexedSet;
    }

    private static IndexedSet<string> GeneratePartialWrapped() 
    {
        IndexedSet<string> indexedSet = new(8);
        indexedSet.Add("gamma");
        indexedSet.Add("delta");
        indexedSet.Add("epsilon");
        indexedSet.Add("zeta");
        indexedSet.PushFirst("beta");
        indexedSet.PushFirst("alpha");
        return indexedSet;
    }
    
    private static IndexedSet<string>[] GenerateAll() => [GenerateFull(), GeneratePartial(),
        GeneratePartialMissingFirst(), GeneratePartialWrapped()];
    
    [SetUp]
    public void Setup() {

    }

    [Test]
    public void TestPushAndGet()
    {
        IndexedSet<string> indexedSet = new(16);
        indexedSet.PushLast("beta");
        indexedSet.PushFirst("alpha");
        indexedSet.PushLast("gamma");
        Assert.That(indexedSet[0], Is.EqualTo("alpha"));
        Assert.That(indexedSet[1], Is.EqualTo("beta"));
        Assert.That(indexedSet[2], Is.EqualTo("gamma"));
    }

    [Test]
    public void TestHeadWraps()
    {
        IndexedSet<string> indexedSet = new(4);
        indexedSet.PushFirst("gamma");
        indexedSet.PushFirst("beta");
        indexedSet.PushFirst("alpha");
        Assert.That(indexedSet[0], Is.EqualTo("alpha"));
        Assert.That(indexedSet[1], Is.EqualTo("beta"));
        Assert.That(indexedSet[2], Is.EqualTo("gamma"));
        Assert.That(indexedSet.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedSet.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedSet.IndexOf("gamma"), Is.EqualTo(2));
        Assert.That(indexedSet.IndexOf("not here"), Is.EqualTo(-1));

        var received = indexedSet.PopAt(1);
        Assert.That(received, Is.EqualTo("beta"));
        Assert.That(indexedSet[1], Is.EqualTo("gamma"));
        Assert.That(indexedSet.IndexOf("beta"), Is.EqualTo(-1));
        Assert.That(indexedSet.IndexOf("gamma"), Is.EqualTo(1));
    }

    [Test]
    public void TestEnumerator()
    {
        IndexedSet<string> indexedSet = new(4);
        indexedSet.PushLast("beta");
        indexedSet.PushFirst("alpha");
        indexedSet.PushLast("gamma");
        indexedSet.PushLast("delta");
        indexedSet.PushLast("epsilon");
        indexedSet.PushLast("eta");
        indexedSet.PushFirst("OOPS");
        indexedSet.PopFirst();
        indexedSet.Insert(5, "zeta");
        indexedSet.Insert(5, "BEFORE ZETA");
        indexedSet.RemoveAt(5);
        Assert.That(indexedSet, Is.EquivalentTo(["alpha", "beta", "gamma",  "delta", "epsilon", "zeta", "eta"]));
        foreach (var letter in indexedSet)
        {
            Console.WriteLine(letter);
        }
    }

    [Test]
    public void TestIndexOf()
    {
        IndexedSet<string> indexedSet = [
            "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta",
            "alpha1", "beta1", "gamma1", "delta1", "epsilon1", "zeta1", "eta1",
            "alpha2", "beta2", "gamma2", "delta2", "epsilon2", "zeta2", "eta2"
        ];
        Assert.That(indexedSet.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedSet.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedSet.IndexOf("beta", 2), Is.EqualTo(-1));
        Assert.That(indexedSet.IndexOf("beta", 2, 4), Is.EqualTo(-1));
        Assert.That(indexedSet.IndexOf("beta2", 15, 4), Is.EqualTo(15));
        Assert.That(indexedSet.LastIndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedSet.LastIndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedSet.LastIndexOf("beta", 7), Is.EqualTo(1));
        Assert.That(indexedSet.LastIndexOf("beta", 20, 4), Is.EqualTo(-1));
        Assert.That(indexedSet.LastIndexOf("beta2", 20, 20), Is.EqualTo(15));
    }

}