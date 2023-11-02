using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using PDAAplication.Interface;

namespace PDAAplication.Core.DataManager.FileManager
{
    class DataSaver<T> where T : ISavable
    {
        private StringBuilder builder;
        public DataSaver()
        {
            builder = new();
        }

        /// <summary>
        /// Pripravý data na uloženie do súboru
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
        public async void SaveData()
        {
            // modal window for save
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = "Data.csv";
            saveFileDialog1.Filter = "Data (*.csv)|*.csv";
            saveFileDialog1.Title = "Save to a file";
            

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (StreamWriter outputFile = new StreamWriter(saveFileDialog1.FileName))
                {
                    await outputFile.WriteAsync(builder.ToString());
                }
            }
        }

    }
}
