using System.Drawing;
using Quadtree.StructureClasses.HelperClass;
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
        QuadTreeNodeLeaf<string> testNodeLeaf8 = new(new(-45, -45), new (-10, -10), _testNodeLeafData);
        QuadTreeNodeLeaf<string> testNodeLeaf9 = new(new(-80, -80), new (80, 80), _testNodeLeafData);
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf3));
        Assert.True(testNodeLeaf3.OverlapNode(testNodeLeaf2));
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf4));
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf5));
        Assert.True(testNodeLeaf5.OverlapNode(testNodeLeaf2));
        Assert.False(testNodeLeaf2.OverlapNode(testNodeLeaf6));
        Assert.False(testNodeLeaf2.OverlapNode(testNodeLeaf7));
        Assert.True(testNodeLeaf8.OverlapNode(testNodeLeaf7));
        Assert.True(testNodeLeaf7.OverlapNode(testNodeLeaf8));
        Assert.True(testNodeLeaf9.OverlapNode(testNodeLeaf2));
        Assert.True(testNodeLeaf2.OverlapNode(testNodeLeaf9));
    }

    [Test]
    public void TestInitLeafs()
    {
        QuadTreeNodeLeaf<string> testNodeLeaf2 = new(new(0, 0), new(20, 20), _testNodeLeafData);
        Assert.False(testNodeLeaf2.LeafsInicialised);
        Assert.False(testNodeLeaf2.LeafsInicialised);
        var Leafs = testNodeLeaf2.TestInitLeafs();
        Assert.True(testNodeLeaf2.LeafsInicialised);
        Assert.That(Leafs[0].PointDownLeft, Is.EqualTo(new PointD(0, 0)));
        Assert.That(Leafs[0].PointUpRight, Is.EqualTo(new PointD(10, 10)));
        Assert.That(Leafs[1].PointDownLeft, Is.EqualTo(new PointD(0, 10)));
        Assert.That(Leafs[1].PointUpRight, Is.EqualTo(new PointD(10, 20)));
        Assert.That(Leafs[2].PointDownLeft, Is.EqualTo(new PointD(10, 10)));
        Assert.That(Leafs[2].PointUpRight, Is.EqualTo(new PointD(20, 20)));
        Assert.That(Leafs[3].PointDownLeft, Is.EqualTo(new PointD(10, 0)));
        Assert.That(Leafs[3].PointUpRight, Is.EqualTo(new PointD(20, 10)));
        
        QuadTreeNodeLeaf<string> testNodeLeaf3 = new(new(-50, -50), new(50, 50), _testNodeLeafData);
        Assert.False(testNodeLeaf3.LeafsInicialised);
        Assert.False(testNodeLeaf3.LeafsInicialised);
        Leafs = testNodeLeaf3.TestInitLeafs();
        Assert.True(testNodeLeaf3.LeafsInicialised);
        Assert.That(Leafs[0].PointDownLeft, Is.EqualTo(new PointD(-50, -50)));
        Assert.That(Leafs[0].PointUpRight, Is.EqualTo(new PointD(0, 0)));
        Assert.That(Leafs[1].PointDownLeft, Is.EqualTo(new PointD(-50, 0)));
        Assert.That(Leafs[1].PointUpRight, Is.EqualTo(new PointD(0, 50)));
        Assert.That(Leafs[2].PointDownLeft, Is.EqualTo(new PointD(0, 0)));
        Assert.That(Leafs[2].PointUpRight, Is.EqualTo(new PointD(50, 50)));
        Assert.That(Leafs[3].PointDownLeft, Is.EqualTo(new PointD(0, -50)));
        Assert.That(Leafs[3].PointUpRight, Is.EqualTo(new PointD(50, 0)));
    }


    [Test]
    public void GetOverlapingLeafs()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.TestInitLeafs();
        QuadTreeNodeLeaf<int> searchArea = new(new(5, 5), new(15, 15));
        List<QuadTreeNodeLeaf<int>> leafs = testNodeLeaf2.GetOverlapingLefs(searchArea);
        Assert.That(leafs.Count, Is.EqualTo(4));
        Assert.That(leafs[0].PointDownLeft, Is.EqualTo(new PointD(0, 0)));
        Assert.That(leafs[0].PointUpRight, Is.EqualTo(new PointD(10, 10)));
        Assert.That(leafs[1].PointDownLeft, Is.EqualTo(new PointD(0, 10)));
        Assert.That(leafs[1].PointUpRight, Is.EqualTo(new PointD(10, 20)));
        Assert.That(leafs[2].PointDownLeft, Is.EqualTo(new PointD(10, 10)));
        Assert.That(leafs[2].PointUpRight, Is.EqualTo(new PointD(20, 20)));
        Assert.That(leafs[3].PointDownLeft, Is.EqualTo(new PointD(10, 0)));
        Assert.That(leafs[3].PointUpRight, Is.EqualTo(new PointD(20, 10)));
        
        QuadTreeNodeLeaf<int> searchArea2 = new(new(50, 50), new(150, 150));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea2);
        Assert.That(leafs.Count, Is.EqualTo(0));
        
        QuadTreeNodeLeaf<int> searchArea3 = new(new(30, 30), new(50, 50));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea3);
        Assert.That(leafs.Count, Is.EqualTo(0));
        
        QuadTreeNodeLeaf<int> searchArea4 = new(new(-50, -50), new(1, 1));
        leafs = testNodeLeaf2.GetOverlapingLefs(searchArea4);
        Assert.That(leafs.Count, Is.EqualTo(1));
        Assert.That(leafs[0].PointDownLeft, Is.EqualTo(new PointD(0, 0)));
        Assert.That(leafs[0].PointUpRight, Is.EqualTo(new PointD(10, 10)));
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
        Assert.That(tmp.PointDownLeft, Is.EqualTo(new PointD(10, 10)));
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

    [Test]
    public void SamePoints()
    {
        QuadTreeNodeData<string> testNodeData = new(new(10, 10), new(20, 20), "data");
        QuadTreeNodeData<string> testNodeData2 = new(new(11, 10), new(20, 20), "data");
        Assert.True(_testNodeLeaf.HaveSamePoints(testNodeData));
        Assert.False(_testNodeLeaf.HaveSamePoints(testNodeData2));
    }

    [Test]
    public void DataWithSamePoints()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(10, 10), new(20, 20));
        QuadTreeNodeData<int> testNodeData = new(new(10, 10), new(20, 20), 1);
        QuadTreeNodeData<int> testNodeData2 = new(new(10, 10), new(20, 20), 2);
        QuadTreeNodeData<int> testNodeData3 = new(new(11, 10), new(20, 20), 3);

        QuadTreeNodeLeaf<int> searchData = new(new(10, 10), new(20, 20));
        QuadTreeNodeLeaf<int> searchData2 = new(new(11, 10), new(20, 20));
        
        testNodeLeaf2.AddData(testNodeData);
        testNodeLeaf2.AddData(testNodeData2);
        testNodeLeaf2.AddData(testNodeData3);

        var tmp = testNodeLeaf2.GetDataWithSamePoints(searchData);
        Assert.That(tmp.Count, Is.EqualTo(2));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(2));
        
        tmp = testNodeLeaf2.GetDataWithSamePoints(searchData2);
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.True(tmp.Contains(3));
        
        tmp = testNodeLeaf2.RemoveDataWithSamePoints(searchData);
        Assert.That(tmp.Count, Is.EqualTo(2));
        Assert.True(tmp.Contains(1));
        Assert.True(tmp.Contains(2));
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(1));
        
        tmp = testNodeLeaf2.RemoveDataWithSamePoints(searchData2);
        Assert.That(tmp.Count, Is.EqualTo(1));
        Assert.True(tmp.Contains(3));
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(0));
    }


    [Test]
    public void CanBeRemoved()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(10, 10), new(20, 20));
        Assert.True(testNodeLeaf2.CanLeafsBeRemoved());
        Assert.False(testNodeLeaf2.LeafsInicialised);
        testNodeLeaf2.TestInitLeafs();
        Assert.True(testNodeLeaf2.LeafsInicialised);
        Assert.True(testNodeLeaf2.CanLeafsBeRemoved());
        Assert.False(testNodeLeaf2.LeafsInicialised);
        
        var tmpLeafs = testNodeLeaf2.TestInitLeafs();
        tmpLeafs[0].AddData(new QuadTreeNodeData<int>(new(10, 10), new(15, 15), 1));
        Assert.True(testNodeLeaf2.CanLeafsBeRemoved());
        Assert.That(testNodeLeaf2.DataCount(), Is.EqualTo(1));
        Assert.False(testNodeLeaf2.LeafsInicialised);
        
        // skontroluje keď je medzi dvoma vrstvami vrstva tkorá nemá data ale jej potomok má, tak nemôže byť zmazaná
        tmpLeafs = testNodeLeaf2.TestInitLeafs();
        var innerLeafs = tmpLeafs[0].TestInitLeafs();
        var ex = Assert.Throws<Exception>(() => innerLeafs[0].AddData(new QuadTreeNodeData<int>(new(10, 10), new(15, 15), 1)));
        Assert.That(ex.Message, Is.EqualTo("Data is not in this node. This shouldn't happened"));
        innerLeafs[0].AddData(new QuadTreeNodeData<int>(new(10, 10), new(12.5, 12.5), 1));
        Assert.False(testNodeLeaf2.CanLeafsBeRemoved());
        Assert.That(tmpLeafs[0].DataCount(), Is.EqualTo(0));
        Assert.True(testNodeLeaf2.LeafsInicialised);
        
        tmpLeafs = testNodeLeaf2.TestInitLeafs();
        tmpLeafs[0].TestInitLeafs();
        Assert.False(testNodeLeaf2.CanLeafsBeRemoved());
        Assert.True(testNodeLeaf2.LeafsInicialised);
        Assert.True(tmpLeafs[0].LeafsInicialised);
    }

    [Test]
    public void GetOverlapingData()
    {
        QuadTreeNodeLeaf<int> testNodeLeaf2 = new(new(0, 0), new(20, 20));
        testNodeLeaf2.AddData(new QuadTreeNodeData<int>(new(0,0), new (10,10), 1));
        testNodeLeaf2.AddData(new QuadTreeNodeData<int>(new(5,5), new (15,15), 2));
        testNodeLeaf2.AddData(new QuadTreeNodeData<int>(new(10,10), new (20,20), 3));
        
        var tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(0,0), new (10,10)));
        Assert.That(tmpData.Count, Is.EqualTo(2));
        Assert.That(tmpData[0], Is.EqualTo(1));
        Assert.That(tmpData[1], Is.EqualTo(2));
        
        tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(5,5), new (15,15)));
        Assert.That(tmpData.Count, Is.EqualTo(3));
        Assert.That(tmpData[0], Is.EqualTo(1));
        Assert.That(tmpData[1], Is.EqualTo(2));
        Assert.That(tmpData[2], Is.EqualTo(3));
        
        tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(12,12), new (30,30)));
        Assert.That(tmpData.Count, Is.EqualTo(2));
        Assert.That(tmpData[0], Is.EqualTo(2));
        Assert.That(tmpData[1], Is.EqualTo(3));
        
        tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(15,15), new (30,30)));
        Assert.That(tmpData.Count, Is.EqualTo(1));
        Assert.That(tmpData[0], Is.EqualTo(3));
        
        tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(20,20), new (30,30)));
        Assert.That(tmpData.Count, Is.EqualTo(0));
        
        tmpData = testNodeLeaf2.GetOverlapingData(new QuadTreeNodeLeaf<int>(new(-20,-20), new (0,0)));
        Assert.That(tmpData.Count, Is.EqualTo(0));
    }

    /// <summary>
    /// Test skontroluje či sa dajú zadávať súradnice aj v drumo variante. Základny varian je lavý dolný roh a pravý horný roh, ruhý variant je lavý horný roh a pravý dolný roh.
    /// </summary>
    [Test]
    public void CoordinationTest()
    {
        //todo toto je ešte na prehodnotenie, lebo to nefunguje správne
        QuadTreeNodeLeaf<string> testNodeLeaf2 = new(new(11,  11), new(19, 19));
        QuadTreeNodeLeaf<string> testNodeLeaf3 = new(new(11,  19), new(19, 11));
        Assert.True(_testNodeLeaf.ContainNode(testNodeLeaf2));
        Assert.True(_testNodeLeaf.ContainNode(testNodeLeaf3));
        
        Assert.True(_testNodeLeaf.OverlapNode(testNodeLeaf2));
        Assert.True(_testNodeLeaf.OverlapNode(testNodeLeaf3));
    }
    
}