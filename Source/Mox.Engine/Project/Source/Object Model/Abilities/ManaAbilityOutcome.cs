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
using System.Collections.Generic;

namespace Mox
{
    public abstract class ManaAbilityOutcome
    {
        #region Variables

        private static readonly AnyOutcome m_any = new AnyOutcome();

        #endregion

        #region Methods

        public abstract bool CanProvide(ManaPayment cost);

        public abstract IEnumerable<ManaAmount> GetPossibleAmounts();

        public static ManaAbilityOutcome None
        {
            get { return null; }
        }

        public static ManaAbilityOutcome Any
        {
            get { return m_any; }
        }

        public static ManaAbilityOutcome OfColor(Color color)
        {
            return new ColoredOutcome(color);
        }

        #endregion

        #region Inner Types

        private class AnyOutcome : ManaAbilityOutcome
        {
            public override bool CanProvide(ManaPayment cost)
            {
                return true;
            }

            public override IEnumerable<ManaAmount> GetPossibleAmounts()
            {
                return null;
            }
        }

        private class ColoredOutcome : ManaAbilityOutcome
        {
            private readonly Color m_color;

            public ColoredOutcome(Color color)
            {
                m_color = color;
            }

            public override bool CanProvide(ManaPayment payment)
            {
                if (m_color == Color.None)
                {
                    return payment.Payments.Contains(Color.None);
                }

                foreach (Color paymentColor in payment.Payments)
                {
                    if (paymentColor == Color.None || (paymentColor & m_color) != Color.None)
                    {
                        return true;
                    }
                }

                return false;
            }

            public override IEnumerable<ManaAmount> GetPossibleAmounts()
            {
                ManaAmount amount = new ManaAmount();
                amount.Add(m_color, 1);
                yield return amount;
            }
        }

        #endregion
    }
}