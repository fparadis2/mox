// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;

namespace Mox
{
    /// <summary>
    /// Allows easier manipulation of flags.
    /// </summary>
    public struct Flags<T> : IEquatable<Flags<T>>, IEquatable<T>
    {
        #region Variables

        private T m_value;

        #endregion

        #region Constructor

        static Flags()
        {
            Type type = typeof(T);
            Throw.InvalidProgramIf(!type.IsEnum, "Flags can only be used with enum types");
            Throw.InvalidProgramIf(type.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0, "Flags can only be used with enum types with the Flags attribute");
        }

        private Flags(T value)
        {
            m_value = value;
        }

        #endregion

        #region Properties

        public bool this[T flags]
        {
            get 
            {
                long flagsInLong = GetLongValue(flags);
                return (ValueInLong & flagsInLong) == flagsInLong;
            }
            set 
            {
                long flagsInLong = GetLongValue(flags);

                if (value)
                {
                    ValueInLong |= flagsInLong;
                }
                else
                {
                    ValueInLong &= ~flagsInLong;
                }
            }
        }

        private long ValueInLong
        {
            get { return GetLongValue(m_value); }
            set { m_value = (T)Enum.ToObject(typeof(T), value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the given <paramref name="flags"/> are completely contained.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool Contains(T flags)
        {
            return this[flags];
        }

        private static long GetLongValue(T flags)
        {
            return Convert.ToInt64(flags);
        }

        #endregion

        #region Overrides

        public static implicit operator T(Flags<T> flags)
        {
            return flags.m_value;
        }

        public static implicit operator Flags<T>(T flags)
        {
            return new Flags<T>(flags);
        }

        public static bool operator ==(Flags<T> a, Flags<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Flags<T> a, Flags<T> b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Flags<T>)
            {
                return Equals((Flags<T>)obj);
            }

            if (obj is T)
            {
                return Equals((T)obj);
            }

            return false;
        }

        public bool Equals(Flags<T> obj)
        {
            return Equals(obj.m_value);
        }

        public bool Equals(T obj)
        {
            return Equals(m_value, obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return m_value.ToString();
        }

        #endregion
    }
}
