using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using PDAAplication.Interface;
using Quadtree.StructureClasses.HelperClass;

namespace PDAAplication.MVVM.Model
{
    public class GPS: ISavable, IEquatable<GPS>
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
    }
}
