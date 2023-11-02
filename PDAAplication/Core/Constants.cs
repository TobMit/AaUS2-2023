using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAAplication.Core
{
    public enum ObjectType
    {
        Nehnutelnost = 0,
        Parcela = 1
    }

    public static class Constants
    {
        public static double MAX_SIZE_OF_OBJCET_PERCENTAGE = 0.15;
        public static int MAX_SIZE_TO_SHOW = 500;
    }
}
