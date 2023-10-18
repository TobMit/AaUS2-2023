using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class Insert
{
    private QuadTree<int> tmp;
    [SetUp]
    public void Setup()
    {
        tmp = new(0.0, 0.0, 180.0, 90.0, 10);
    }
    [Test]
    public void InsertThorwException()
    {
        var ex = Assert.Throws<Exception>(() => tmp.Insert(0.0, 0.0, 181.0, 91.0, 10));
        Assert.That(ex.Message, Is.EqualTo("Coordinates exceed parameter size"));
        Assert.DoesNotThrow(() => tmp.Insert(0.0, 0.0, 180.0, 90.0, 10));
    }
}