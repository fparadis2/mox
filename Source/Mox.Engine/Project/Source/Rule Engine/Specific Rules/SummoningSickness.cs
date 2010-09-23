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

namespace Mox.Rules
{
    /// <summary>
    /// Implements the "summoning sickness" rule
    /// </summary>
    public static class SummoningSickness
    {
        #region Variables

        private static readonly Property<bool> SummoningSicknessProperty = Property<bool>.RegisterAttachedProperty("SummoningSicknessProperty", typeof(SummoningSickness), PropertyFlags.Private);
        private static readonly Scope m_bypassScope = new Scope();

        #endregion

        #region Methods

        public static bool HasSummoningSickness(this Card card)
        {
            if (m_bypassScope.InScope || !card.Is(Type.Creature) || card.HasAbility<HasteAbility>())
            {
                return false;
            }

            return card.GetValue(SummoningSicknessProperty);
        }

        internal static void SetSickness(Card card)
        {
            card.SetValue(SummoningSicknessProperty, true);
        }

        internal static void RemoveSickness(Card card)
        {
            card.ResetValue(SummoningSicknessProperty);
        }

        internal static IDisposable Bypass()
        {
            return m_bypassScope.Begin();
        }

        #endregion
    }
}
