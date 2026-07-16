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

}