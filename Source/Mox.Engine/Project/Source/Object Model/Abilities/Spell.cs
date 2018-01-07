﻿// Copyright (c) François Paradis
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

using Mox.Flow;

namespace Mox.Abilities
{
    public delegate void SpellEffect(SpellResolutionContext s);

    /// <summary>
    /// A spell is a card or an ability on the stack.
    /// </summary>
    /// <remarks>
    /// Note that the formal definition of a spell is a card on the stack (not an ability), hence the <see cref="SpellType"/> property.
    /// </remarks>
    public class Spell
    {
        #region Variables

        private readonly Ability m_ability;
        private readonly Player m_controller;
        private readonly object m_context;

        private readonly List<Cost> m_costs = new List<Cost>();
        private readonly List<Action> m_actions = new List<Action>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Spell(Ability ability, Player controller, object context = null)
        {
            Throw.IfNull(ability, "ability");
            Throw.IfNull(controller, "controller");

            m_ability = ability;
            m_controller = controller;
            m_context = context;

            UseStack = !ability.IsManaAbility;
        }

        /// <summary>
        /// "Resolving" constructor
        /// </summary>
        /// <remarks>
        /// <see cref="Costs"/> are not copied.
        /// </remarks>
        private Spell(Spell spell, Game game)
        {
            m_ability = Resolvable<Ability>.Resolve(game, spell.Ability);
            m_controller = Resolvable<Player>.Resolve(game, spell.Controller);
            m_context = spell.Context;

            UseStack = spell.UseStack;
            EffectPart = spell.EffectPart;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Source of the spell.
        /// </summary>
        public Card Source
        {
            get { return m_ability.Source; }
        }

        /// <summary>
        /// Ability represented by the spell.
        /// </summary>
        public Ability Ability
        {
            get { return m_ability; }
        }

        /// <summary>
        /// Controller of the spell.
        /// </summary>
        public Player Controller
        {
            get { return m_controller; }
        }

        /// <summary>
        /// Ability-specific context.
        /// </summary>
        public object Context
        {
            get { return m_context; }
        }

        /// <summary>
        /// Game from which this spell comes from.
        /// </summary>
        public Game Game
        {
            get { return m_controller.Manager; }
        }

        public Part EffectPart
        {
            get; 
            set;
        }

        /// <summary>
        /// Effect the spell has upon resolution.
        /// </summary>
        public SpellEffect Effect
        {
            set { EffectPart = value == null ? null : new SimpleEffectPart(value); }
        }

        /// <summary>
        /// Whether the spell should be pushes on the stack (or "resolved" immediatly).
        /// </summary>
        public bool UseStack
        {
            get;
            set;
        }

        /// <summary>
        /// Contains the costs that are required to play the spell.
        /// </summary>
        public IReadOnlyList<Cost> Costs
        {
            get { return m_costs; }
        }

        /// <summary>
        /// Contains the actions that happen when the spell resolves.
        /// </summary>
        public IReadOnlyList<Action> Actions
        {
            get { return m_actions; }
        }

        /// <summary>
        /// Type of 'stack object'
        /// </summary>
        public SpellType SpellType
        {
            get 
            {
                if (Ability is PlayCardAbility)
                {
                    return SpellType.Spell;
                }

                return SpellType.Ability; 
            }
        }

        #endregion

        #region Methods

        public void AddCost(Cost cost)
        {
            m_costs.Add(cost);
        }

        public void AddAction(Action action)
        {
            m_actions.Add(action);
        }

        public Spell Resolve(Game game, bool forceNew)
        {
            if (game == Game && !forceNew)
            {
                return this;
            }
            
            return new Spell(this, game);
        }

        internal Storage ToStorage()
        {
            return new Storage(this);
        }

        #endregion

        #region Inner Types

        [Serializable]
        internal class Storage
        {
            private readonly Resolvable<Ability> m_ability;
            private readonly Resolvable<Player> m_controller;
            private readonly object m_context;

            private readonly bool m_useStack;

            public Storage(Spell spell)
            {
                m_ability = spell.Ability;
                m_controller = spell.Controller;
                m_context = spell.Context;

                m_useStack = spell.UseStack;
            }

            public Spell CreateSpell(Game game)
            {
                Ability ability = m_ability.Resolve(game);
                Player controller = m_controller.Resolve(game);

                Spell spell = new Spell(ability, controller, m_context)
                { 
                    UseStack = m_useStack
                };

                return spell;
            }
        }

        private class SimpleEffectPart : Part, ISpellEffectPart
        {
            private readonly SpellEffect m_spellEffect;

            public SimpleEffectPart(SpellEffect spellEffect)
            {
                m_spellEffect = spellEffect;
            }

            public override Part Execute(Context context)
            {
                var spell = this.PopSpell(context);

                var spellResolutionContext = new SpellResolutionContext(context.Game, spell);
                m_spellEffect(spellResolutionContext);
                return null;
            }
        }

        #endregion
    }
}
