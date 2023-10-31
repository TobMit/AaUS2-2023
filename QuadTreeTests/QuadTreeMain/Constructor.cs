using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class Constructor
{
    [Test]
    public void QuadTreeRound()
    {
        QuadTree<int> tmp;
        var ex = Assert.Throws<Exception>(() => new QuadTree<int>(0.0, 0.0, 180.0, 90.0, 0));
        Assert.That(ex.Message, Is.EqualTo("Min depth is 1"));
        Assert.DoesNotThrow(() => new QuadTree<int>(0.0, 0.0, 180.0, 90.0, 10));
        Assert.DoesNotThrow(() => new QuadTree<int>(0.0, 0.0, 180.0, 90.0));
    }
}