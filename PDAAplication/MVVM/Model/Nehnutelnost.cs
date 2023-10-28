using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAAplication.MVVM.Model
{
    class Nehnutelnost
    {
        public int SupisneCislo { get; set; }
        public string Popis { get; set; }
        public List<Parcela> ZoznamParciel { get; set; }
        public GPS JuhoZapadnyBod { get; set; }
        public GPS SeveroVychodnyBod { get; set; }
    }
}
