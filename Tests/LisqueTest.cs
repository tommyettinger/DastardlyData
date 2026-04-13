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
    }
}