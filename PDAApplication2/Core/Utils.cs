using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using PDAApplication2.MVVM.Model;

namespace PDAApplication2.Core
{
    public class Utils
    {
        private static Regex number = new Regex(@"[^0-9]+");
        private static Regex decimalNumber = new Regex(@"^[0-9,.\-]+$");

        private static Regex xCooridate = new Regex(@"^[WwEe]$");
        private static Regex yCooridate = new Regex(@"^[SsNn]$");

        /// <summary>
        /// Kontrola či je zadané dobre des číslo
        /// </summary>
        public static void DoublePreviewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !decimalNumber.IsMatch(e.Text);
        }

        /// <summary>
        /// Kontrola či je zadané dobre des číslo
        /// </summary>
        public static void NumaberPreviewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = number.IsMatch(e.Text);
        }

        /// <summary>
        /// Kontroluje či je dobré písmenko pri súradniciach
        /// </summary>
        public static void xPrewiewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !xCooridate.IsMatch(e.Text);
        }

        /// <summary>
        /// Kontroluje či je dobré písmenko pri súradniciach
        /// </summary>
        public static void yPrewiewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !yCooridate.IsMatch(e.Text);
        }

        /// <summary>
        /// Skontroluje či sú zadané súradnice v správnom formáte a ak áno tak sa prepočítajú do správneho formátu pre strom
        /// </summary>
        /// <param name="dlgGps1">gps od používateľa</param>
        /// <param name="dlgGps2">gps od používateľa</param>
        /// <param name="recalculatedGps1">Prepočítané hodnoty gps pre strom</param>
        /// <param name="recalculatedGps2">Prepočítané hodnoty gps pre strom</param>
        /// <returns>True ak je všetko ok, false ak používateľ zadal zle       </returns>
        public static bool CheckAndRecalculateGps(GPS dlgGps1, GPS dlgGps2, GPS recalculatedGps1,
            GPS recalculatedGps2)
        {
            if (char.ToUpper(dlgGps1.SirkaX) == 'W' && char.ToUpper(dlgGps1.DlzkaY) == 'S' && char.ToUpper(dlgGps2.SirkaX) == 'E' && char.ToUpper(dlgGps2.DlzkaY) == 'N')
            {
                recalculatedGps1.X = dlgGps1.X;
                recalculatedGps1.SirkaX = dlgGps1.SirkaX;
                recalculatedGps1.Y = dlgGps1.Y;
                recalculatedGps1.DlzkaY = dlgGps1.DlzkaY;
                recalculatedGps2.X = dlgGps2.X;
                recalculatedGps2.SirkaX = dlgGps2.SirkaX;
                recalculatedGps2.Y = dlgGps2.Y;
                recalculatedGps2.DlzkaY = dlgGps2.DlzkaY;
                return true;
            }
            
            if (char.ToUpper(dlgGps1.SirkaX) == 'E' && char.ToUpper(dlgGps1.DlzkaY) == 'N' && char.ToUpper(dlgGps2.SirkaX) == 'W' && char.ToUpper(dlgGps2.DlzkaY) == 'S')
            {
                recalculatedGps2.X = dlgGps1.X;
                recalculatedGps2.SirkaX = dlgGps1.SirkaX;
                recalculatedGps2.Y = dlgGps1.Y;
                recalculatedGps2.DlzkaY = dlgGps1.DlzkaY;
                recalculatedGps1.X = dlgGps2.X;
                recalculatedGps1.SirkaX = dlgGps2.SirkaX;
                recalculatedGps1.Y = dlgGps2.Y;
                recalculatedGps1.DlzkaY = dlgGps2.DlzkaY;
                return true;
            }
            
            // prepočet do stromu
            if (char.ToUpper(dlgGps1.SirkaX) == 'W' && char.ToUpper(dlgGps1.DlzkaY) == 'N' && char.ToUpper(dlgGps2.SirkaX) == 'E' && char.ToUpper(dlgGps2.DlzkaY) == 'S')
            {
                recalculatedGps1.X = dlgGps1.X;
                recalculatedGps1.SirkaX = 'W';
                recalculatedGps1.Y = dlgGps2.Y;
                recalculatedGps1.DlzkaY = 'S';
                recalculatedGps2.X = dlgGps2.X;
                recalculatedGps2.SirkaX = 'E';
                recalculatedGps2.Y = dlgGps1.Y;
                recalculatedGps2.DlzkaY = 'N';
                return true;
            }
            
            if (char.ToUpper(dlgGps1.SirkaX) == 'E' && char.ToUpper(dlgGps1.DlzkaY) == 'S' && char.ToUpper(dlgGps2.SirkaX) == 'W' && char.ToUpper(dlgGps2.DlzkaY) == 'N')
            {
                recalculatedGps2.X = dlgGps1.X;
                recalculatedGps2.SirkaX = 'W';
                recalculatedGps2.Y = dlgGps2.Y;
                recalculatedGps2.DlzkaY = 'S';
                recalculatedGps1.X = dlgGps2.X;
                recalculatedGps1.SirkaX = 'E';
                recalculatedGps1.Y = dlgGps1.Y;
                recalculatedGps1.DlzkaY = 'N';
                return true;
            }
            
            return false;
        }
    }
}
