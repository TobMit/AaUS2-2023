using System;
using PDAApplication2.Interface;

namespace PDAApplication2.MVVM.Model;

public class ObjectModelQuad : ISavable, IComparable<int>
{
    public int IdObjektu { get; set; }
    public GPS GpsBod1 { get; set; }
    public GPS GpsBod2 { get; set; }
    
    public ObjectModelQuad(int idObjektu, GPS pGpsBod1, GPS pGpsBod2)
    {
        IdObjektu = idObjektu;
        GpsBod1 = pGpsBod1;
        GpsBod2 = pGpsBod2;
    }
    public string ToSave()
    {
        return IdObjektu + ";" + GpsBod1.ToSave() + ";" + GpsBod2.ToSave() + "\n";
    }

    public int CompareTo(int other)
    {
        return IdObjektu.CompareTo(other);
    }
}