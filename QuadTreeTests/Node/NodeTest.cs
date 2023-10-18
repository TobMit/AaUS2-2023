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
        Assert.True(testNode.containNode(testNodeData));
        Assert.False(testNodeData.containNode(testNode));
    }
    
    [Test] 
    public void NodeContainsPoints()
    {
        Assert.True(testNode.containsPoints(new(12, 10), new(18, 18)));
        Assert.True(testNode.containsPoints(new(18, 18), new(12, 10)));
        Assert.False(testNode.containsPoints(new(120, 100), new(180, 180)));
        Assert.False(testNode.containsPoints(new(12, 10), new(21, 21)));
    }
    
    [Test]
    public void GetData()
    {
        Assert.AreEqual("data", testNodeData.Data);
    }
    
}