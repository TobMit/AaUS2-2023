using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using DynamicHashFileStructure.StructureClasses;
using PDAApplication2.Interface;

namespace PDAApplication2.MVVM.Model
{
    public class GPS: ISavable, IEquatable<GPS>, IRecordData<int>
    {
        public char SirkaX { get; set; }
        public double X { get; set; }
        public char DlzkaY { get; set; }
        public double Y { get; set; }

        public string XVypis
        {
            get
            {
                return char.ToUpper(SirkaX) + ": " + X;
            }
        }

        public string YVypis
        {
            get
            {
                return char.ToUpper(DlzkaY) + ": " + Y;
            }
        }

        public GPS(double pX, double pY)
        {
            X = pX;
            Y = pY;
        }

        public GPS(double pX, char pSirkaX, double pY, char pDlzkaY)
        {
            X = pX;
            SirkaX = pSirkaX;
            Y = pY;
            DlzkaY = pDlzkaY;
        }

        public GPS(GPS gps)
        {
            X = gps.X;
            SirkaX = gps.SirkaX;
            Y = gps.Y;
            DlzkaY = gps.DlzkaY;
        }


        public GPS()
        {
            X = 0;
            Y = 0;
        }

        public string ToSave()
        {
            return X + ";" + SirkaX + ";" + Y + ";" + DlzkaY; // tu nie je \n, lebo to je len jeden riadok (pokazilo by mi to ukladanie v ObjectModel)
        }

        public bool Equals(GPS? other)
        {
            if (other == null)
                return false;
            return Math.Abs(X - other.X) < Double.Epsilon && Math.Abs(Y - other.Y) < Double.Epsilon && SirkaX == other.SirkaX && DlzkaY == other.DlzkaY;
        }
        
        public static bool operator ==(GPS? gps1, GPS? gps2)
        {
            if (gps1 is null)
            {
                if (gps2 is null)
                {
                    return true;
                }
                return false;
            }
            return gps1.Equals(gps2);
        }

        public static bool operator !=(GPS? gps1, GPS? gps2)
        {
            return !(gps1 == gps2);
        }
        
        public static int GetSize()
        {
            return sizeof(char) + sizeof(double) + sizeof(char) + sizeof(double);
        }

        public byte[] GetBytes()
        {
            List<Byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes(SirkaX));
            bytes.AddRange(BitConverter.GetBytes(X));
            bytes.AddRange(BitConverter.GetBytes(DlzkaY));
            bytes.AddRange(BitConverter.GetBytes(Y));
            return bytes.ToArray();
        }

        public static object FromBytes(byte[] bytes)
        {
            GPS gps = new();
            int offset = 0;
            gps.SirkaX = BitConverter.ToChar(bytes, offset);
            offset += sizeof(char);
            gps.X = BitConverter.ToDouble(bytes, offset);
            offset += sizeof(double);
            gps.DlzkaY = BitConverter.ToChar(bytes, offset);
            offset += sizeof(char);
            gps.Y = BitConverter.ToDouble(bytes, offset);
            return gps;
        }

        //nebudeme potrebovať je to iba z interface
        public int CompareTo(int other)
        {
            throw new NotImplementedException();
        }
        
        // ani toto nebudeme potrebovať
        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        // ani toto nebude treba
        public static byte[] GetBytesForHash(int key)
        {
            throw new NotImplementedException();
        }

        // ani toto nebude treba
        public int GetKey()
        {
            throw new NotImplementedException();
        }
    }
}
