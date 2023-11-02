using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Quadtree.StructureClasses.HelperClass;

public class PointD : IEquatable<PointD>
{
    public double X { get; set; }
    public double Y { get; set; }
    
    

    public PointD(double x, double y)
    {
        X = x;
        Y = y;
    }


    /// <summary>
    /// Porovnáva či sú 2 pointy rovnaké.
    /// </summary>
    /// <param name="obj">Object na test rovnosti</param>
    /// <returns>True ak sú rovnaké, false ak niesú rovnaké</returns>
    public override bool Equals([NotNullWhen(true)] object? obj) => obj is PointD && Equals((PointD)obj);
    public bool Equals(PointD? other)
    {
        return Math.Abs(other.X - this.X) < Double.Epsilon && Math.Abs(other.Y - this.Y) < Double.Epsilon;
    }
    
    /// <summary>
    /// Funcia vie porovnať či majú oba PointD rovnaké hodnoty, je to párová funkcia !=
    /// </summary>
    /// <param name="left">Lavý PointD na porovnávanie</param>
    /// <param name="right">Pravý PointD na porovavanie</param>
    /// <returns></returns>
    public static bool operator ==(PointD left, PointD right) => Math.Abs(left.X - right.X) < Double.Epsilon && Math.Abs(left.Y - right.Y) < Double.Epsilon;
    
    /// <summary>
    /// Zistí či sú dva PointD nerovnaké, je to párova funkcia ==
    /// </summary>
    /// <param name="left">Lavý PointD na porovnávanie</param>
    /// <param name="right">Pravý PointD na porovavanie</param>
    /// <returns></returns>
    public static bool operator !=(PointD left, PointD right) => !(left == right);
    
    /// <summary>
    /// Vynázosby PointD o daný počet
    /// </summary>
    /// <param name="left">PointD ktorý násobime</param>
    /// <param name="right">čislo ktorým násobime</param>
    /// <returns></returns>
    public static PointD operator *(PointD left, double right) => new(left.X * right, left.Y * right);
}