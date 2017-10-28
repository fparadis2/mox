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

namespace Mox
{
    public enum EvaluationContextType
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

    public class ExecutionEvaluationContext
    {
        #region Variables

        private ManaPotentialCache m_manaPotentialCache;

        #endregion

        #region Constructor

        public ExecutionEvaluationContext(Player player, EvaluationContextType type)
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

        public EvaluationContextType Type
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

                    if (Type == EvaluationContextType.Normal)
                    {
                        return true;
                    }

                    return ability.IsManaAbility && Type == EvaluationContextType.ManaPayment;

                case AbilityType.Triggered:
                    return Type == EvaluationContextType.Triggered;

                case AbilityType.Attack:
                    return Type == EvaluationContextType.Attack;

                case AbilityType.Block:
                    return Type == EvaluationContextType.Block;

                case AbilityType.Static:
                    return false;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }
}