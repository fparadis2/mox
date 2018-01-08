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
using System.Reflection;
using Mox.Flow;

namespace Mox.Abilities
{
    /// <summary>
    /// Types of abilities.
    /// </summary>
    public enum AbilityType
    {
        /// <summary>
        /// Normal (activated) ability, including simply playing a card.
        /// </summary>
        Normal,
        /// <summary>
        /// Triggered ability (cannot be played directly)
        /// </summary>
        Triggered,
        /// <summary>
        /// Attack ability.
        /// </summary>
        Attack,
        /// <summary>
        /// Block ability.
        /// </summary>
        Block,
        /// <summary>
        /// Static ability.
        /// </summary>
        Static
    }

    public enum AbilitySpeed
    {
        /// <summary>
        /// Instant speed (flash-like)
        /// </summary>
        Instant,
        /// <summary>
        /// Sorcery speed
        /// </summary>
        Sorcery
    }

    /// <summary>
    /// An ability (activated or triggered).
    /// </summary>
    /// <remarks>
    /// Mox also treats the "playing of a card" as an ability.
    /// </remarks>
    public abstract class Ability : GameObject
    {
        #region Variables

        private readonly Card m_source = null;
        public static readonly Property<Card> SourceProperty = Property<Card>.RegisterProperty<Ability>("Source", a => a.m_source, PropertyFlags.Private);

        #endregion

        #region Properties

        /// <summary>
        /// Source of the ability.
        /// </summary>
        public Card Source
        {
            get { return m_source; }
        }

        public Player Controller
        {
            get { return Source.Controller; }
        }

        /// <summary>
        /// Game of the ability.
        /// </summary>
        public Game Game
        {
            get { return Source.Manager; }
        }

        /// <summary>
        /// Returns the type of this ability.
        /// </summary>
        public virtual AbilityType AbilityType
        {
            get { return AbilityType.Normal; }
        }

        /// <summary>
        /// Returns true if this ability is a mana ability.
        /// </summary>
        public virtual bool IsManaAbility
        {
            get { return false; }
        }

        /// <summary>
        /// Returns the possible outcome when this is a mana ability.
        /// </summary>
        public virtual void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            throw new InvalidProgramException("Should check IsManaAbility OR override FillManaOutcome");
        }

        public virtual AbilitySpeed AbilitySpeed
        {
            get { return AbilitySpeed.Instant; }
        }

        public virtual string AbilityText
        {
            get
            {
                AbilityTextAttribute attribute = GetType().GetCustomAttribute<AbilityTextAttribute>();
                if (attribute != null)
                    return attribute.Text;

                throw new NotImplementedException(string.Format("Ability of type {0} doesn't have an ability text.", GetType().FullName));
            }
        }

        #region Basic Costs

        /// <summary>
        /// A cost that can never be played.
        /// </summary>
        protected static Cost CannotPlay
        {
            get { return Cost.CannotPlay; }
        }

        /// <summary>
        /// A cost that requires that the object be tapped.
        /// </summary>
        protected static Cost Tap(Card card)
        {
            return Cost.Tap(card);
        }

        /// <summary>
        /// A cost that requires the controller to pay the given <paramref name="manaCost"/>.
        /// </summary>
        /// <param name="manaCost"></param>
        /// <returns></returns>
        protected static Cost PayMana(ManaCost manaCost)
        {
            return new PayManaCost(manaCost);
        }

        /// <summary>
        /// A cost that requires the controller to pay the given <paramref name="manaCost"/>.
        /// </summary>
        /// <param name="manaCost"></param>
        /// <returns></returns>
        protected static Cost PayMana(string manaCost)
        {
            return PayMana(ManaCost.Parse(manaCost));
        }

        /// <summary>
        /// A cost that requires the controller to sacrifice the given <paramref name="card"/>.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        protected static Cost Sacrifice(Card card)
        {
            return new SacrificeCost(card);
        }

        protected static class Target
        {
            public static TargetCost<Player> Player()
            {
                return TargetCost.Player();
            }

            public static TargetCost<Card> Card()
            {
                return TargetCost.Card();
            }

            public static TargetCost<Card> Creature()
            {
                return TargetCost.Creature();
            }

            public static TargetCost<Card> Permanent()
            {
                return TargetCost.Permanent();
            }
        }

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the ability can be played.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanPlay(AbilityEvaluationContext evaluationContext)
        {
            // During mana payment, we can only play mana abilities, etc.
            if (!evaluationContext.CanPlay(this))
            {
                return false;
            }

            // Can only play abilities we control.
            if (Controller != evaluationContext.Player)
            {
                return false;
            }

            if (!CanPlayConsideringTiming(evaluationContext.Player))
            {
                return false;
            }

            return true;
        }

        private bool CanPlayConsideringTiming(Player player)
        {
            switch (AbilitySpeed)
            {
                case AbilitySpeed.Sorcery:
                    return CanPlaySorcery(player);

                case AbilitySpeed.Instant:
                    return true;

                default:
                    throw new NotImplementedException();
            }
        }

        private bool CanPlaySorcery(Player player)
        {
            if (!Game.SpellStack.IsEmpty)
            {
                return false;
            }

            if (Game.State.ActivePlayer != player || !Game.State.CurrentPhase.IsMainPhase())
            {
                return false;
            }

            return true;
        }

        protected override void Init()
        {
            base.Init();

            UpdateSource(null, Source, this);
        }

        protected override void Uninit()
        {
            UpdateSource(Source, null, this);

            base.Uninit();
        }

        private static void UpdateSource(Card oldSource, Card newSource, Ability ability)
        {
            if (oldSource != null)
            {
                bool result = oldSource.InternalAbilities.Remove(ability);
                Debug.Assert(result);
            }

            if (newSource != null)
            {
                Debug.Assert(!newSource.InternalAbilities.Contains(ability));
                newSource.InternalAbilities.Add(ability);
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SourceProperty && !Equals(e.NewValue, Source))
            {
                UpdateSource((Card)e.OldValue, (Card)e.NewValue, this);
            }
        }

        #endregion
    }
}
