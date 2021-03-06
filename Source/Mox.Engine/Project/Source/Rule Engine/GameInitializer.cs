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
using Mox.Database;
using System.Diagnostics;

namespace Mox
{
    /// <summary>
    /// Initializes a game before it starts.
    /// </summary>
    public class GameInitializer
    {
        #region Variables

        private readonly Dictionary<Player, IDeck> m_decks = new Dictionary<Player, IDeck>();
        private readonly ICardFactory m_cardFactory;
        private readonly ICardDatabase m_cardDatabase;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        public GameInitializer(ICardFactory cardFactory, ICardDatabase cardDatabase)
        {
            Throw.IfNull(cardFactory, "cardFactory");
            Throw.IfNull(cardDatabase, "cardDatabase");

            m_cardFactory = cardFactory;
            m_cardDatabase = cardDatabase;

            StartingPlayerLife = 20;
            Seed = -1;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Starting player life.
        /// </summary>
        public int StartingPlayerLife
        {
            get;
            set;
        }

        /// <summary>
        /// Starting seed.
        /// </summary>
        /// <remarks>
        /// Negative means automatic seeding.
        /// </remarks>
        public int Seed
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Assigns the given <paramref name="deck"/> to the given <paramref name="player"/>.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="deck"></param>
        public void AssignDeck(Player player, IDeck deck)
        {
            Throw.IfNull(player, "player");
            m_decks[player] = deck;
        }

        /// <summary>
        /// Initializes the given game.
        /// </summary>
        /// <param name="game"></param>
        public void Initialize(Game game)
        {
            Throw.IfNull(game, "game");

            using (game.Controller.BeginCommandGroup())
            {
                InitializeRandom(game);

                InitializePlayers(game);

                InitializeDecks(game);

                InitializeCards(game);
            }
        }

        private void InitializeRandom(Game game)
        {
            IRandom random = Seed < 0 ? Random.New() : Random.New(Seed);

            game.UseRandom(random);
        }

        private void InitializePlayers(Game game)
        {
            foreach (Player player in game.Players)
            {
                player.Life = StartingPlayerLife;
            }
        }

        private void InitializeDecks(Game game)
        {
            foreach (Player player in game.Players)
            {
                IDeck deck;
                if (m_decks.TryGetValue(player, out deck))
                {
                    foreach (CardIdentifier cardIdentifier in deck.Cards)
                    {
                        var resolvedCard = m_cardDatabase.ResolveCardIdentifier(cardIdentifier);

                        Card card = game.CreateCard(player, resolvedCard);
                        card.Zone = game.Zones.Library;
                    }
                }
            }
        }

        private void InitializeCards(Game game)
        {
            foreach (var card in game.Cards)
            {
                var result = MasterCardFactory.Initialize(card, m_cardFactory, m_cardDatabase);
                switch (result.Type)
                {
                    case CardFactoryResult.ResultType.Success:
                        break;

                    case CardFactoryResult.ResultType.NotImplemented:
                        Trace.TraceError($"Card {card.Name} is not implemented yet: {result.Error}");
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #endregion
    }
}
