using System.Collections;
using System.ComponentModel;
using Quadtree.StructureClasses;
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
    
}