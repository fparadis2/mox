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

using Mox.Abilities;
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

        protected override Part SequenceImpl(Part.Context context, Player player)
        {
            if (!context.Game.CombatData.Attackers.IsEmpty)
            {
                Wave wave = m_wave;
                XStrikeSplitter splitter = new XStrikeSplitter(context.Game.CombatData, ref wave, context.Game.CombatData);

                context.Schedule(new AssignAttackerDamage(splitter.Attackers));
                context.Schedule(new AssignBlockerDamage(splitter.Blockers));
                Part result = base.SequenceImpl(context, player);

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

        private static IEnumerable<Card> GetCards(Game game, IEnumerable<int> identifiers)
        {
            return identifiers.Select(game.GetObjectByIdentifier<Card>);
        }

        #endregion

        #region Inner Parts

        private class AssignAttackerDamage : Part
        {
            private readonly IEnumerable<int> m_attackers;

            public AssignAttackerDamage(IEnumerable<int> attackers)
            {
                Debug.Assert(attackers != null);
                m_attackers = attackers;
            }

            public override Part Execute(Context context)
            {
                DamageAssignment damageAssignment = new DamageAssignment();

                foreach (Card attacker in GetCards(context.Game, m_attackers))
                {
                    Debug.Assert(context.Game.CombatData.Attackers.AttackerIdentifiers.Contains(attacker.Identifier));

                    int combatDamage = GetCombatDamage(attacker);
                    var blockers = context.Game.CombatData.GetBlockers(attacker).ToArray();

                    foreach (var blocker in blockers)
                    {
                        combatDamage = damageAssignment.AssignDamage(combatDamage, blocker);
                    }

                    if (combatDamage > 0)
                    {
                        if (blockers.Length == 0 || attacker.HasAbility<TrampleAbility>())
                        {
                            damageAssignment.AssignDamageToTarget(combatDamage);
                        }
                        else
                        {
                            damageAssignment.AssignRemaining(combatDamage);
                        }
                    }
                }

                damageAssignment.Execute((ITargetable)context.Game.CombatData.AttackTarget);
                return null;
            }
        }

        private class AssignBlockerDamage : Part
        {
            private readonly IEnumerable<int> m_blockers;

            public AssignBlockerDamage(IEnumerable<int> blockers)
            {
                Debug.Assert(blockers != null);
                m_blockers = blockers;
            }

            public override Part Execute(Context context)
            {
                DamageAssignment damageAssignment = new DamageAssignment();

                foreach (Card blocker in GetCards(context.Game, m_blockers))
                {
                    Card card = blocker;
                    Debug.Assert(context.Game.CombatData.Blockers.Blockers.Any(pair => pair.BlockingCreatureId == card.Identifier));

                    int combatDamage = GetCombatDamage(blocker);

                    foreach (var attacker in context.Game.CombatData.GetAttackers(blocker))
                    {
                        combatDamage = damageAssignment.AssignDamage(combatDamage, attacker);
                    }

                    damageAssignment.AssignRemaining(combatDamage);
                }

                damageAssignment.Execute(null);
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

        private class DamageAssignment
        {
            private readonly List<DamageToCard> m_damagesToCards = new List<DamageToCard>();
            private int m_damageToTarget;

            private struct DamageToCard
            {
                public Card Card;
                public int Damage;
                public int DamageToLethal;
            }

            public int AssignDamage(int remaining, Card card)
            {
                var damageToCard = GetDamageToCard(card, out int index);
                int damageDone = Math.Min(remaining, damageToCard.DamageToLethal - damageToCard.Damage);

                damageToCard.Damage += damageDone;
                m_damagesToCards[index] = damageToCard;

                return remaining - damageDone;
            }

            public void AssignRemaining(int remaining)
            {
                // Assign to last card for no particular reason
                if (m_damagesToCards.Count > 0)
                {
                    var damageToCard = m_damagesToCards[m_damagesToCards.Count - 1];
                    damageToCard.Damage += remaining;
                    m_damagesToCards[m_damagesToCards.Count - 1] = damageToCard;
                }
            }

            public void AssignDamageToTarget(int damage)
            {
                m_damageToTarget += damage;
            }

            public void Execute(ITargetable target)
            {
                foreach (var damageToCard in m_damagesToCards)
                {
                    if (damageToCard.Damage > 0)
                    {
                        damageToCard.Card.DealDamage(damageToCard.Damage);
                    }
                }

                if (target != null && m_damageToTarget > 0)
                {
                    target.DealDamage(m_damageToTarget);
                }
            }

            private DamageToCard GetDamageToCard(Card card, out int index)
            {
                for (int i = 0; i < m_damagesToCards.Count; i++)
                {
                    if (m_damagesToCards[i].Card == card)
                    {
                        index = i;
                        return m_damagesToCards[i];
                    }
                }

                DamageToCard newDamage = new DamageToCard
                {
                    Card = card,
                    DamageToLethal = card.Toughness - card.Damage
                };

                index = m_damagesToCards.Count;
                m_damagesToCards.Add(newDamage);
                return newDamage;
            }
        }

        #endregion
    }
}
