using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using PDAAplication.Interface;

namespace PDAAplication.MVVM.Model
{
    public class GPS: ISavable
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

        public GPS()
        {
            X = 0;
            Y = 0;
        }

        public string ToSave()
        {
            return X + ";" + SirkaX + ";" + Y + ";" + DlzkaY; // tu nie je \n, lebo to je len jeden riadok (pokazilo by mi to ukladanie v ObjectModel)
        }
    }
}
