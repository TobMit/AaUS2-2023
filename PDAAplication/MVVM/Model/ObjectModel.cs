using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.CompilerServices;
using PDAAplication.Core;
using PDAAplication.Interface;

namespace PDAAplication.MVVM.Model
{
    public class ObjectModel: ObservableObjects, ISavable, IComparable<int>
    {
        private int _idObjektu;
        private string _popis;

        public int IdObjektu
        {
            get => _idObjektu;
            set
            {
                if (value == _idObjektu) return;
                _idObjektu = value;
                OnPropertyChanged();
            }
        }

        public string Popis
        {
            get => _popis;
            set
            {
                if (value == _popis) return;
                _popis = value;
                OnPropertyChanged();
            }
        }

        public GPS GpsBod1 { get; set; }
        public GPS GpsBod2 { get; set; }
        public ObservableCollection<ObjectModel> ZoznamObjektov { get; set; }

        public Core.ObjectType ObjectType { get; set; }

        public string Title
        {
            get
            {
                return (ObjectType == Core.ObjectType.Nehnutelnost ? "Nehnuteľnosť: " : "Parcela: ") + IdObjektu;
            }
        }

        public ObjectModel(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2, Core.ObjectType type)
        {
            IdObjektu = idObjektu;
            Popis = pPopis;
            ZoznamObjektov = new();
            GpsBod1 = pGpsBod1;
            GpsBod2 = pGpsBod2;
            ObjectType = type;
        }

        public string ToSave()
        {
            return IdObjektu + ";" + Popis + ";" + GpsBod1.ToSave() + ";" + GpsBod2.ToSave() + ";" + ObjectType + "\n";
        }

        public int CompareTo(int other)
        {
            return IdObjektu.CompareTo(other);
        }
    }
}
