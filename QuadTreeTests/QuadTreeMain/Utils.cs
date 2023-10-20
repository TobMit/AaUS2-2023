namespace QuadTreeTests.QuadTreeMain;
using Quadtree.StructureClasses;
public class Utils
{
    [Test]
    public void QuadTreeRound()
    {
        Assert.That(QuadTree<int>.TestQuadTreeRound(1.1234567), Is.EqualTo(11234567));
        Assert.That(QuadTree<int>.TestQuadTreeRound(1.1234), Is.EqualTo(11234000));
        Assert.That(QuadTree<int>.TestQuadTreeRound(1.12345678), Is.Not.EqualTo(112345678));
    }

    [Test]
    public void CheckCoordinates()
    {
        Assert.True(QuadTree<int>.TestCheckCoordinates(100000000, 100000000));
        Assert.True(QuadTree<int>.TestCheckCoordinates(-1800000000, -900000000));
        Assert.True(QuadTree<int>.TestCheckCoordinates(1800000000, 900000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(-1800000001, -900000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(-1800000000, -900000001));
        Assert.False(QuadTree<int>.TestCheckCoordinates(1800000001, 900000000));
        Assert.False(QuadTree<int>.TestCheckCoordinates(1800000000, 900000001));
    }
}