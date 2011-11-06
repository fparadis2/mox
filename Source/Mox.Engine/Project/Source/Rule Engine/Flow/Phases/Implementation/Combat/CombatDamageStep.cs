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
using System.Diagnostics;
using System.Linq;

using Mox.Flow.Parts;

namespace Mox.Flow.Phases
{
    public class CombatDamageStep : Step
    {
        #region Variables

        // For First/Double Strike
        private enum Wave
        {
            First,
            Second,
        }

        private readonly Wave m_wave;

        #endregion

        #region Constructor

        public CombatDamageStep()
            : this(Wave.First)
        {
        }

        private CombatDamageStep(Wave wave)
            : base(Steps.CombatDamage)
        {
            m_wave = wave;
        }

        #endregion

        #region Methods

        protected override NewPart SequenceImpl(NewPart.Context context, Player player)
        {
            if (!context.Game.CombatData.Attackers.IsEmpty)
            {
                // TODO: Support more than 2 players
                Player defendingPlayer = Player.GetNextPlayer(player);

                Wave wave = m_wave;
                XStrikeSplitter splitter = new XStrikeSplitter(context.Game.CombatData, ref wave, context.Game.CombatData);

                context.Schedule(new AssignAttackerDamage(player, splitter.Attackers));
                context.Schedule(new AssignBlockerDamage(defendingPlayer, splitter.Blockers));
                NewPart result = base.SequenceImpl(context, player);

                if (wave == Wave.First)
                {
                    context.Game.CombatData.XStrikeCreatures = new CombatData.XStrikeCreaturesResult(splitter.Attackers.Union(splitter.Blockers).ToArray());
                    context.Schedule(new SequenceStep(player, new CombatDamageStep(Wave.Second)));
                }

                return result;
            }

            return null;
        }

        private static int GetCombatDamage(Card card)
        {
            return card.Power;
        }

        private static void AssignDamageToCards(int damage, IEnumerable<Card> damagedCards)
        {
            List<int> damageToAssign = new List<int>();

            foreach (Card card in damagedCards)
            {
                int damageDone = Math.Min(damage, card.Toughness - card.Damage);
                damageToAssign.Add(damageDone);
                damage -= damageDone;
            }

            if (damageToAssign.Count > 0)
            {
                // Add remaining damage to last card
                damageToAssign[damageToAssign.Count - 1] += damage;
            }

            int i = 0;
            foreach (Card card in damagedCards)
            {
                card.DealDamage(damageToAssign[i++]);
            }
        }

        private static IEnumerable<Card> GetCards(Game game, IEnumerable<int> identifiers)
        {
            return identifiers.Select(game.GetObjectByIdentifier<Card>);
        }

        #endregion

        #region Inner Parts

        private class AssignAttackerDamage : PlayerPart
        {
            private readonly IEnumerable<int> m_attackers;

            public AssignAttackerDamage(Player player, IEnumerable<int> attackers)
                : base(player)
            {
                Debug.Assert(attackers != null);
                m_attackers = attackers;
            }

            public override NewPart Execute(Context context)
            {
                // TODO: Support more than 2 players
                Player defendingPlayer = Player.GetNextPlayer(GetPlayer(context));

                foreach (Card attacker in GetCards(context.Game, m_attackers))
                {
                    Debug.Assert(context.Game.CombatData.Attackers.AttackerIdentifiers.Contains(attacker.Identifier));

                    int combatDamage = GetCombatDamage(attacker);
                    if (!context.Game.CombatData.IsBlocked(attacker))
                    {
                        defendingPlayer.DealDamage(combatDamage);
                    }
                    else
                    {
                        AssignDamageToCards(combatDamage, context.Game.CombatData.GetBlockers(attacker));
                    }
                }

                return null;
            }
        }

        private class AssignBlockerDamage : PlayerPart
        {
            private readonly IEnumerable<int> m_blockers;

            public AssignBlockerDamage(Player player, IEnumerable<int> blockers)
                : base(player)
            {
                Debug.Assert(blockers != null);
                m_blockers = blockers;
            }

            public override NewPart Execute(Context context)
            {
                foreach (Card blocker in GetCards(context.Game, m_blockers))
                {
                    Card card = blocker;
                    Debug.Assert(context.Game.CombatData.Blockers.Blockers.Any(pair => pair.BlockingCreatureId == card.Identifier));

                    int combatDamage = GetCombatDamage(blocker);
                    AssignDamageToCards(combatDamage, context.Game.CombatData.GetAttackers(blocker));
                }

                return null;
            }
        }

        private class XStrikeSplitter
        {
            #region Variables

            private readonly List<int> m_attackers = new List<int>();
            private readonly List<int> m_blockers = new List<int>();

            #endregion

            #region Constructor

            public XStrikeSplitter(CombatData combatData, ref Wave wave, CombatData data)
            {
                var attackers = combatData.Attackers.GetAttackers(combatData.Manager);
                var blockers = combatData.Blockers.GetBlockers(combatData.Manager);

                Split(combatData, wave, attackers, m_attackers);
                Split(combatData, wave, blockers, m_blockers);

                if (wave == Wave.First && m_attackers.Count == 0 && m_blockers.Count == 0)
                {
                    wave = Wave.Second;
                    Split(combatData, wave, attackers, m_attackers);
                    Split(combatData, wave, blockers, m_blockers);
                }
            }

            #endregion

            #region Properties

            public IEnumerable<int> Attackers
            {
                get { return m_attackers; }
            }

            public IEnumerable<int> Blockers
            {
                get { return m_blockers; }
            }

            #endregion

            #region Methods

            private static void Split(CombatData combatData, Wave wave, IEnumerable<Card> cards, ICollection<int> result)
            {
                int[] alreadyAssignedDamage = combatData.XStrikeCreatures.Creatures;

                foreach (Card card in cards)
                {
                    if (card.HasAbility<DoubleStrikeAbility>())
                    {
                        result.Add(card.Identifier);
                    }
                    else if (!alreadyAssignedDamage.Contains(card.Identifier))
                    {
                        if (card.HasAbility<FirstStrikeAbility>() || wave != Wave.First)
                        {
                            result.Add(card.Identifier);
                        }
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}
