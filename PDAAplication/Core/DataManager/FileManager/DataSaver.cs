using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAAplication.Core.DataManager.FileManager
{
    class DataSaver
    {
        public static void SaveData(string path, string data)
        {
            System.IO.File.WriteAllText(path, data);
        }
    }
}
