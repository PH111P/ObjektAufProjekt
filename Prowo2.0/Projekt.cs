using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DataStructures;

namespace Prowo
{
    public class Projekt : IComparable<Projekt>
    {
        public int CompareTo(Projekt P)
        {
            return this.Projektname.CompareTo(P.Projektname);
        }

        public string Projektname { get; private set; }
        public string Description { get; private set; }
        public int MinAnz { get; private set; }
        public int MaxAnz { get; private set; }

        private int _AnzLeiter = 0;
        public int AnzLeiter
        {
            get { return _AnzLeiter; }
            private set { _AnzLeiter = value; }
        }

        public BitString KlassenStufen { get; private set; }

        private List<Schüler> Teilnehmer = new List<Schüler>();
        public readonly bool editable;
        public readonly bool erhaltenswert;

        public Projekt(const_type<string> Projektname, const_type<int> MinAnz, const_type<int> MaxAnz,
            const_type<BitString> KlStufen, bool editable = true, string Desc = null,bool E_Wert = false )
        {
            this.Projektname = Projektname;
            this.MinAnz = MinAnz;
            this.MaxAnz = MaxAnz;
            this.KlassenStufen = KlStufen;
            this.editable = editable;
            this.Description = Desc;
            this.erhaltenswert = E_Wert;
        }
        public Projekt(const_type<Projekt> P)
            : this(P.data.Projektname, P.data.MinAnz, P.data.MaxAnz, P.data.KlassenStufen, P.data.editable, P.data.Description,P.data.erhaltenswert)
        {
            this.Teilnehmer = new List<Schüler>();
            foreach (var S in P.data.Teilnehmer)
                this.Add(S);
        }

        public override string ToString()
        {
            return this.Projektname;// +" " + TeilnehmerCount;
        }

        public IEnumerable<Schüler> GetList()
        {
            foreach (var t in this.Teilnehmer)
                if (!t.isLeiter)
                    yield return t;
        }
        public IEnumerable<Schüler> GetLeiterList()
        {
            foreach (var t in this.Teilnehmer)
                if (t.isLeiter)
                    yield return t;
        }
        public int TeilnehmerCount { get { return this.Teilnehmer.Count - this.AnzLeiter; } }
        public int Length { get { return this.Teilnehmer.Count; } }
        public void reset()
        {
            this.Teilnehmer.Clear();
            this.AnzLeiter = 0;
        }

        public void Add(const_type<Schüler> Item)
        {
            this.Teilnehmer.Add(Item);
            if (Item.data.isLeiter)
                ++this.AnzLeiter;
        }
        public void Remove(const_type<Schüler> Item)
        {
            this.Teilnehmer.Remove(Item);
            if (Item.data.isLeiter)
                --this.AnzLeiter;
        }

        public Schüler this[const_type<int> index]
        {
            get { return this.Teilnehmer.ElementAt(index); }
        }

        public List<Schüler> kill()
        {
            if (this.erhaltenswert)
                throw new InvalidOperationException();
            this.MinAnz = 0;
            this.MaxAnz = 0;
            this.AnzLeiter = 0;
            var ret = new List<Schüler>(this.Teilnehmer);
            for (int i = 0; i < ret.Count; ++i)
                if (ret[i].isLeiter)
                    ret[i].isLeiter = false;

            this.Teilnehmer.Clear();
            return ret;
        }
    }
}