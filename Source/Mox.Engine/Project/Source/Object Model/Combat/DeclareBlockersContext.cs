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

namespace Mox
{
    [Serializable]
    public class DeclareBlockersContext
    {
        #region Variables

        private readonly List<int> m_attackers = new List<int>();
        private readonly List<int> m_legalBlockers = new List<int>();

        #endregion

        #region Constructor

        public DeclareBlockersContext(IEnumerable<Card> attackers, IEnumerable<Card> legalBlockers)
            : this(attackers.Select(c => c.Identifier), legalBlockers)
        {
        }

        private DeclareBlockersContext(IEnumerable<int> attackerIdentifiers, IEnumerable<Card> legalBlockers)
        {
            m_attackers.AddRange(attackerIdentifiers);
            m_legalBlockers.AddRange(legalBlockers.Select(c => c.Identifier));
        }

        #endregion

        #region Properties

        public IList<int> Attackers
        {
            get { return m_attackers; }
        }

        public IList<int> LegalBlockers
        {
            get { return m_legalBlockers; }
        }

        public bool IsEmpty
        {
            get { return m_legalBlockers.Count == 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the given <paramref name="result"/> is valid for this context.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool IsValid(DeclareBlockersResult result)
        {
            return result.Blockers.All(pair => m_legalBlockers.Contains(pair.BlockingCreatureId) && m_attackers.Contains(pair.BlockedCreatureId));
        }

        /// <summary>
        /// Constructs the attack context for the given <paramref name="player"/>.
        /// </summary>
        /// <returns></returns>
        public static DeclareBlockersContext ForPlayer(Player player)
        {
            Zone battlefield = player.Manager.Zones.Battlefield;

            var legalBlockers = from Card card in player.Manager.Cards
                                where card.Is(Type.Creature) && 
                                      card.Controller == player && 
                                      card.Zone == battlefield && 
                                      !card.Tapped &&
                                      CanBlock(card, player)
                                select card;

            return new DeclareBlockersContext(player.Manager.CombatData.Attackers.AttackerIdentifiers, legalBlockers);
        }

        private static bool CanBlock(Card card, Player player)
        {
            ExecutionEvaluationContext evaluationContext = new ExecutionEvaluationContext
            {
                Type = EvaluationContextType.Block
            };

            return card.Abilities.Where(a => a.AbilityType == AbilityType.Block).All(a => a.CanPlay(player, evaluationContext));
        }

        #endregion
    }
}
