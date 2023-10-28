using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDAAplication.MVVM.Model
{
    class Parcela
    {
        public int CisloParcely { get; set; }
        public string Popis { get; set; }
        public List<Parcela> ZoznamNehnutelnosti { get; set; }
        public GPS JuhoZapadnyBod { get; set; }
        public GPS SeveroVychodnyBod { get; set; }
    }
}
