using System.Collections;
using System.ComponentModel;
using Quadtree.StructureClasses;

namespace QuadTreeTests.Node;

public class NodeTest
{
    private QuadTreeNode<string> testNode;
    private QuadTreeNode<string> testNodeData;
    [SetUp]
    public void Setup()
    {
        testNode = new(new(10, 10), new(20, 20));
        testNodeData = new(new(12, 10), new(18, 18), "data");
    }

    [Test] 
    public void NodeReturnIsLease()
    {
        Assert.True(testNode.IsLeaf);
    }
    
    [Test] 
    public void DataNodeReturnIsLease()
    {
        Assert.False(testNodeData.IsLeaf);
    }
    
    [Test] 
    public void NodeContainsNode()
    {
        Assert.True(testNode.ContainNode(testNodeData));
        Assert.False(testNodeData.ContainNode(testNode));
    }
    
    [Test] 
    public void NodeContainsPoints()
    {
        Assert.True(testNode.ContainsPoints(new(12, 10), new(18, 18)));
        Assert.True(testNode.ContainsPoints(new(18, 18), new(12, 10)));
        Assert.False(testNode.ContainsPoints(new(120, 100), new(180, 180)));
        Assert.False(testNode.ContainsPoints(new(12, 10), new(21, 21)));
    }
    
    [Test]
    public void GetData()
    {
        Assert.AreEqual("data", testNodeData.GetData(0));
        Assert.Throws<ArgumentOutOfRangeException>(()=>testNodeData.GetData(1));
        Assert.NotNull(testNodeData.GetArrayListData());
        Assert.AreEqual(1, testNodeData.GetArrayListData().Count);
        testNodeData.AddData("data2");
        Assert.AreEqual("data2", testNodeData.GetData(1));
        Assert.AreEqual(2, testNodeData.GetArrayListData().Count);
        testNodeData.RemoveData("data2");
        Assert.AreEqual(1, testNodeData.GetArrayListData().Count);
        ArrayList tmp = new();
        tmp.Add("data2");
        tmp.Add("data3");
        testNodeData.AddData(tmp);
        Assert.AreEqual(3, testNodeData.GetArrayListData().Count);
        Assert.AreEqual("data3", testNodeData.GetData(2));
    }
    
}