using Quadtree.StructureClasses;

namespace QuadTreeTests.QuadTreeMain;

public class Optimalisation
{
    private QuadTree<int, int> quadTree;
    private Random rnd;

    private double MAX_SIZE_OF_OBJCET_PERCENTAGE = 0.25;
    
    [SetUp]
    public void Setup()
    {
        rnd = new Random();
    }

    [Test]
    public void ScaleUpAndDown()
    {
        quadTree = new(0.0, 0.0, 100.0, 100.0);
        quadTree.OptimalizationOn = false;
        
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(0, 100-2, rnd);
            double y = Program.NextDouble(51, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(10000));
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo(quadTree.OriginalPointUpRight.X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo((quadTree.OriginalPointUpRight*1.3).Y).Within(0.0001));
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));

        // budem generovať dáta v dolnej polovičke aby som vynutil optimalizáciou posun dole
        quadTree.OptimalizationOn = false;
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(0, 100-2, rnd);
            double y = Program.NextDouble(0, 50-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(20000));
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo(quadTree.OriginalPointUpRight.X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo((quadTree.OriginalPointUpRight*1.3*1.3*0.7).Y).Within(0.0001));

    }
    
    [Test]
    public void ScaleDowAndUp()
    {
        quadTree = new(0.0, 0.0, 100.0, 100.0);
        quadTree.OptimalizationOn = false;
        
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(0, 100-2, rnd);
            double y = Program.NextDouble(0, 50-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(10000));
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.X, Is.EqualTo(quadTree.OriginalPointDownLeft.X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.Y, Is.EqualTo(-30.0).Within(0.0001));
        Console.WriteLine(quadTree.Health);
        
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));

        
        quadTree.OptimalizationOn = false;
        for (int i = 0; i < 20000; i++)
        {
            double x = Program.NextDouble(0, 100-2, rnd);
            double y = Program.NextDouble(51, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(30000));
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo(quadTree.OriginalPointUpRight.X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo(139.0).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.X, Is.EqualTo(quadTree.OriginalPointDownLeft.X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.Y, Is.EqualTo(-30.0).Within(0.0001));
    }
    
    [Test]
    public void ScaleRightAndLeft()
    {
        quadTree = new(0.0, 0.0, 100.0, 100.0);
        quadTree.OptimalizationOn = false;
        
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(51, 100-2, rnd);
            double y = Program.NextDouble(0, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(10000));
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo((quadTree.OriginalPointUpRight*1.3).X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo(quadTree.OriginalPointUpRight.Y).Within(0.0001));
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));

        // budem generovať dáta v dolnej polovičke aby som vynutil optimalizáciou posun dole
        quadTree.OptimalizationOn = false;
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(0, 50-2, rnd);
            double y = Program.NextDouble(0, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(20000));
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo((quadTree.OriginalPointUpRight*1.3*1.3*0.7).X).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo(quadTree.OriginalPointUpRight.Y).Within(0.0001));

    }
    
    [Test]
    public void ScaleLeftAndRight()
    {
        quadTree = new(0.0, 0.0, 100.0, 100.0);
        quadTree.OptimalizationOn = false;
        
        for (int i = 0; i < 10000; i++)
        {
            double x = Program.NextDouble(0, 50-2, rnd);
            double y = Program.NextDouble(0, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 50 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(10000));
        Assert.That(quadTree.TestGetRoot().PointDownLeft, Is.EqualTo(quadTree.OriginalPointDownLeft));
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointUpRight, Is.EqualTo(quadTree.OriginalPointUpRight));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.X, Is.EqualTo(-30.0).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.Y, Is.EqualTo(quadTree.OriginalPointDownLeft.Y).Within(0.0001));
        Console.WriteLine(quadTree.Health);
        
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));

        
        quadTree.OptimalizationOn = false;
        for (int i = 0; i < 20000; i++)
        {
            double x = Program.NextDouble(51, 100-2, rnd);
            double y = Program.NextDouble(0, 100-2, rnd);
            var tmpSirka = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - x), rnd);
            var tmpViska = Program.NextDouble(0, Math.Min(100 * MAX_SIZE_OF_OBJCET_PERCENTAGE, 100 - 1 - y), rnd);
            double x2 = x + tmpSirka;
            double y2 = y + tmpViska;
            quadTree.Insert(x, y, x2, y2, i);
        }
        
        quadTree.OptimalizationOn = true;
        Assert.That(quadTree.Count, Is.EqualTo(30000));
        Assert.That(quadTree.Count, Is.EqualTo(quadTree.Recount()));
        Console.WriteLine(quadTree.Health);
        quadTree.Optimalise(true);
        Console.WriteLine(quadTree.Health);
        Assert.That(quadTree.TestGetRoot().PointUpRight.X, Is.EqualTo(139.0).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointUpRight.Y, Is.EqualTo(quadTree.OriginalPointUpRight.Y).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.X, Is.EqualTo(-30.0).Within(0.0001));
        Assert.That(quadTree.TestGetRoot().PointDownLeft.Y, Is.EqualTo(quadTree.OriginalPointDownLeft.Y).Within(0.0001));
    }
    
}