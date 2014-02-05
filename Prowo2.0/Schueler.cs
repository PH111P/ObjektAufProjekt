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
    public class TupleRndCmp<T1, T2> : IComparer<Tuple<T1, T2>>
        where T1 : IComparable<T1>
        where T2 : IComparable<T2>
    {
        public int Compare(Tuple<T1, T2> A, Tuple<T1, T2> B)
        {
            return new Random().Next(-1, 2);
        }
    }

    public class Objekt : IComparable<Objekt>
    {
        public bool WishesSet { get; private set; }
        public readonly bool wasLeiter;
        public bool isLeiter;
        public readonly const_type<string> Name, Vorname;
        public readonly List<int> Wünsche;
        public readonly const_type<aKlasse> Klasse;

        public Objekt(const_type<string> name, const_type<string> vorname, const_type<aKlasse> klasse, const_type<List<int>> wishes, bool leiter = false)
        {
            this.Name = name;
            this.Vorname = vorname;

            if (wishes.Data.Count > 0)
                this.Wünsche = wishes;
            this.WishesSet = wishes.Data.Count > 0;
            this.wasLeiter = this.isLeiter = leiter;

            //Unsafe programming looks like this:
            this.Klasse = klasse;
        }
        public Objekt(const_type<Objekt> val)
        {
            this.Name = val.Data.Name;
            this.Vorname = val.Data.Vorname;
            this.Wünsche = val.Data.Wünsche;
            this.Klasse = val.Data.Klasse;
            this.WishesSet = val.Data.WishesSet;
            this.wasLeiter = val.Data.wasLeiter;
            this.isLeiter = val.Data.isLeiter;
        }

        public IEnumerable<int> GetList()
        {
            foreach (var w in Wünsche)
                yield return w;
        }

        public int CompareTo(Objekt S)
        {
            int st1 = 0, st2 = 0;
            while (this.Name.Data[st1] == ' ' || Char.IsLower(this.Name.Data[st1]))
                ++st1;
            while (S.Name.Data[st2] == ' ' || Char.IsLower(S.Name.Data[st2]))
                ++st2;

            if (this.Name.Data.Substring(st1) == S.Name.Data.Substring(st2))
                return this.Vorname.Data.CompareTo(S.Vorname);
            else return this.Name.Data.Substring(st1).CompareTo(S.Name.Data.Substring(st2));
        }

        public int GetKlasse()
        {
            return Prowo.Klasse.ID[(Prowo.Klasse)(this.Klasse.Data.Base())];
        }

        public int this[int index] { get { return Wünsche[index]; } }
    }
}