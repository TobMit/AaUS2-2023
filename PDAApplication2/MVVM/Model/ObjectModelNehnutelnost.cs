using System;
using System.Collections.Generic;
using DynamicHashFileStructure.StructureClasses;
using PDAApplication2.Core;

namespace PDAApplication2.MVVM.Model;

public class ObjectModelNehnutelnost : ObjectModel, IRecordData<int>
{
    private const int MAX_SIZE_POPIS = 15;

    public ObjectModelNehnutelnost(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2, List<int> pZoznamObjektov) : base(idObjektu, pPopis,
        pGpsBod1, pGpsBod2, ObjectType.Nehnutelnost, pZoznamObjektov)
    {
    }
    
    public ObjectModelNehnutelnost(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2) : base(idObjektu, pPopis,
        pGpsBod1, pGpsBod2, ObjectType.Nehnutelnost)
    {
    }


    public int CompareTo(object? obj)
    {
        if (obj == null) return -1;
        ObjectModel other = obj as ObjectModel;
        if (other != null)
        {
            bool good = true;
            good &= GpsBod1.Equals(other.GpsBod1);
            good &= GpsBod2.Equals(other.GpsBod2);
            good &= IdObjektu == other.IdObjektu;
            good &= Popis == other.Popis;
            good &= ObjectType == other.ObjectType;
            if (!good)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
        else
        {
            return -1;
        }
    }

    public int CompareTo(int other)
    {
        return IdObjektu.CompareTo(other);
    }

    public static int GetSize()
    {
        //        ID       +     GPS1    +       GPS2    +  pocetObjekotv  +                     Pole add objektov               + pocetPlatnychZnakov            + Popis;
        return sizeof(int) + GPS.GetSize() + GPS.GetSize() + sizeof(int)  + (Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST * sizeof(int)) + sizeof(int) + MAX_SIZE_POPIS * sizeof(char);
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = new();
        bytes.AddRange(BitConverter.GetBytes(IdObjektu));
        bytes.AddRange(GpsBod1.GetBytes());
        bytes.AddRange(GpsBod2.GetBytes());
        bytes.AddRange(BitConverter.GetBytes(ZoznamObjektov.Count));
        foreach (int objekt in ZoznamObjektov)
        {
            bytes.AddRange(BitConverter.GetBytes(objekt));
        }

        bytes.AddRange(BitConverter.GetBytes(Popis.Length));
        bytes.AddRange(System.Text.Encoding.UTF8.GetBytes(Popis.PadRight(MAX_SIZE_POPIS)));
        return bytes.ToArray();
    }

    public static object FromBytes(byte[] bytes)
    {
        var offset = 0;
        int idObjektu = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        GPS gps1 = (GPS)GPS.FromBytes(bytes[offset..(offset + GPS.GetSize())]);
        offset += GPS.GetSize();
        GPS gps2 = (GPS)GPS.FromBytes(bytes[offset..(offset + GPS.GetSize())]);
        offset += GPS.GetSize();
        int pocetObjektov = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        List<int> zoznamObjektov = new();
        for (int i = 0; i < pocetObjektov; i++)
        {
            zoznamObjektov.Add(BitConverter.ToInt32(bytes, offset));
            offset += sizeof(int);
        }

        int pocetPlatnychZnakov = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        string popis =
            System.Text.Encoding.UTF8.GetString(
                bytes[offset..(offset + pocetPlatnychZnakov)]); // nemusím načítať viac ako je počet platných znakov
        
        return new ObjectModelNehnutelnost(idObjektu, popis, gps1, gps2, zoznamObjektov);
    }

    public static byte[] GetBytesForHash(int key)
    {
        return BitConverter.GetBytes(key % 503);
    }

    public int GetKey()
    {
        return IdObjektu;
    }
}