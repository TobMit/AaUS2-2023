using System;
using System.Collections.Generic;
using System.Text;
using DynamicHashFileStructure.StructureClasses;
using PDAApplication2.Core;

namespace PDAApplication2.MVVM.Model;

public class ObjectModelNehnutelnost : ObjectModel, IRecordData<int>
{
    private const int MAX_SIZE_POPIS = 15;

    public ObjectModelNehnutelnost(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2, int pSupisneCislo, List<int> pZoznamObjektov) : base(idObjektu, pPopis,
        pGpsBod1, pGpsBod2, ObjectType.Nehnutelnost, pSupisneCislo, pZoznamObjektov)
    {
    }
    
    public ObjectModelNehnutelnost(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2, int pSupisneCislo) : base(idObjektu, pPopis,
        pGpsBod1, pGpsBod2, ObjectType.Nehnutelnost, pSupisneCislo)
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
            good &= SupisneCislo == other.SupisneCislo;
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
        //        ID       +     GPS1    +       GPS2    +  pocetObjekotv  +                     Pole add objektov               + pocetPlatnychZnakov    + Popis      +    supisneCislo
        return sizeof(int) + GPS.GetSize() + GPS.GetSize() + sizeof(int)  + (Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST * sizeof(int)) + sizeof(int) + MAX_SIZE_POPIS  + sizeof(int);
    }

    public byte[] GetBytes()
    {
        List<byte> bytes = new();
        bytes.AddRange(BitConverter.GetBytes(IdObjektu));
        bytes.AddRange(BitConverter.GetBytes(SupisneCislo));
        bytes.AddRange(GpsBod1.GetBytes());
        bytes.AddRange(GpsBod2.GetBytes());
        bytes.AddRange(BitConverter.GetBytes(ZoznamObjektov.Count));
        for (int i = 0; i < Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST; i++)
        {
            if (i>=ZoznamObjektov.Count)
            {
                bytes.AddRange(BitConverter.GetBytes(-1));
            }
            else
            {
                bytes.AddRange(BitConverter.GetBytes(ZoznamObjektov[i]));
            }
        }

        bytes.AddRange(BitConverter.GetBytes(Popis.Length > MAX_SIZE_POPIS ? MAX_SIZE_POPIS : Popis.Length));
        bytes.AddRange(Encoding.ASCII.GetBytes(Popis.PadRight(MAX_SIZE_POPIS)));
        return bytes.ToArray();
    }

    public static object FromBytes(byte[] bytes)
    {
        var offset = 0;
        int idObjektu = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        int supisneCislo = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        GPS gps1 = (GPS)GPS.FromBytes(bytes[offset..(offset + GPS.GetSize())]);
        offset += GPS.GetSize();
        GPS gps2 = (GPS)GPS.FromBytes(bytes[offset..(offset + GPS.GetSize())]);
        offset += GPS.GetSize();
        
        int pocetObjektov = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        List<int> zoznamObjektov = new();
        for (int i = 0; i < Constants.MAX_COUNT_PARCELS_IN_NEHNUTELNOST; i++)
        {
            if (i < pocetObjektov)
            {
                zoznamObjektov.Add(BitConverter.ToInt32(bytes, offset));
            }
            offset += sizeof(int);
        }

        int pocetPlatnychZnakov = BitConverter.ToInt32(bytes, offset);
        offset += sizeof(int);
        string popis = Encoding.ASCII.GetString(bytes, offset, MAX_SIZE_POPIS);
        popis = popis.Substring(0, pocetPlatnychZnakov);
        
        return new ObjectModelNehnutelnost(idObjektu, popis, gps1, gps2, supisneCislo,  zoznamObjektov);
    }

    public static byte[] GetBytesForHash(int key)
    {
        //return BitConverter.GetBytes(key % 503);
        return BitConverter.GetBytes(key % 19);
    }

    public int GetKey()
    {
        return IdObjektu;
    }
}