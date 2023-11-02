namespace QuadTreeTests.QuadTreeMain;
using Quadtree.StructureClasses;
public class Utils
{
    [Test]
    public void Recount()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Assert.That(quadtree.Count, Is.EqualTo(quadtree.Recount()));
        Assert.That(quadtree.Recount(), Is.EqualTo(6));
    }
    
    [Test]
    public void SetNewDepth()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        quadtree.SetQuadTreeDepth(2);
        Assert.That(quadtree.Count, Is.EqualTo(quadtree.Recount()));
        Assert.That(quadtree.Recount(), Is.EqualTo(6));
        var tmp = quadtree.FindOverlapingData(-20.0, -20.0, -1, -1);
        Assert.That(tmp.Count, Is.EqualTo(6));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(2));
    }
    
    [Test]
    public void ToList()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        quadtree.SetQuadTreeDepth(2);
        Assert.That(quadtree.Count, Is.EqualTo(quadtree.Recount()));
        Assert.That(quadtree.Recount(), Is.EqualTo(6));
        var tmp = quadtree.ToList();
        Assert.That(tmp.Count, Is.EqualTo(6));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(2));
    }
}