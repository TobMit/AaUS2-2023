using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;

namespace PDAAplication.MVVM.Model
{
    public class ObjectModel
    {
        public int IdObjektu { get; set; }
        public string Popis { get; set; }
        public GPS JuhoZapadnyBod { get; set; }
        public GPS SeveroVychodnyBod { get; set; }
        public ObservableCollection<ObjectModel> ZoznamObjektov { get; set; }

        public Core.ObjectType ObjectType { get; set; }

        public ObjectModel(int idObjektu, string pPopis, GPS pJuhoZapadnyBod, GPS pSeveroVychodnyBod, Core.ObjectType type)
        {
            IdObjektu = idObjektu;
            Popis = pPopis;
            ZoznamObjektov = new();
            JuhoZapadnyBod = pJuhoZapadnyBod;
            SeveroVychodnyBod = pSeveroVychodnyBod;
            ObjectType = type;
        }
    }
}
