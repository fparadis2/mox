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
    /// Card types.
    /// </summary>
    [Flags]
    public enum Type
    {
        /// <summary>
        /// No type.
        /// </summary>
        None = 0,

        /// <summary>
        /// Artifact.
        /// </summary>
        Artifact = 1,

        /// <summary>
        /// Creature.
        /// </summary>
        Creature = 2,

        /// <summary>
        /// Enchantment.
        /// </summary>
        Enchantment = 4,

        /// <summary>
        /// Instant.
        /// </summary>
        Instant = 8,
        
        /// <summary>
        /// Land.
        /// </summary>
        Land = 16,

        /// <summary>
        /// Planeswalker.
        /// </summary>
        Planeswalker = 32,

        /// <summary>
        /// Sorcery.
        /// </summary>
        Sorcery = 64,

        /// <summary>
        /// Tribal.
        /// </summary>
        Tribal = 128,

        /// <summary>
        /// Scheme.
        /// </summary>
        Scheme = 256
    }

    public static class TypeExtensions
    {
        #region Constants

        private static readonly Type[] ms_typesInSignificanceOrder = new[] { Type.Creature, Type.Land, Type.Instant, Type.Sorcery, Type.Enchantment, Type.Planeswalker, Type.Tribal, Type.Artifact, Type.Scheme };

        private const Type ms_permanentTypes = Type.Artifact | Type.Creature | Type.Enchantment | Type.Land | Type.Planeswalker;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the most significant type out of the given <paramref name="type"/>.
        /// </summary>
        /// <remarks>
        /// Passing 'Artifact Creature' returns only 'Creature'.
        /// </remarks>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetDominantType(this Type type)
        {
            if (type == Type.None)
            {
                return type;
            }

            foreach (Type includedType in ms_typesInSignificanceOrder)
            {
                if (type.Is(includedType))
                {
                    return includedType;
                }
            }

            throw new InvalidProgramException("Missing type from " + type + " in ms_typesInSignificanceOrder?");
        }

        public static bool IsPermanent(this Type type)
        {
            return type.IsAny(ms_permanentTypes);
        }

        #endregion
    }
}
