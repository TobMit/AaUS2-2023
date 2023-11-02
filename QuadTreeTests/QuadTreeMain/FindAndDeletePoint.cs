using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class FindAndDeletePoint
{

    private QuadTree<int, int> _quadTree;
    [SetUp]
    public void SetUp()
    {
        _quadTree = new(-50, -50, 100, 100, 20);
        _quadTree.Insert(-5.0, -5.0, -1, -1, 7);
        _quadTree.Insert(-5.0, -5.0, -1, -1, 8);
        _quadTree.Insert(-45.0, -45.0, -10, -10, 2);
        _quadTree.Insert(-45.0, -45.0, 30, 30, 1);
        _quadTree.Insert(-45.0, -45.0, -10, -10, 3);
        _quadTree.Insert(-20.0, -20.0, -10, -10, 5);
        _quadTree.Insert(-10.0, -10.0, -1, -1, 6);
        _quadTree.Insert(-45.0, -45.0, -10, -10, 4);
    }

    [Test]
    [Order(0)]
    public void Find()
    {
        var tmpList = _quadTree.Find(-45.0, -45.0, 30, 30);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(1));
        
        tmpList = _quadTree.Find(-20.0, -20.0, -10, -10);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(5));
        
        tmpList = _quadTree.Find(-45.0, -45.0, -10, -10);
        Assert.That(tmpList.Count, Is.EqualTo(3));
        Assert.True(tmpList.Contains(2));
        Assert.True(tmpList.Contains(3));
        Assert.True(tmpList.Contains(4));
        
        tmpList = _quadTree.Find(-10.0, -10.0, -1, -1);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(6));
        
        tmpList = _quadTree.Find(-5.0, -5.0, -1, -1);
        Assert.That(tmpList.Count, Is.EqualTo(2));
        Assert.True(tmpList.Contains(7));
        Assert.True(tmpList.Contains(8));
    }
    
    [Test]
    [Order(1)]
    public void Delete()
    {
        var tmpList = _quadTree.Delete(-45.0, -45.0, 30, 30, 1);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(1));
        Assert.That(_quadTree.Count, Is.EqualTo(7));
        
        tmpList = _quadTree.Delete(-20.0, -20.0, -10, -10, 5);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(5));
        Assert.That(_quadTree.Count, Is.EqualTo(6));
        
        tmpList = _quadTree.Delete(-45.0, -45.0, -10, -10, 2);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.True(tmpList.Contains(2));
        Assert.That(_quadTree.Count, Is.EqualTo(5));
        
        tmpList = _quadTree.Delete(-45.0, -45.0, -10, -10, 3);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.True(tmpList.Contains(3));
        Assert.That(_quadTree.Count, Is.EqualTo(4));
        
        tmpList = _quadTree.Delete(-45.0, -45.0, -10, -10, 4);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.True(tmpList.Contains(4));
        Assert.That(_quadTree.Count, Is.EqualTo(3));
        
        tmpList = _quadTree.Delete(-45.0, -45.0, -10, -10, 2);
        Assert.That(tmpList.Count, Is.EqualTo(0));
        Assert.That(_quadTree.Count, Is.EqualTo(3));
        
        tmpList = _quadTree.Delete(-10.0, -10.0, -1, -1, 6);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.That(tmpList[0], Is.EqualTo(6));
        Assert.That(_quadTree.Count, Is.EqualTo(2));
        
        tmpList = _quadTree.Delete(-5.0, -5.0, -1, -1, 7);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.True(tmpList.Contains(7));
        Assert.That(_quadTree.Count, Is.EqualTo(1));
        
        tmpList = _quadTree.Delete(-5.0, -5.0, -1, -1, 8);
        Assert.That(tmpList.Count, Is.EqualTo(1));
        Assert.True(tmpList.Contains(8));
        Assert.That(_quadTree.Count, Is.EqualTo(0));
    }
}