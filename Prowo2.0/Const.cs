using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures
{
    /// <summary>
    /// A symbolic ``const´´- type
    /// </summary>
    public class const_type<T>
    {
        public T data { get; private set; }

        public const_type(T data) { this.data = data; }

        public static implicit operator const_type<T>(T data) { return new const_type<T>(data); }
        public static implicit operator T(const_type<T> data) { return data.data; }

        public override bool Equals(object obj) { return data.Equals(obj); }
        public override int GetHashCode() { return data.GetHashCode(); }
        public override string ToString() { return data.ToString(); }

        public static bool operator ==(const_type<T> A, const_type<T> B) { return A.data.Equals(B.data); }
        public static bool operator !=(const_type<T> A, const_type<T> B) { return !(A==B); }
    }
}
