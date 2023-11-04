using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PDAAplication.Core
{
    public class Utils
    {
        private static Regex number = new Regex(@"[^0-9]+");
        private static Regex decimalNumber = new Regex(@"-?\d+(?:\.\d+)?");

        private static Regex xCooridate = new Regex(@"^[WwEe]$");
        private static Regex yCooridate = new Regex(@"^[SsNn]$");

        public static void DoublePreviewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = decimalNumber.IsMatch(e.Text);
        }

        public static void NumaberPreviewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = number.IsMatch(e.Text);
        }

        public static void xPrewiewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = xCooridate.IsMatch(e.Text);
        }

        public static void yPrewiewCheck(object sender, TextCompositionEventArgs e)
        {
            e.Handled = yCooridate.IsMatch(e.Text);
        }
    }
}
