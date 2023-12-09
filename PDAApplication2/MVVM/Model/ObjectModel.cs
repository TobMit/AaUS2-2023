using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using DynamicHashFileStructure.StructureClasses;
using Microsoft.VisualBasic.CompilerServices;
using PDAApplication2.Core;
using PDAApplication2.Interface;
using ObjectType = PDAApplication2.Core.ObjectType;

namespace PDAApplication2.MVVM.Model
{
    public class ObjectModel
    {
        //private int _idObjektu;
        //private string _popis;

        public GPS GpsBod1 { get; set; }
        public GPS GpsBod2 { get; set; }
        public List<int> ZoznamObjektov { get; set; }

        public Core.ObjectType ObjectType { get; protected set; }

        public int IdObjektu { get; set; }

        public string Popis { get; set; }

        public string Title
        {
            get { return (ObjectType == Core.ObjectType.Nehnutelnost ? "Nehnuteľnosť: " : "Parcela: ") + IdObjektu; }
        }

        public ObjectModel(int idObjektu, string pPopis, GPS pGpsBod1, GPS pGpsBod2, Core.ObjectType type, List<int> pZoznamObjektov)
        {
            IdObjektu = idObjektu;
            Popis = pPopis;
            ZoznamObjektov = pZoznamObjektov;
            GpsBod1 = pGpsBod1;
            GpsBod2 = pGpsBod2;
            ObjectType = type;
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

        public override string ToString()
        {
            var ObjectTypeString = ObjectType == ObjectType.Nehnutelnost ? "Nehnutelnost" : "Parcela";

            StringBuilder sb = new();
            sb.AppendLine($"Id: {IdObjektu}");
            sb.AppendLine($"\tPopis: {Popis}");
            sb.AppendLine($"\t GPS1: {GpsBod1}");
            sb.AppendLine($"\t GPS2: {GpsBod2}");
            sb.AppendLine($"\tTyp Objektu: {ObjectTypeString}");
            
            StringBuilder zoznam = new();
            foreach (var obj in ZoznamObjektov)
            {
                zoznam.Append(obj + ", ");
            }
            sb.AppendLine($"\tZoznam adries: {zoznam.ToString()}");


            return sb.ToString();
        }
    }
}