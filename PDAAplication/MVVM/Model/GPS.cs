using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace PDAAplication.MVVM.Model
{
    public class GPS
    {
        public char Sirka { get; set; }
        public double X { get; set; }
        public char Dlzka { get; set; }
        public double Y { get; set; }

        public GPS(double pX, double pY)
        {
            X = pX;
            Y = pY;
        }

        public GPS()
        {
            X = 0;
            Y = 0;
        }

    }
}
