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

using Mox.Flow;

namespace Mox
{
    public delegate void SpellEffect(Spell s, Part<IGameController>.Context c);

    /// <summary>
    /// A spell is a card or an ability on the stack.
    /// </summary>
    /// <remarks>
    /// Note that the formal definition of a spell is a card on the stack (not an ability), hence the <see cref="SpellType"/> property.
    /// </remarks>
    public class Spell
    {
        #region Variables

        private readonly Game m_game;
        private readonly Ability m_ability;
        private readonly Player m_controller;
        private readonly object m_context;

        private readonly List<Cost> m_costs = new List<Cost>();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Spell(Game game, Ability ability, Player controller)
            : this(game, ability, controller, null)
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public Spell(Game game, Ability ability, Player controller, object context)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(ability, "ability");
            Throw.IfNull(controller, "controller");

            m_game = game;
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
            m_game = game;
            m_ability = Resolvable<Ability>.Resolve(game, spell.Ability);
            m_controller = Resolvable<Player>.Resolve(game, spell.Controller);
            m_context = spell.Context;

            UseStack = spell.UseStack;
            Effect = spell.Effect;
            PreEffect = spell.PreEffect;
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
            get { return m_game; }
        }

        /// <summary>
        /// Effect the spell has upon resolution.
        /// </summary>
        public SpellEffect Effect
        {
            get;
            set;
        }

        /// <summary>
        /// Effect the spell has before being pushed on the stack.
        /// </summary>
        public SpellEffect PreEffect
        {
            get;
            set;
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
        public IList<Cost> Costs
        {
            get { return m_costs; }
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
            private readonly SpellEffect m_preEffect;
            private readonly SpellEffect m_effect;

            public Storage(Spell spell)
            {
                m_ability = spell.Ability;
                m_controller = spell.Controller;
                m_context = spell.Context;

                m_useStack = spell.UseStack;
                m_preEffect = spell.PreEffect;
                m_effect = spell.Effect;
            }

            public Spell CreateSpell(Game game)
            {
                Ability ability = m_ability.Resolve(game);
                Player controller = m_controller.Resolve(game);

                Spell spell = new Spell(game, ability, controller, m_context)
                { 
                    UseStack = m_useStack, 
                    PreEffect = m_preEffect, 
                    Effect = m_effect 
                };

                return spell;
            }
        }

        #endregion
    }
}
