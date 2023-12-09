using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using PDAApplication2.Interface;

namespace PDAApplication2.Core.DataManager.FileManager
{
    class DataSaver<T> where T : ISavable
    {
        private StringBuilder builder;
        public DataSaver()
        {
            builder = new();
        }

        /// <summary>
        /// Umožňuje pridať 1 riadok záznamu ktorý sa uloží do súboru
        /// </summary>
        /// <param name="line"></param>
        public void AddLine(string line)
        {
            builder.Append(line);
        }

        /// <summary>
        /// Pripraví data na uloženie do súboru
        /// </summary>
        /// <param name="dataToSave"> Dáta ktoré chceme uložiť do súboru</param>
        public void PrepareForSave(List<T> dataToSave)
        {
            foreach (T data in dataToSave)
            {
                builder.Append(data.ToSave());
            }
        }

        /// <summary>
        /// Uloží dáta do súboru asynchronne 
        /// </summary>
        public async void SaveData(string file)
        {
            using (StreamWriter outputFile = new StreamWriter(file))
            {
                await outputFile.WriteAsync(builder.ToString());
            }
        }

    }
}
