using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class Insert
{
    private QuadTree<int, int> tmp;
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

    [Test]
    public void BasicInsert()
    {
        // Tento ukažkový strom je správny
        QuadTree<int, int> quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 4);
        quadtree.Insert(-5.0, -5.0, -1, -1, 7);
        quadtree.Insert(-5.0, -5.0, -1, -1, 8);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 3);
        quadtree.Insert(-20.0, -20.0, -10, -10, 5);
        quadtree.Insert(-10.0, -10.0, -1, -1, 6);
        quadtree.Insert(-45.0, -45.0, -10, -10, 4);
        Assert.That(quadtree.Count, Is.EqualTo(8));
        
        // Tento ukažkový strom je správny
        quadtree = new QuadTree<int, int>(-50, -50, 100, 100, 3);
        quadtree.Insert(-5.0, -5.0, -1, -1, 7);
        quadtree.Insert(-5.0, -5.0, -1, -1, 8);
        quadtree.Insert(-45.0, -45.0, 30, 30, 1);
        quadtree.Insert(-45.0, -45.0, -10, -10, 2);
        quadtree.Insert(-45.0, -45.0, -10, -10, 3);
        quadtree.Insert(-45.0, -45.0, -10, -10, 4);
        quadtree.Insert(-20.0, -20.0, -10, -10, 5);
        quadtree.Insert(-10.0, -10.0, -1, -1, 6);
        Assert.That(quadtree.Count, Is.EqualTo(8));
        
    }
}