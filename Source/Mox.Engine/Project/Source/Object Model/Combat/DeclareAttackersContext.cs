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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mox.Rules;

namespace Mox
{
    [Serializable]
    public class DeclareAttackersContext
    {
        #region Variables

        private readonly List<int> m_legalAttackers = new List<int>();

        #endregion

        #region Constructor

        public DeclareAttackersContext(IEnumerable<Card> legalAttackers)
        {
            m_legalAttackers.AddRange(legalAttackers.Select(c => c.Identifier));
        }

        #endregion

        #region Properties

        public IList<int> LegalAttackers
        {
            get { return m_legalAttackers; }
        }

        public bool IsEmpty
        {
            get { return m_legalAttackers.Count == 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the given <paramref name="result"/> is valid for this context.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool IsValid(DeclareAttackersResult result)
        {
            return result.AttackerIdentifiers.All(m_legalAttackers.Contains);
        }

        /// <summary>
        /// Constructs the attack context for the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static DeclareAttackersContext ForPlayer(Player player)
        {
            Zone battlefield = player.Manager.Zones.Battlefield;

            var legalAttackers = from Card card in player.Manager.Cards
                                 where card.Is(Type.Creature) && 
                                       card.Controller == player && 
                                       card.Zone == battlefield && 
                                       !card.Tapped &&
                                       !card.HasSummoningSickness() &&
                                       CanAttack(card, player)
                                 select card;

            return new DeclareAttackersContext(legalAttackers);
        }

        private static bool CanAttack(Card card, Player player)
        {
            ExecutionEvaluationContext evaluationContext = new ExecutionEvaluationContext
            {
                Type = EvaluationContextType.Attack
            };

            return card.Abilities.Where(a => a.AbilityType == AbilityType.Attack).All(a => a.CanPlay(player, evaluationContext));
        }

        #endregion
    }
}
