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
}