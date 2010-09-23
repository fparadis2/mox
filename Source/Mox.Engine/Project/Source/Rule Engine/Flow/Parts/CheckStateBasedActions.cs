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

namespace Mox.Flow.Parts
{
    /// <summary>
    /// A part that checks state-based actions just before priority is given to a player.
    /// </summary>
    public class CheckStateBasedActions : Part<IGameController>
    {
        #region Overrides of Part

        public override Part<IGameController> Execute(Context context)
        {
            Game game = context.Game;

            bool hasResultedInAction = false;

            hasResultedInAction |= CheckPlayerLife(game);
            hasResultedInAction |= CheckCards(game);

            return hasResultedInAction && !game.State.HasEnded ? new CheckStateBasedActions() : null;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 704.5a If a player has 0 or less life, he or she loses the game.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        private static bool CheckPlayerLife(Game game)
        {
            foreach (Player player in game.Players)
            {
                if (player.Life <= 0)
                {
                    // TODO: Support more than 2 players
                    game.State.Winner = Player.GetNextPlayer(player);
                    return true;
                }
            }

            return false;
        }

        private static bool CheckCards(Game game)
        {
            LegendRule legendRule = new LegendRule();
            bool actionWasTaken = false;

            foreach (Card card in game.Cards)
            {
                Zone zone = card.Zone;

                if (zone == game.Zones.Battlefield || zone == game.Zones.Exile)
                {
                    Type type = card.Type;
                    var subTypes = card.SubTypes;

                    if (type.Is(Type.Creature))
                    {
                        actionWasTaken |= CheckCreature(game, card);
                    }

                    if (type.Is(Type.Enchantment))
                    {
                        actionWasTaken |= CheckEnchantment(game, card, subTypes);
                    }

                    if (type.Is(Type.Artifact))
                    {
                        actionWasTaken |= CheckArtifact(game, card, subTypes);
                    }

                    // 704.5q If a creature is attached to an object or player, it becomes unattached and remains on the battlefield. Similarly, if a permanent that's neither an Aura, an Equipment, nor a Fortification is attached to an object or player, it becomes unattached and remains on the battlefield.
                    if (card.AttachedTo != null && (type.Is(Type.Creature) || !subTypes.IsAny(SubType.Aura, SubType.Equipment, SubType.Fortification)))
                    {
                        card.Attach(null);
                        actionWasTaken |= true;
                    }

                    if (card.Is(SuperType.Legendary))
                    {
                        legendRule.Consider(card);
                    }
                }
            }

            actionWasTaken |= legendRule.Apply(game);
            return actionWasTaken;
        }

        /// <summary>
        /// 704.5f If a creature has toughness 0 or less, it's put into its owner's graveyard. Regeneration can't replace this event.
        /// 704.5g If a creature has toughness greater than 0, and the total damage marked on it is greater than or equal to its toughness, that creature has been dealt lethal damage and is destroyed. Regeneration can replace this event.
        /// </summary>
        /// <returns></returns>
        private static bool CheckCreature(Game game, Card creatureCard)
        {
            if (creatureCard.Toughness <= 0)
            {
                creatureCard.Zone = game.Zones.Graveyard;
                return true;
            }

            if (creatureCard.Damage >= creatureCard.Toughness)
            {
                creatureCard.Destroy();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 704.5n If an Aura is attached to an illegal object or player, or is not attached to an object or player, that Aura is put into its owner's graveyard.
        /// </summary>
        /// <returns></returns>
        private static bool CheckEnchantment(Game game, Card enchantmentCard, SubTypes subTypes)
        {
            if (subTypes.Is(SubType.Aura))
            {
                Card attachedTo = enchantmentCard.AttachedTo;

                if (attachedTo == null ||
                    !attachedTo.Is(Type.Creature) || // Probably going to check more than type for complex enchantments
                    attachedTo.Zone != game.Zones.Battlefield)
                {
                    enchantmentCard.Zone = game.Zones.Graveyard;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 704.5p If an Equipment or Fortification is attached to an illegal permanent, it becomes unattached from that permanent. It remains on the battlefield.
        /// </summary>
        /// <returns></returns>
        private static bool CheckArtifact(Game game, Card artifactCard, SubTypes subTypes)
        {
            bool isEquipment = subTypes.Is(SubType.Equipment);
            bool isFortifaction = subTypes.Is(SubType.Fortification);

            if (isEquipment || isFortifaction)
            {
                Card attachedTo = artifactCard.AttachedTo;

                if (attachedTo != null)
                {
                    if (attachedTo.Zone != game.Zones.Battlefield)
                    {
                        artifactCard.Attach(null);
                        return true;
                    }

                    Type attachedToType = attachedTo.Type;

                    if (isEquipment && !attachedToType.Is(Type.Creature))
                    {
                        artifactCard.Attach(null);
                        return true;
                    }

                    if (isFortifaction && !attachedToType.Is(Type.Land))
                    {
                        artifactCard.Attach(null);
                        return true;
                    }
                }
            }

            return false;
        }

        private class LegendRule
        {
            private readonly List<Card> m_legendaryCards = new List<Card>();

            public void Consider(Card card)
            {
                Debug.Assert(card.Is(SuperType.Legendary));
                m_legendaryCards.Add(card);
            }

            public bool Apply(Game game)
            {
                bool actionTaken = false;
                Zone graveyard = game.Zones.Graveyard;

                foreach (var grouping in m_legendaryCards.GroupBy(c => c.Name))
                {
                    if (grouping.Count() > 1)
                    {
                        actionTaken = true;
                        grouping.ForEach(c => c.Zone = graveyard);
                    }
                }

                return actionTaken;
            }
        }

        #endregion
    }
}
