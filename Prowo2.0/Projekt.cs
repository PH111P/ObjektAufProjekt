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

        public BitString AllowedKlassen { get; private set; }

        private List<Objekt> Teilnehmer = new List<Objekt>();
        public readonly bool editable;
        public readonly bool erhaltenswert;

        public Projekt(const_type<string> Projektname, const_type<int> MinAnz, const_type<int> MaxAnz,
            const_type<BitString> KlStufen, bool editable = true, string Desc = null,bool E_Wert = false )
        {
            this.Projektname = Projektname;
            this.MinAnz = MinAnz;
            this.MaxAnz = MaxAnz;
            this.AllowedKlassen = KlStufen;
            this.editable = editable;
            this.Description = Desc;
            this.erhaltenswert = E_Wert;
        }
        public Projekt(const_type<Projekt> P)
            : this(P.Data.Projektname, P.Data.MinAnz, P.Data.MaxAnz, P.Data.AllowedKlassen, P.Data.editable, P.Data.Description,P.Data.erhaltenswert)
        {
            this.Teilnehmer = new List<Objekt>();
            foreach (var S in P.Data.Teilnehmer)
                this.Add(S);
        }

        public override string ToString()
        {
            return this.Projektname;// +" " + TeilnehmerCount;
        }

        public IEnumerable<Objekt> GetList()
        {
            foreach (var t in this.Teilnehmer)
                if (!t.isLeiter)
                    yield return t;
        }
        public IEnumerable<Objekt> GetLeiterList()
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

        public void Add(const_type<Objekt> Item)
        {
            this.Teilnehmer.Add(Item);
            if (Item.Data.isLeiter)
                ++this.AnzLeiter;
        }
        public void Remove(const_type<Objekt> Item)
        {
            this.Teilnehmer.Remove(Item);
            if (Item.Data.isLeiter)
                --this.AnzLeiter;
        }

        public Objekt this[const_type<int> index]
        {
            get { return this.Teilnehmer.ElementAt(index); }
        }

        public List<Objekt> kill()
        {
            if (this.erhaltenswert)
                throw new InvalidOperationException();
            this.MinAnz = 0;
            this.MaxAnz = 0;
            this.AnzLeiter = 0;
            var ret = new List<Objekt>(this.Teilnehmer);
            for (int i = 0; i < ret.Count; ++i)
                if (ret[i].isLeiter)
                    ret[i].isLeiter = false;

            this.Teilnehmer.Clear();
            return ret;
        }
    }
}