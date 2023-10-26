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

    [Test]
    public void Recount()
    {
        QuadTree<int> quadtree = new QuadTree<int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Assert.That(quadtree.Count, Is.EqualTo(quadtree.Recount()));
        Assert.That(quadtree.Recount(), Is.EqualTo(6));
    }
}