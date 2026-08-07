using Dastardly.Data;

namespace Tests;

public class IndexedDictionaryTest
{
    private static IndexedDictionary<string, string> GenerateFull() 
    {
        IndexedDictionary<string, string> indexedDictionary = new(8);
        indexedDictionary.Add("alpha", "alpha");
        indexedDictionary.Add("beta", "beta");
        indexedDictionary.Add("gamma", "gamma");
        indexedDictionary.Add("delta", "delta");
        indexedDictionary.Add("epsilon", "epsilon");
        indexedDictionary.Add("zeta", "zeta");
        indexedDictionary.Add("eta", "eta");
        indexedDictionary.Add("theta", "theta");
        return indexedDictionary;
    }

    private static IndexedDictionary<string, string> GeneratePartial() 
    {
        IndexedDictionary<string, string> indexedDictionary = new(8);
        indexedDictionary.Add("alpha", "alpha");
        indexedDictionary.Add("beta", "beta");
        indexedDictionary.Add("gamma", "gamma");
        indexedDictionary.Add("delta", "delta");
        indexedDictionary.Add("epsilon", "epsilon");
        indexedDictionary.Add("zeta", "zeta");
        return indexedDictionary;
    }

    private static IndexedDictionary<string, string> GeneratePartialMissingFirst() 
    {
        IndexedDictionary<string, string> indexedDictionary = new(8);
        indexedDictionary.Add("", "");
        indexedDictionary.Add("alpha", "alpha");
        indexedDictionary.Add("beta", "beta");
        indexedDictionary.Add("gamma", "gamma");
        indexedDictionary.Add("delta", "delta");
        indexedDictionary.Add("epsilon", "epsilon");
        indexedDictionary.Add("zeta", "zeta");
        indexedDictionary.RemoveAt(0);
        return indexedDictionary;
    }

    private static IndexedDictionary<string, string> GeneratePartialWrapped() 
    {
        IndexedDictionary<string, string> indexedDictionary = new(8);
        indexedDictionary.Add("gamma", "gamma");
        indexedDictionary.Add("delta", "delta");
        indexedDictionary.Add("epsilon", "epsilon");
        indexedDictionary.Add("zeta", "zeta");
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("beta", "beta"));
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("alpha", "alpha"));
        return indexedDictionary;
    }
    
    private static IndexedDictionary<string, string>[] GenerateAll() => [GenerateFull(), GeneratePartial(),
        GeneratePartialMissingFirst(), GeneratePartialWrapped()];
    
    [SetUp]
    public void Setup() {

    }

    [Test]
    public void TestPushAndGet()
    {
        IndexedDictionary<string, string> indexedDictionary = new(16);
        indexedDictionary.Add("beta", "beta");
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("alpha", "alpha"));
        indexedDictionary.Add("gamma", "gamma");
        Assert.That(indexedDictionary["alpha"], Is.EqualTo("alpha"));
        Assert.That(indexedDictionary["beta"], Is.EqualTo("beta"));
        Assert.That(indexedDictionary["gamma"], Is.EqualTo("gamma"));
    }
/*
    [Test]
    public void TestHeadWraps()
    {
        IndexedDictionary<string, string> indexedDictionary = new(4);
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("gamma", "gamma"));
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("beta", "beta"));
        indexedDictionary.PushFirst(new KeyValuePair<string, string>("alpha", "alpha"));
        Assert.That(indexedDictionary["alpha"], Is.EqualTo("alpha"));
        Assert.That(indexedDictionary["beta"], Is.EqualTo("beta"));
        Assert.That(indexedDictionary["gamma"], Is.EqualTo("gamma"));
        Assert.That(indexedDictionary.IndexOf(new KeyValuePair<string, string>("alpha", "alpha")), Is.EqualTo(0));
        Assert.That(indexedDictionary.IndexOf(new KeyValuePair<string, string>("beta", "beta")), Is.EqualTo(1));
        Assert.That(indexedDictionary.IndexOf(new KeyValuePair<string, string>("gamma", "gamma")), Is.EqualTo(2));
        Assert.That(indexedDictionary.IndexOf(new KeyValuePair<string, string>("not here", "not here")), Is.EqualTo(-1));

        var received = indexedDictionary.PopAt(1).Key;
        Assert.That(received, Is.EqualTo("beta"));
        Assert.That(indexedDictionary[1], Is.EqualTo("gamma"));
        Assert.That(indexedDictionary.IndexOf("beta"), Is.EqualTo(-1));
        Assert.That(indexedDictionary.IndexOf("gamma"), Is.EqualTo(1));
    }

    [Test]
    public void TestEnumerator()
    {
        IndexedDictionary<string, string> indexedDictionary = new(4);
        indexedDictionary.PushLast("beta");
        indexedDictionary.PushFirst("alpha");
        indexedDictionary.PushLast("gamma");
        indexedDictionary.PushLast("delta");
        indexedDictionary.PushLast("epsilon");
        indexedDictionary.PushLast("eta");
        indexedDictionary.PushFirst("OOPS");
        indexedDictionary.PopFirst();
        indexedDictionary.Insert(5, "zeta");
        indexedDictionary.Insert(5, "BEFORE ZETA");
        indexedDictionary.RemoveAt(5);
        Assert.That(indexedDictionary, Is.EquivalentTo(["alpha", "beta", "gamma",  "delta", "epsilon", "zeta", "eta"]));
        foreach (var letter in indexedDictionary)
        {
            Console.WriteLine(letter);
        }
    }

    [Test]
    public void TestIndexOf()
    {
        IndexedDictionary<string, string> indexedDictionary = [
            "alpha", "beta", "gamma", "delta", "epsilon", "zeta", "eta",
            "alpha1", "beta1", "gamma1", "delta1", "epsilon1", "zeta1", "eta1",
            "alpha2", "beta2", "gamma2", "delta2", "epsilon2", "zeta2", "eta2"
        ];
        Assert.That(indexedDictionary.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedDictionary.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedDictionary.IndexOf("beta", 2), Is.EqualTo(-1));
        Assert.That(indexedDictionary.IndexOf("beta", 2, 4), Is.EqualTo(-1));
        Assert.That(indexedDictionary.IndexOf("beta2", 15, 4), Is.EqualTo(15));
        Assert.That(indexedDictionary.LastIndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedDictionary.LastIndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedDictionary.LastIndexOf("beta", 7), Is.EqualTo(1));
        Assert.That(indexedDictionary.LastIndexOf("beta", 20, 4), Is.EqualTo(-1));
        Assert.That(indexedDictionary.LastIndexOf("beta2", 20, 20), Is.EqualTo(15));
    }

    [Test]
    public void TestIndexOfWrapping()
    {
        IndexedDictionary<string, string> indexedDictionary = 
        [                      "gamma",   "delta",  "epsilon",  "zeta",  "eta",
            "alpha1", "beta1", "gamma1",  "delta1", "epsilon1", "zeta1", "eta1",
            "alpha2", "beta2", "gamma2",  "delta2", "epsilon2", "zeta2", "eta2"];
        // makes head wrap around.
        indexedDictionary.PushFirst("beta");
        indexedDictionary.PushFirst("alpha");
        Assert.That(indexedDictionary.IndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedDictionary.IndexOf("beta"), Is.EqualTo(1));
        Assert.That(indexedDictionary.IndexOf("beta", 2), Is.EqualTo(-1));
        Assert.That(indexedDictionary.IndexOf("beta", 2, 4), Is.EqualTo(-1));
        Assert.That(indexedDictionary.IndexOf("beta2", 15, 4), Is.EqualTo(15));
        Assert.That(indexedDictionary.LastIndexOf("alpha"), Is.EqualTo(0));
        Assert.That(indexedDictionary.LastIndexOf("beta1"), Is.EqualTo(8));
        Assert.That(indexedDictionary.LastIndexOf("beta", 7), Is.EqualTo(1));
        Assert.That(indexedDictionary.LastIndexOf("beta", 20, 4), Is.EqualTo(-1));
        Assert.That(indexedDictionary.LastIndexOf("beta2", 20, 20), Is.EqualTo(15));
        Assert.That(indexedDictionary.LastIndexOf("beta", 2, 2), Is.EqualTo(1));
        Assert.That(indexedDictionary.LastIndexOf("beta1", 8, 8), Is.EqualTo(8));
    }

    [Test]
    public void TestListEquivalence()
    {
        var indexedDictionaries = GenerateAll();
        List<string, string>[] lists = [new(indexedDictionaries[0]), new(indexedDictionaries[1]), new(indexedDictionaries[2]), new(indexedDictionaries[3])];
        for (int i = 0; i < 4; i++)
        {
            Assert.That(indexedDictionaries[i], Is.EquivalentTo(lists[i]));
            indexedDictionaries[i].Reverse();
            lists[i].Reverse();
            Assert.That(indexedDictionaries[i], Is.EquivalentTo(lists[i]));
            indexedDictionaries[i].Reverse(2, 3);
            lists[i].Reverse(2, 3);
            Assert.That(indexedDictionaries[i], Is.EquivalentTo(lists[i]));
            indexedDictionaries[i].Sort();
            lists[i].Sort();
            Assert.That(indexedDictionaries[i], Is.EquivalentTo(lists[i]));
        }
    }

    [Test]
    public void TestRemoveAll()
    {
        var indexedDictionaries = GenerateAll();
        for (var i = 0; i < 4; i++)
        {
            indexedDictionaries[i].RemoveWhere(x => x.EndsWith('a'));
            Assert.That(indexedDictionaries[i], Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void TestDictionaryUniqueness()
    {
        var indexedDictionaries = GenerateAll();
        for (var i = 0; i < 4; i++)
        {
            var size = indexedDictionaries[i].Count;
            indexedDictionaries[i].Add("beta");
            Assert.That(indexedDictionaries[i], Has.Count.EqualTo(size));
            indexedDictionaries[i].Remove("beta");
            indexedDictionaries[i].RemoveAt(2);
            Assert.That(indexedDictionaries[i], Has.Count.EqualTo(size - 2));
            indexedDictionaries[i].Add("beta");
            indexedDictionaries[i].Add("beta");
            Assert.That(indexedDictionaries[i], Has.Count.EqualTo(size - 1));
        }
    }
*/
}