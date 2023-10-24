using System.Drawing;
using Quadtree.StructureClasses.Node;

namespace QuadTreeTests.Node;

public class NodeTest
{
    private QuadTreeNodeLeaf<string> _testNodeLeaf;
    private QuadTreeNodeData<string> _testNodeLeafData;
    [SetUp]
    public void Setup()
    {
        _testNodeLeafData = new(new(12, 10), new(18, 18), "data");
        _testNodeLeaf = new(new(10, 10), new(20, 20), _testNodeLeafData);
    }
    
    [Test] 
    public void NodeContainsNode()
    {
        Assert.True(_testNodeLeaf.ContainNode(_testNodeLeafData));
        Assert.False(_testNodeLeafData.ContainNode(_testNodeLeaf));
    }
    
    [Test] 
    public void NodeContainsPoints()
    {
        Assert.True(_testNodeLeaf.ContainsPoints(new(12, 10), new(18, 18)));
        Assert.True(_testNodeLeaf.ContainsPoints(new(18, 18), new(12, 10)));
        Assert.False(_testNodeLeaf.ContainsPoints(new(120, 100), new(180, 180)));
        Assert.False(_testNodeLeaf.ContainsPoints(new(12, 10), new(21, 21)));
    }
    
    [Test]
    public void GetData()
    {
        Assert.That(_testNodeLeaf.GetData(0).Data, Is.EqualTo("data"));
        Assert.Throws<ArgumentOutOfRangeException>(()=>_testNodeLeaf.GetData(1));
        Assert.NotNull(_testNodeLeaf.GetArrayListData());
        Assert.That(_testNodeLeaf.DataCount(), Is.EqualTo(1));
        QuadTreeNodeData<string> data = new(new(12, 10), new(20, 20),"data2");
        QuadTreeNodeData<string> dataErr = new(new(12, 10), new(21, 21),"data2");
        _testNodeLeaf.AddData(data);
        Assert.Throws<Exception>(()=>_testNodeLeaf.AddData(dataErr));
        Assert.That(_testNodeLeaf.GetData(1).Data, Is.EqualTo("data2"));
        Assert.That(_testNodeLeaf.DataCount(), Is.EqualTo(2));
        _testNodeLeaf.RemoveData(data);
        Assert.That(_testNodeLeaf.DataCount(), Is.EqualTo(1));
        List<QuadTreeNodeData<string>> tmp = new();
        tmp.Add(data);
        tmp.Add(new(new(12, 10), new(21, 21), "data3"));
        _testNodeLeaf.AddData(tmp);
        Assert.That(_testNodeLeaf.DataCount(), Is.EqualTo(3));
        Assert.That(_testNodeLeaf.GetData(2).Data, Is.EqualTo("data3"));
        Assert.False(_testNodeLeaf.DataIsEmpty());
        _testNodeLeaf.ClearData();
        Assert.True(_testNodeLeaf.DataIsEmpty());
    }

    [Test]
    public void OverlapTest()
    {
        QuadTreeNodeLeaf<string> testNodeLeaf2 = new(new(0, 0), new(20, 20), _testNodeLeafData);
        
        QuadTreeNodeLeaf<string> testNodeLeaf3 = new(new(10, 10), new(30, 30), _testNodeLeafData);
        QuadTreeNodeLeaf<string> testNodeLeaf4 = new(new(10, 10), new(15, 15), _testNodeLeafData);
        QuadTreeNodeLeaf<string> testNodeLeaf5 = new(new(-10, -10), new(15, 15), _testNodeLeafData);
        QuadTreeNodeLeaf<string> testNodeLeaf6 = new(new(-20, -20), new(0, 0), _testNodeLeafData);
        QuadTreeNodeLeaf<string> testNodeLeaf7 = new(new(-20, -20), new(-1, -1), _testNodeLeafData);
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf3));
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf4));
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf5));
        Assert.False(testNodeLeaf2.OverlapNode(testNodeLeaf6));
        Assert.False(testNodeLeaf2.OverlapNode(testNodeLeaf7));
    }

    [Test]
    public void TestInitLeafs()
    {
        QuadTreeNodeLeaf<string> testNodeLeaf2 = new(new(0, 0), new(20, 20), _testNodeLeafData);
        Assert.False(testNodeLeaf2.LeafsInicialised);
        Assert.False(testNodeLeaf2.LeafsInicialised);
        var Leafs = testNodeLeaf2.TestInitLeafs();
        Assert.True(testNodeLeaf2.LeafsInicialised);
        Assert.That(Leafs[0].PointDownLeft, Is.EqualTo(new Point(0, 0)));
        Assert.That(Leafs[0].PointUpRight, Is.EqualTo(new Point(10, 10)));
        Assert.That(Leafs[1].PointDownLeft, Is.EqualTo(new Point(0, 10)));
        Assert.That(Leafs[1].PointUpRight, Is.EqualTo(new Point(10, 20)));
        Assert.That(Leafs[2].PointDownLeft, Is.EqualTo(new Point(10, 10)));
        Assert.That(Leafs[2].PointUpRight, Is.EqualTo(new Point(20, 20)));
        Assert.That(Leafs[3].PointDownLeft, Is.EqualTo(new Point(10, 0)));
        Assert.That(Leafs[3].PointUpRight, Is.EqualTo(new Point(20, 10)));
        
        QuadTreeNodeLeaf<string> testNodeLeaf3 = new(new(-50, -50), new(50, 50), _testNodeLeafData);
        Assert.False(testNodeLeaf3.LeafsInicialised);
        Assert.False(testNodeLeaf3.LeafsInicialised);
        Leafs = testNodeLeaf3.TestInitLeafs();
        Assert.True(testNodeLeaf3.LeafsInicialised);
        Assert.That(Leafs[0].PointDownLeft, Is.EqualTo(new Point(-50, -50)));
        Assert.That(Leafs[0].PointUpRight, Is.EqualTo(new Point(0, 0)));
        Assert.That(Leafs[1].PointDownLeft, Is.EqualTo(new Point(-50, 0)));
        Assert.That(Leafs[1].PointUpRight, Is.EqualTo(new Point(0, 50)));
        Assert.That(Leafs[2].PointDownLeft, Is.EqualTo(new Point(0, 0)));
        Assert.That(Leafs[2].PointUpRight, Is.EqualTo(new Point(50, 50)));
        Assert.That(Leafs[3].PointDownLeft, Is.EqualTo(new Point(0, -50)));
        Assert.That(Leafs[3].PointUpRight, Is.EqualTo(new Point(50, 0)));
    }


    [Test]
    public void GetOverlapingLeafs()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.TestInitLeafs();
        QuadTreeNodeLeaf<int> searchArea = new(new(5, 5), new(15, 15));
        List<QuadTreeNodeLeaf<int>> leafs = testNodeLeaf2.GetOverlapingLefs(searchArea);
        Assert.That(leafs.Count, Is.EqualTo(4));
        Assert.That(leafs[0].PointDownLeft, Is.EqualTo(new Point(0, 0)));
        Assert.That(leafs[0].PointUpRight, Is.EqualTo(new Point(10, 10)));
        Assert.That(leafs[1].PointDownLeft, Is.EqualTo(new Point(0, 10)));
        Assert.That(leafs[1].PointUpRight, Is.EqualTo(new Point(10, 20)));
        Assert.That(leafs[2].PointDownLeft, Is.EqualTo(new Point(10, 10)));
        Assert.That(leafs[2].PointUpRight, Is.EqualTo(new Point(20, 20)));
        Assert.That(leafs[3].PointDownLeft, Is.EqualTo(new Point(10, 0)));
        Assert.That(leafs[3].PointUpRight, Is.EqualTo(new Point(20, 10)));
        
        QuadTreeNodeLeaf<int> searchArea2 = new(new(50, 50), new(150, 150));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea2);
        Assert.That(leafs.Count, Is.EqualTo(0));
        
        QuadTreeNodeLeaf<int> searchArea3 = new(new(30, 30), new(50, 50));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea3);
        Assert.That(leafs.Count, Is.EqualTo(0));
        
        QuadTreeNodeLeaf<int> searchArea4 = new(new(-50, -50), new(1, 1));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea4);
        Assert.That(leafs.Count, Is.EqualTo(1));
        Assert.That(leafs[0].PointDownLeft, Is.EqualTo(new Point(0, 0)));
        Assert.That(leafs[0].PointUpRight, Is.EqualTo(new Point(10, 10)));
    }

    [Test]
    public void AnyInitSubNodeContainDataNode()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        QuadTreeNodeLeaf<int> searchArea = new(new(15, 15), new(20, 20));
        
        Assert.False(testNodeLeaf2.AnyInitSubNodeContainDataNode(searchArea));
        testNodeLeaf2.TestInitLeafs();
        Assert.True(testNodeLeaf2.AnyInitSubNodeContainDataNode(searchArea));
        
        QuadTreeNodeLeaf<int> searchArea2 = new(new(5, 5), new(20, 20));
        Assert.False(testNodeLeaf2.AnyInitSubNodeContainDataNode(searchArea2));
    }
    
    [Test]
    public void AnySubNodeContainDataNode()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        QuadTreeNodeLeaf<int> searchArea = new(new(15, 15), new(20, 20));
        
        Assert.True(testNodeLeaf2.AnySubNodeContainDataNode(searchArea));
        
        testNodeLeaf2 = new(new(0, 0), new(20, 20));
        QuadTreeNodeLeaf<int> searchArea2 = new(new(5, 5), new(20, 20));
        Assert.False(testNodeLeaf2.AnySubNodeContainDataNode(searchArea2));
    }

    [Test]
    public void GetLeafThatCanContainDataNode()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.TestInitLeafs();
        QuadTreeNodeLeaf<int> searchArea = new(new(15, 15), new(20, 20));
        var tmp = testNodeLeaf2.GetLeafThatCanContainDataNode(searchArea);
        Assert.NotNull(tmp);
        Assert.That(tmp.PointDownLeft, Is.EqualTo(new Point(10, 10)));
        QuadTreeNodeLeaf<int> searchArea2 = new(new(20, 20), new(25, 25));
        tmp = testNodeLeaf2.GetLeafThatCanContainDataNode(searchArea2);
        Assert.Null(tmp);

    }

    [Test]
    public void RemoveDataInRange()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.TestInitLeafs();
        QuadTreeNodeData<int> data = new(new(15, 15), new(20, 20), 1);
        QuadTreeNodeData<int> data2 = new(new(10, 10), new(20, 20), 2);
        testNodeLeaf2.AddData(data);
        testNodeLeaf2.AddData(data2);
        QuadTreeNodeLeaf<int> searchArea = new(new(15, 15), new(20, 20));
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(2));
        var tmp = testNodeLeaf2.RemoveDataInRange(searchArea);
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.That(tmp[0], Is.EqualTo(1));
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(1));
    }
    
    [Test]
    public void GetDataInRange()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.TestInitLeafs();
        QuadTreeNodeData<int> data = new(new(15, 15), new(20, 20), 1);
        QuadTreeNodeData<int> data2 = new(new(10, 10), new(20, 20), 2);
        testNodeLeaf2.AddData(data);
        testNodeLeaf2.AddData(data2);
        QuadTreeNodeLeaf<int> searchArea = new(new(15, 15), new(20, 20));
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(2));
        var tmp = testNodeLeaf2.GetDataInRange(searchArea);
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.That(tmp[0], Is.EqualTo(1));
    }
}