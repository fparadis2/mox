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
    /// <summary>
    /// Contains data that pertains to the current combat phase.
    /// </summary>
    public class CombatData : GameObject
    {
        #region Variables

        private static readonly Property<DeclareAttackersResult> AttackersProperty = Property<DeclareAttackersResult>.RegisterProperty("Attackers", typeof(CombatData));
        private static readonly Property<DeclareBlockersResult> BlockersProperty = Property<DeclareBlockersResult>.RegisterProperty("Blockers", typeof(CombatData));
        private static readonly Property<XStrikeCreaturesResult> XStrikeCreaturesProperty = Property<XStrikeCreaturesResult>.RegisterProperty("XStrikeCreatures", typeof(CombatData));

        #endregion

        #region Properties

        /// <summary>
        /// Attackers
        /// </summary>
        public DeclareAttackersResult Attackers
        {
            get { return GetValue(AttackersProperty) ?? new DeclareAttackersResult(); }
            set { SetValue(AttackersProperty, value); }
        }

        /// <summary>
        /// Blockers.
        /// </summary>
        public DeclareBlockersResult Blockers
        {
            get { return GetValue(BlockersProperty) ?? new DeclareBlockersResult(); }
            set { SetValue(BlockersProperty, value); }
        }

        /// <summary>
        /// XStrikeCreatures.
        /// </summary>
        internal XStrikeCreaturesResult XStrikeCreatures
        {
            get { return GetValue(XStrikeCreaturesProperty) ?? new XStrikeCreaturesResult(new int[0]); }
            set { SetValue(XStrikeCreaturesProperty, value); }
        }

        #endregion

        #region Methods

        #region Accessors

        /// <summary>
        /// Gets whether the given <paramref name="attacker"/> is blocked.
        /// </summary>
        /// <remarks>
        /// This does NOT guarantee that there actually are blockers blocking the attacker. They might have been removed from combat by another effect.
        /// The rules state: "A creature remains blocked even if all the creatures blocking it are removed from combat."
        /// </remarks>
        /// <param name="attacker"></param>
        /// <returns></returns>
        public bool IsBlocked(Card attacker)
        {
            return Blockers.Blockers.Any(pair => pair.BlockedCreatureId == attacker.Identifier);
        }

        /// <summary>
        /// Gets whether the given <paramref name="card"/> is currently attacking.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public bool IsAttacking(Card card)
        {
            return Attackers.AttackerIdentifiers.Contains(card.Identifier);
        }

        public IEnumerable<Card> GetBlockers(Card attacker)
        {
            return from pair in Blockers.Blockers
                   where pair.BlockedCreatureId == attacker.Identifier && pair.BlockingCreatureId != ObjectManager.InvalidIdentifier
                   select Manager.GetObjectByIdentifier<Card>(pair.BlockingCreatureId);
        }

        public IEnumerable<Card> GetAttackers(Card blocker)
        {
            return from pair in Blockers.Blockers
                   where pair.BlockingCreatureId == blocker.Identifier
                   select Manager.GetObjectByIdentifier<Card>(pair.BlockedCreatureId);
        }

        #endregion

        #region Modifications

        /// <summary>
        /// Removes the given <paramref name="card"/> from combat.
        /// </summary>
        /// <param name="card"></param>
        public void RemoveFromCombat(Card card)
        {
            RemoveFromAttackers(card);
            RemoveFromBlockers(card);
        }

        private void RemoveFromAttackers(Card card)
        {
            DeclareAttackersResult result = Attackers.Clone();
            if (result.Remove(card))
            {
                Attackers = result; // Setting it again to trigger replication, etc...
            }
        }

        private void RemoveFromBlockers(Card card)
        {
            DeclareBlockersResult result = Blockers.Clone();
            if (result.Remove(card))
            {
                Blockers = result; // Setting it again to trigger replication, etc...
            }
        }

        #endregion

        #endregion

        #region Event Handlers

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == AttackersProperty)
            {
                var oldValue = (DeclareAttackersResult)e.OldValue;
                var newValue = (DeclareAttackersResult)e.NewValue;

                if (oldValue != null)
                {
                    foreach (Card attacker in oldValue.GetAttackers(Manager))
                    {
                        attacker.PropertyChanged -= combattant_PropertyChanged;
                    }
                }

                if (newValue != null)
                {
                    foreach (Card attacker in newValue.GetAttackers(Manager))
                    {
                        attacker.PropertyChanged += combattant_PropertyChanged;
                    }
                }
            }
            else if (e.Property == BlockersProperty)
            {
                var oldValue = (DeclareBlockersResult)e.OldValue;
                var newValue = (DeclareBlockersResult)e.NewValue;

                if (oldValue != null)
                {
                    foreach (Card blocker in oldValue.GetBlockers(Manager))
                    {
                        blocker.PropertyChanged -= combattant_PropertyChanged;
                    }
                }

                if (newValue != null)
                {
                    foreach (Card blocker in newValue.GetBlockers(Manager))
                    {
                        blocker.PropertyChanged += combattant_PropertyChanged;
                    }
                }
            }
        }

        void combattant_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Card card = (Card)sender;
            if (e.Property == Card.ZoneIdProperty && card.Zone != Manager.Zones.Battlefield)
            {
                RemoveFromCombat(card);
            }
            else if (e.Property == Card.ControllerProperty)
            {
                RemoveFromCombat(card);
            }
        }

        #endregion

        #region Inner Types

        /// <summary>
        /// Contains a list of creatures that already assigned damaged during the first combat damage step (if any)
        /// </summary>
        internal class XStrikeCreaturesResult
        {
            private readonly int[] m_creatures;

            public XStrikeCreaturesResult(int[] creatures)
            {
                Debug.Assert(creatures != null, "creatures");
                m_creatures = creatures;
            }

            public int[] Creatures
            {
                get { return m_creatures; }
            }
        }

        #endregion
    }
}
