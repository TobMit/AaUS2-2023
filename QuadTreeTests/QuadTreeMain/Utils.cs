namespace QuadTreeTests.QuadTreeMain;
using Quadtree.StructureClasses;
public class Utils
{
    [Test]
    public void QuadTreeRound()
    {
        Assert.AreEqual(QuadTree<int>.TestQuadTreeRound(1.12345),112345);
        Assert.AreEqual(QuadTree<int>.TestQuadTreeRound(1.1234),112340);
        Assert.AreNotEqual(QuadTree<int>.TestQuadTreeRound(1.123456),1123456);
    }

    [Test]
    public void CheckCoordinates()
    {
        Assert.True(QuadTree<int>.TestCheckCoordinates(1000000, 1000000));
        Assert.True(QuadTree<int>.TestCheckCoordinates(-18000000, -9000000));
        Assert.True(QuadTree<int>.TestCheckCoordinates(18000000, 9000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(-18000001, -9000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(-18000000, -9000001));
        Assert.False(QuadTree<int>.TestCheckCoordinates(18000001, 9000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(18000000, 9000001));
    }
}