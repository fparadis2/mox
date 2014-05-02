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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// Represents a game.
    /// </summary>
    public partial class Game : ObjectManager
    {
        #region Variables

        private readonly IList<Player> m_players;
        private readonly IList<Card> m_cards;
        private readonly IList<Ability> m_abilities;

        private readonly GameState m_gameState;
        private readonly GlobalData m_globalData;
        private readonly TurnData m_turnData;
        private readonly CombatData m_combatData;
        private readonly TargetData m_targetData;

        private ILog m_log = LogContext.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public Game()
        {
            m_eventRepository = new EventRepository(this);
            m_players = new ReadOnlyCollection<Player>(RegisterInventory<Player>());
            m_cards = RegisterInventory<Card>();
            m_abilities = RegisterInventory<Ability>();

            using (DisableController())
            {
                m_gameState = Create<GameState>();
                Objects.Add(m_gameState);

                m_globalData = Create<GlobalData>();
                Objects.Add(m_globalData);

                m_turnData = Create<TurnData>();
                Objects.Add(m_turnData);

                m_combatData = Create<CombatData>();
                Objects.Add(m_combatData);
            }

            m_zones = new GameZones(this);
            m_spellStack = new SpellStack(this);
            m_targetData = new TargetData(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Players taking part in the game.
        /// </summary>
        public IList<Player> Players
        {
            get { return m_players; }
        }

        /// <summary>
        /// Cards in this game.
        /// </summary>
        public ICollection<Card> Cards
        {
            get { return m_cards; }
        }

        /// <summary>
        /// Abilities defined in this game.
        /// </summary>
        public ICollection<Ability> Abilities
        {
            get { return m_abilities; }
        }

        /// <summary>
        /// Game State.
        /// </summary>
        public GameState State
        {
            get { return m_gameState; }
        }

        /// <summary>
        /// Global Data.
        /// </summary>
        public GlobalData GlobalData
        {
            get { return m_globalData; }
        }

        /// <summary>
        /// Turn data. Reset every turn.
        /// </summary>
        public TurnData TurnData
        {
            get { return m_turnData; }
        }

        /// <summary>
        /// Combat data.
        /// </summary>
        public CombatData CombatData
        {
            get { return m_combatData; }
        }

        internal TargetData TargetData
        {
            get { return m_targetData; }
        }

        public ILog Log
        {
            get
            {
                Debug.Assert(m_log != null);
                return m_log;
            }
            set
            {
                m_log = value ?? LogContext.Empty;
            }
        }

        #endregion

        #region Methods

        #region Creation

        /// <summary>
        /// Creates a new player in this game.
        /// </summary>
        public Player CreatePlayer()
        {
            Player player = Create<Player>();
            Objects.Add(player);
            return player;
        }

        /// <summary>
        /// Creates a card.
        /// </summary>
        /// <returns></returns>
        public Card CreateCard(Player owner, CardIdentifier cardIdentifier)
        {
            Throw.IfNull(owner, "owner");
            Throw.InvalidArgumentIf(cardIdentifier.IsInvalid, "Invalid card identifier", "cardIdentifier");

            using (Controller.BeginCommandGroup())
            {
                Card card = Create<Card>();
                SetObjectValue(card, Card.OwnerProperty, owner);
                SetObjectValue(card, Card.CardIdentifierProperty, cardIdentifier);
                SetObjectValue(card, Card.ControllerProperty, card.Owner);
                Objects.Add(card);
                return card;
            }
        }

        #region CreateAbility

        /// <summary>
        /// Creates an ability.
        /// </summary>
        /// <typeparam name="TAbility"></typeparam>
        /// <returns></returns>
        public TAbility CreateAbility<TAbility>(Card abilitySource)
            where TAbility : Ability, new()
        {
            return CreateAbility<TAbility>(abilitySource, null);
        }

        /// <summary>
        /// Creates a scoped ability.
        /// </summary>
        /// <typeparam name="TAbility"></typeparam>
        /// <typeparam name="TObjectScope"></typeparam>
        /// <returns></returns>
        public TAbility CreateScopedAbility<TAbility, TObjectScope>(Card abilitySource)
            where TAbility : Ability, new()
            where TObjectScope : IObjectScope, new()
        {
            return CreateAbility<TAbility>(abilitySource, typeof(TObjectScope));
        }

        private TAbility CreateAbility<TAbility>(Card abilitySource, System.Type scopeType)
            where TAbility : Ability, new()
        {
            Throw.IfNull(abilitySource, "abilitySource");

            using (Controller.BeginCommandGroup())
            {
                TAbility ability = Create<TAbility>(scopeType);
                SetObjectValue(ability, Ability.SourceProperty, abilitySource);
                Objects.Add(ability);
                return ability;
            }
        }

        #endregion

        #region Effects

        #region Global

        public TrackingEffectInstance CreateTrackingEffect(EffectBase effect, Condition condition, Zone zone)
        {
            return CreateTrackingEffect(effect, condition, zone, null);
        }

        public TrackingEffectInstance CreateScopedTrackingEffect<TObjectScope>(EffectBase effect, Condition condition, Zone zone)
            where TObjectScope : IObjectScope, new()
        {
            return CreateTrackingEffect(effect, condition, zone, typeof(TObjectScope));
        }

        private TrackingEffectInstance CreateTrackingEffect(EffectBase effect, Condition condition, Zone zone, System.Type objectScopeType)
        {
            Throw.IfNull(condition, "condition");
            Throw.IfNull(zone, "zone");

            return CreateEffect<TrackingEffectInstance>(effect, objectScopeType, e =>
            {
                SetObjectValue(e, TrackingEffectInstance.ConditionProperty, condition);
                SetObjectValue(e, TrackingEffectInstance.ZoneProperty, zone.ZoneId);
            });
        }

        #endregion

        #endregion

        #endregion

        #region Hash

        public override void ComputeHash(Hash hash)
        {
            base.ComputeHash(hash);

            m_targetData.ComputeHash(hash);
        }

        #endregion

        #endregion
    }
}
