using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataStructures;

namespace Prowo
{
    public abstract class aKlasse
    {
        protected const_type<string> Name;
        public abstract string GetName();
        public abstract aKlasse Base();
    }

    public class Klasse : aKlasse
    {
        public override string GetName()
        {
            return Name;
        }
        public override aKlasse Base()
        {
            return this;
        }

        public static Dictionary<Klasse, int> ID = new Dictionary<Klasse, int>();

        public static List<Klasse> ID_rev = new List<Klasse>();
        private static int ID_cnt = 0;

        public Klasse(const_type<string> name)
        {
            this.Name = name;
            if (!ID.ContainsKey(this))
            {
                ID[this] = ID_cnt++;
                ID_rev.Add(this);
            }
        }

        public static bool operator ==(Klasse other, Klasse k)
        {
            return k.Equals(other);
        }
        public static bool operator !=(Klasse other, Klasse k)
        {
            return !k.Equals(other);
        }       

        public override bool Equals(object obj)
        {
            return obj == null ? false : this.Name.Data == ((Klasse)obj).Name.Data;
        }
        public override int GetHashCode()
        {
            return this.Name.Data.GetHashCode();
        }
        public override string ToString()
        {
            return GetName();
        }

        public static void Reset()
        {
            ID = new Dictionary<Klasse, int>();
            ID_rev = new List<Klasse>();
            ID_cnt = 0;
        }
    }

    public class KlasseDecorator : aKlasse
    {
        protected const_type<aKlasse> DecoratedObject;

        public override aKlasse Base()
        {
            return DecoratedObject.Data.Base();
        }
        public override string GetName()
        {
            return DecoratedObject.Data.GetName() + " - " + this.Name;
        }

        public KlasseDecorator(const_type<aKlasse> decoratedObject, const_type<string> name)
        {
            this.Name = name;
            this.DecoratedObject = decoratedObject;
        }
    }
}
