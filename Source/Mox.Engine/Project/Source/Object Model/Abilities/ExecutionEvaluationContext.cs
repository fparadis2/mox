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
using System.Diagnostics;

namespace Mox.Abilities
{
    public enum AbilityEvaluationContextType
    {
        /// <summary>
        /// Normal (priority given)
        /// </summary>
        Normal,
        /// <summary>
        /// During mana payment.
        /// </summary>
        ManaPayment,
        /// <summary>
        /// During triggered abilities evaluation.
        /// </summary>
        Triggered,
        /// <summary>
        /// During attacker declaration
        /// </summary>
        Attack,
        /// <summary>
        /// During blocker declaration
        /// </summary>
        Block
    }

    public class AbilityEvaluationContext
    {
        #region Variables

        private ManaPotentialCache m_manaPotentialCache;

        #endregion

        #region Constructor

        public AbilityEvaluationContext(Player player, AbilityEvaluationContextType type)
        {
            Debug.Assert(player != null);

            Player = player;
            Type = type;
        }

        #endregion

        #region Properties

        public Player Player
        {
            get;
        }

        public AbilityEvaluationContextType Type
        {
            get;
        }

        public bool UserMode
        {
            get;
            set;
        }

        internal object AbilityContext
        {
            get;
            set;
        }

        public ManaPotentialCache ManaPotential
        {
            get
            {
                if (m_manaPotentialCache == null)
                {
                    m_manaPotentialCache = new ManaPotentialCache(Player);
                }

                return m_manaPotentialCache;
            }
        }

        #endregion

        #region Methods

        public bool CanPlay(Ability ability)
        {
            switch (ability.AbilityType)
            {
                case AbilityType.Normal:

                    if (Type == AbilityEvaluationContextType.Normal)
                    {
                        return true;
                    }

                    return ability.IsManaAbility && Type == AbilityEvaluationContextType.ManaPayment;

                case AbilityType.Triggered:
                    return Type == AbilityEvaluationContextType.Triggered;

                case AbilityType.Attack:
                    return Type == AbilityEvaluationContextType.Attack;

                case AbilityType.Block:
                    return Type == AbilityEvaluationContextType.Block;

                case AbilityType.Static:
                    return false;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}