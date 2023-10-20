using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class Constructor
{
    [Test]
    public void QuadTreeRound()
    {
        QuadTree<int> tmp;
        var ex = Assert.Throws<Exception>(() => new QuadTree<int>(0.0, 0.0, 181.0, 91.0, 10));
        Assert.That(ex.Message, Is.EqualTo("Wrong world coordination"));
        ex = Assert.Throws<Exception>(() => new QuadTree<int>(0.0, 0.0, 180.0, 90.0, 30));
        Assert.That(ex.Message, Is.EqualTo("Max depth is 29"));
        Assert.DoesNotThrow(() => new QuadTree<int>(0.0, 0.0, 180.0, 90.0, 10));
    }
}