using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DataStructures;

namespace Prowo
{
    public class TupleCmp<T1, T2> : IComparer<Tuple<T1, T2>>
        where T1 : IComparable<T1>
        where T2 : IComparable<T2>
    {
        public int Compare(Tuple<T1, T2> A, Tuple<T1, T2> B)
        {
            if (A.Item1.CompareTo(B.Item1) == 0)
                return A.Item2.CompareTo(B.Item2);
            else
                return A.Item1.CompareTo(B.Item1);
        }
    }

    public class Schüler : IComparable<Schüler>
    {
        public bool WishesSet { get; private set; }
        public readonly bool wasLeiter;
        public bool isLeiter;
        public readonly string Name, Vorname;
        public readonly List<int> Wünsche;
        public enum klassenstufe
        {
            NONE = 0,
            LEHRER = 1,
            _5 = 5,
            _6 = 6,
            _7 = 7,
            _8 = 8,
            _9 = 9,
            _10 = 10,
            _11 = 11
        }
        public readonly klassenstufe Klassenstufe;
        public enum klasse
        {
            NONE = 0,
            _1 = 1,
            _2 = 2,
            _3 = 3
        }
        public readonly klasse Klasse;

        public Schüler(const_type<string> Name, const_type<string> Vorname, const_type<int> KlStf, const_type<List<int>> W, int Kl = 0, bool Leiter = false)
        {
            this.Name = Name;
            this.Vorname = Vorname;

            if (W.data.Count > 0)
                this.Wünsche = W;
            this.WishesSet = W.data.Count > 0;
            this.wasLeiter = this.isLeiter = Leiter;

            if ((KlStf >= 5 && KlStf <= 11) || KlStf == 1)
                this.Klassenstufe = (klassenstufe)KlStf.data;
            else
                this.Klassenstufe = 0;
            if (Kl >= 0 && Kl < 4)
                this.Klasse = (klasse)Kl;
            else
                this.Klasse = klasse.NONE;
        }
        public Schüler(const_type<string> Name, const_type<string> Vorname, const_type<string> KlStf, const_type<List<int>> W, bool Leiter = false)
        {
            this.Name = Name;
            this.Vorname = Vorname;
            if (W.data.Count > 0)
                this.Wünsche = W;
            this.WishesSet = W.data.Count > 0;
            this.isLeiter = this.wasLeiter = Leiter;
            if (KlStf == "Lehrer")
            {
                this.Klassenstufe = klassenstufe.LEHRER;
                this.Klasse = klasse.NONE;
            }
            else
            {
                var kl = KlStf.data.Split('-');
                this.Klassenstufe = (klassenstufe)Convert.ToInt32(kl[0]);
                if (kl.Length > 1)
                    this.Klasse = (klasse)Convert.ToInt32(kl[1]);
                else
                    this.Klasse = klasse.NONE;
            }
        }
        public Schüler(const_type<Schüler> val)
        {
            this.Name = val.data.Name;
            this.Vorname = val.data.Vorname;
            this.Wünsche = val.data.Wünsche;
            this.Klassenstufe = val.data.Klassenstufe;
            this.Klasse = val.data.Klasse;
            this.WishesSet = val.data.WishesSet;
            this.wasLeiter = val.data.wasLeiter;
            this.isLeiter = val.data.isLeiter;
        }

        public IEnumerable<int> GetList()
        {
            foreach (var w in Wünsche)
                yield return w;
        }

        public int CompareTo(Schüler S)
        {
            int st1 = 0, st2 = 0;
            while (this.Name[st1] == ' ' || Char.IsLower(this.Name[st1]))
                ++st1;
            while (S.Name[st2] == ' ' || Char.IsLower(S.Name[st2]))
                ++st2;

            if (this.Name.Substring(st1) == S.Name.Substring(st2))
                return this.Vorname.CompareTo(S.Vorname);
            else return this.Name.Substring(st1).CompareTo(S.Name.Substring(st2));
        }

        public string getKlasse()
        {
            string kl = "";
            if (Klassenstufe == klassenstufe.LEHRER)
                kl = "Lehrer";
            else
            {
                kl += (int)Klassenstufe;
                if (Klassenstufe != klassenstufe._11 && Klassenstufe != klassenstufe.NONE)
                    if (Klasse != klasse.NONE)
                        kl += "-" + (int)Klasse;
            }
            return kl;
        }
        public static Tuple<klassenstufe, klasse> getKlasse(const_type<string> KlStf)
        {
            klassenstufe ks;
            klasse k;
            if (KlStf == "Lehrer")
            {
                ks = klassenstufe.LEHRER;
                k = klasse.NONE;
            }
            else
            {
                var kl = KlStf.data.Split('-');
                ks = (klassenstufe)Convert.ToInt32(kl[0]);
                if (kl.Length > 1)
                    k = (klasse)Convert.ToInt32(kl[1]);
                else
                    k = klasse.NONE;
            }
            return new Tuple<klassenstufe, klasse>(ks, k);
        }

        public int this[int index] { get { return Wünsche[index]; } }
    }
}