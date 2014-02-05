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
        public T Data { get; private set; }

        public const_type(T data) { this.Data = data; }

        public static implicit operator const_type<T>(T data) { return new const_type<T>(data); }
        public static implicit operator T(const_type<T> data) { return data.Data; }

        public override bool Equals(object obj) { return Data.Equals(obj); }
        public override int GetHashCode() { return Data.GetHashCode(); }
        public override string ToString() { return Data.ToString(); }

        public static bool operator ==(const_type<T> A, const_type<T> B) { return A.Data.Equals(B.Data); }
        public static bool operator !=(const_type<T> A, const_type<T> B) { return !(A==B); }
    }
}
