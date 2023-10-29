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
        public List<Nehnutelnost> ZoznamNehnutelnosti { get; set; }
        public GPS JuhoZapadnyBod { get; set; }
        public GPS SeveroVychodnyBod { get; set; }

        public Parcela(int pCisloParcely, string pPopis, GPS pJuhoZapadnyBod, GPS pSeveroVychodnyBod)
        {
            CisloParcely = pCisloParcely;
            Popis = pPopis;
            ZoznamNehnutelnosti = new();
            JuhoZapadnyBod = pJuhoZapadnyBod;
            SeveroVychodnyBod = pSeveroVychodnyBod;
        }
    }
}
