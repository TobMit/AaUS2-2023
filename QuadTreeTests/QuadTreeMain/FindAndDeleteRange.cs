using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class FindAndDeleteRange
{
    [Test]
    public void BasicDeleteInterval()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        var tmp = quadtree.DeleteInterval(-20.0, -20.0, -1, -1);
        Assert.That(quadtree.Count, Is.EqualTo(2));
        Assert.That(tmp.Count, Is.EqualTo(4));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        
        tmp = quadtree.DeleteInterval(-20.0, -20.0, -1, -1);
        Assert.That(quadtree.Count, Is.EqualTo(2));
        Assert.That(tmp.Count, Is.EqualTo(0));
        
        tmp = quadtree.DeleteInterval(-50, -50, 50, 50);
        Assert.That(quadtree.Count, Is.EqualTo(0));
        Assert.That(tmp.Count, Is.EqualTo(2));
        Assert.True(tmp.Contains(2));
        Assert.True(tmp.Contains(1));
        
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        Assert.That(quadtree.Count, Is.EqualTo(1));
        tmp = quadtree.DeleteInterval(-50, -50, 50, 50);
        Assert.That(quadtree.Count, Is.EqualTo(0));
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.True(tmp.Contains(5));
    }
    
    [Test]
    public void BasicFindRange()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        var tmp = quadtree.FindInterval(-20.0, -20.0, -1, -1);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        Assert.That(tmp.Count, Is.EqualTo(4));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        
        tmp = quadtree.FindInterval(-50, -50, 50, 50);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        Assert.That(tmp.Count, Is.EqualTo(6));
        Assert.True(tmp.Contains(2));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        
        tmp = quadtree.FindInterval(-6, -6, 1, 1);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        Assert.That(tmp.Count, Is.EqualTo(2));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
    }
    
    [Test]
    public void OverlapingFind()
    {
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 5);
        quadtree.Insert(-5.0, -5.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-20.0, -20.0, -10, -10, 3);
        quadtree.Insert(-10.0, -10.0, -1, -1, 4);
        Assert.That(quadtree.Count, Is.EqualTo(6));
        var tmp = quadtree.FindOverlapingData(-20.0, -20.0, -1, -1);
        Assert.That(tmp.Count, Is.EqualTo(6));
        Assert.True(tmp.Contains(3));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(2));
        
        tmp = quadtree.FindOverlapingData(-10.0, -10.0, -1, -1);
        Assert.That(tmp.Count, Is.EqualTo(4));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        Assert.True(tmp.Contains(1));
        
        tmp = quadtree.FindOverlapingData(0, 0, 80, 80);
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.True(tmp.Contains(1));
        
        tmp = quadtree.FindOverlapingData(-4, -4, -4, -4);
        Assert.That(tmp.Count, Is.EqualTo(4));
        Assert.True(tmp.Contains(4));
        Assert.True(tmp.Contains(5));
        Assert.True(tmp.Contains(6));
        Assert.True(tmp.Contains(1));
        
    }
}