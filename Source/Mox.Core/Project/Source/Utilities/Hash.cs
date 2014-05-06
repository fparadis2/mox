using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public class Hash
    {
        private const uint FnvPrime = 16777619;
        private const uint FnvOffset = 2166136261;

        private uint m_value = FnvOffset;

        public int Value
        {
            get { unchecked { return (int)m_value; } }
        }

        public void Add(int value)
        {
            AddByte((byte)(value >> 24));
            AddByte((byte)(value >> 16));
            AddByte((byte)(value >> 8));
            AddByte((byte)value);
        }

        public void Add(long value)
        {
            AddByte((byte)(value >> 56));
            AddByte((byte)(value >> 48));
            AddByte((byte)(value >> 40));
            AddByte((byte)(value >> 32));
            AddByte((byte)(value >> 24));
            AddByte((byte)(value >> 16));
            AddByte((byte)(value >> 8));
            AddByte((byte)value);
        }

        public void AddByte(byte value)
        {
            unchecked
            {
                m_value = m_value * FnvPrime;
                m_value = m_value ^ value;
            }
        }

        public void Add(bool value)
        {
            AddByte(value ? (byte)1 : (byte)0);
        }

        public void Add<T>(Resolvable<T> resolvable) 
            where T : class, IObject
        {
            Add(resolvable.Identifier);
        }

        public void Add(string value)
        {
            Add(value == null ? 0 : value.GetHashCode());
        }
    }
}
