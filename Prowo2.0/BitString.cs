using System;
using System.Collections.Generic;
using System.Collections;

namespace DataStructures
{
    public struct BitString
    {
        private enum Bit
        {
            O,
            L
        };
        private List<Bit> bitString;
        /// <summary>
        /// Länge des BitStrings
        /// </summary>
        public int Length;

        /// <summary>
        /// Erzeugt einen BitString aus einem 
        /// gegebenen Array von boolschen Werten.
        /// </summary>
        /// <param name="Input">Das Array, aus dessen Werte der BitString generiert wird.</param>
        public BitString(params bool[] Input)
        {
            this.bitString = new List<Bit>();
            for (int i = 0; i < Input.Length; i++)
                this.bitString.Add(Input[i] ? Bit.L : Bit.O);
            Length = Input.Length;
        }
        /// <summary>
        /// Erzeugt einen neuen BitString aus einem gegebenen Integer
        /// und mit genau der Länge [Length]
        /// </summary>
        /// <param name="Input">Die Variable, deren Wert in den BitString umgewandelt wird</param>
        /// <param name="Length">Die Länge, die der neue BitString besitzen soll. (Optional)
        /// </param>
        public BitString(const_type<int> Input, int Length = -1)
        {
            int a = Input == 0 ? 0 : (int)Math.Floor(Math.Log(Input, 2));
            Bit[] _bitString;
            _bitString = new Bit[a + 1 > Length ? a + 1 : Length];
            if (a == 0)
            {
                _bitString[0] = Bit.O;
                this.Length = Length < 0 ? 1 : Length;
                this.bitString = new List<Bit>(_bitString);
                return;
            }
            while (a >= 0)
            {
                _bitString[_bitString.Length - 1 - a] = Bit.L;
                Input -= (int)Math.Pow(2, a);
                a = (int)Math.Floor(Math.Log(Input, 2));
            }
            if (Length >= 0)
            {
                this.Length = Length;
                this.bitString = new List<Bit>(_bitString);
                while (_bitString.Length > Length)
                    this.RemoveBit(_bitString.Length - 1);
                while (_bitString.Length < Length)
                    this.AddBit(false);
            }
            else
            {
                this.bitString = new List<Bit>(_bitString);
                this.Length = _bitString.Length;
            }
        }

        public bool this[const_type<int> index]
        {
            get { return bitString[index] == Bit.L ? true : false; }
            set { bitString[index] = value ? Bit.L : Bit.O; }
        }

        /// <summary>
        /// Fügt hinter die letzte Stelle des BitStrings ein
        /// Bit des Wertes [Value] an.
        /// </summary>
        /// <param name="value">Die Vairiable, dessen Wert angefügt wird.</param>
        public void AddBit(const_type<bool> value)
        {
            bitString.Add(value ? Bit.L : Bit.O);
            Length++;
        }
        /// <summary>
        /// Fügt in den BitString an die Position [Pos] den
        /// Wert [value] ein.
        /// </summary>
        /// <param name="value">Die Variable, dessen Wert eingefügt wird.</param>
        /// <param name="Pos">Die Position an die der Wert eingefügt wird.
        /// </param>
        public void InsertBit(const_type<bool> value, const_type<int> Pos)
        {
            bitString.Insert(Pos, value ? Bit.L : Bit.O);
            Length++;
        }
        /// <summary>
        /// Entfernt das Bit an Position [Pos].
        /// </summary>
        /// <param name="Pos">Die Position, an deren Stelle ein Wert entfernt werden soll.</param>
        public void RemoveBit(const_type<int> Pos)
        {
            bitString.RemoveAt(Pos);
            Length--;
        }

        /// <summary>
        /// Rotiert den Bitstring in Richtung Rechts um [number] Stellen.
        /// </summary>
        /// <param name="number">Die Anzahl, um wie viele Stellen der BitString in Richtung Rechts rotiert werden soll.</param>
        /// <exception cref="ArgumentException"></exception>
        public void RotateRight(const_type<int> number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException();
            number %= bitString.Count;
            Bit[] HilfsArr = new Bit[number];
            for (int i = 0; i < number; i++)
                HilfsArr[i] = bitString[bitString.Count - i - 1];
            for (int i = bitString.Count - 1; i >= number; i--)
                bitString[i] = bitString[i - number];
            for (int i = 0; i < number; i++)
                bitString[i] = HilfsArr[HilfsArr.Length - i - 1];
        }
        /// <summary>
        /// Rotiert den Bitstring in Richtung Links um [number] Stellen.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <param name="number">Die Anzahl, um wie viele Stellen der BitString in Richtung Links rotiert werden soll.</param>
        public void RotateLeft(const_type<int> number)
        {
            if (number < 0)
                throw new ArgumentOutOfRangeException();
            number %= bitString.Count;
            Bit[] HilfsArr = new Bit[number];
            for (int i = 0; i < number; i++)
                HilfsArr[i] = bitString[i];
            for (int i = 0; i < bitString.Count - number; i++)
                bitString[i] = bitString[i + number];
            for (int i = 0; i < number; i++)
                bitString[bitString.Count - i - 1] = HilfsArr[HilfsArr.Length - i - 1];
        }

        public static implicit operator int(BitString bStr)
        {
            int _return = 0;
            for (int i = 0; i < bStr.bitString.Count; i++)
                _return += (int)(bStr.bitString[i] == Bit.L ? Math.Pow(2, bStr.bitString.Count - 1 - i) : 0);
            return _return;
        }
        static public implicit operator BitString(int value)
        {
            if (value < 0)
                throw new InvalidCastException();
            return new BitString(value);
        }

        public static BitString operator <<(BitString BtS, int N)
        {
            for (int i = 0; i < N; i++)
                BtS.AddBit(false);
            return BtS;
        }
        public static BitString operator >>(BitString BtS, int N)
        {
            for (int i = 0; i < N; i++)
                BtS.RemoveBit(BtS.Length - 1);
            return BtS;
        }

        static public BitString operator !(BitString bStr)
        {
            BitString a = new BitString((int)bStr);
            for (int i = 0; i < bStr.bitString.Count; i++)
                a.bitString[i] = bStr.bitString[i] == Bit.L ? Bit.O : Bit.L;
            return a;
        }
    }
}