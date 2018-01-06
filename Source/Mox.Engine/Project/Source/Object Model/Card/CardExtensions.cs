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
using System.Diagnostics;
using System.Linq;

using Mox.Abilities;

namespace Mox
{
    /// <summary>
    /// Card extension methods.
    /// </summary>
    public static class CardExtensions
    {
        #region Constants

        public const Type PermanentTypes = Type.Artifact | Type.Creature | Type.Enchantment | Type.Land | Type.Planeswalker;

        #endregion

        #region Methods

        #region Is

        #region SuperType

        /// <summary>
        /// Returns true if the card is of the given <paramref name="type"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Is(this Card card, SuperType type)
        {
            return card.SuperType.Is(type);
        }

        public static bool Is(this SuperType tested, SuperType type)
        {
            return (tested & type) == type;
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="type"/>(s) (any of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAny(this Card card, SuperType type)
        {
            return card.SuperType.IsAny(type);
        }

        public static bool IsAny(this SuperType tested, SuperType type)
        {
            return (tested & type) > 0;
        }

        /// <summary>
        /// Returns true if the card is exactly of the given <paramref name="type"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsExactly(this Card card, SuperType type)
        {
            return card.SuperType == type;
        }

        #endregion

        #region Type

        /// <summary>
        /// Returns true if the card is of the given <paramref name="type"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool Is(this Card card, Type type)
        {
            return card.Type.Is(type);
        }

        public static bool Is(this Type tested, Type type)
        {
            return (tested & type) == type;
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="type"/>(s) (any of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsAny(this Card card, Type type)
        {
            return card.Type.IsAny(type);
        }

        public static bool IsAny(this Type tested, Type type)
        {
            return (tested & type) > 0;
        }

        /// <summary>
        /// Returns true if the card is exactly of the given <paramref name="type"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsExactly(this Card card, Type type)
        {
            return card.Type == type;
        }

        #endregion

        #region SubType

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subType"/>.
        /// </summary>
        /// <returns></returns>
        public static bool Is(this Card card, SubType subType)
        {
            return card.SubTypes.Is(subType);
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subTypes"/> (all of them).
        /// </summary>
        /// <returns></returns>
        public static bool IsAll(this Card card, params SubType[] subTypes)
        {
            return card.SubTypes.IsAll(subTypes);
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="subTypes"/> (any of them).
        /// </summary>
        /// <returns></returns>
        public static bool IsAny(this Card card, params SubType[] subTypes)
        {
            return card.SubTypes.IsAny(subTypes);
        }

        #endregion

        #region Color

        /// <summary>
        /// Returns true if the card is of the given <paramref name="color"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool Is(this Card card, Color color)
        {
            return (card.Color & color) == color;
        }

        /// <summary>
        /// Returns true if the card is of the given <paramref name="color"/>(s) (any of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsAny(this Card card, Color color)
        {
            return (card.Color & color) > 0;
        }

        /// <summary>
        /// Returns true if the card is exactly of the given <paramref name="color"/>(s) (all of them).
        /// </summary>
        /// <param name="card"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool IsExactly(this Card card, Color color)
        {
            return card.Color == color;
        }

        #endregion

        /// <summary>
        /// Returns true if the card can be a permanent.
        /// </summary>
        /// <param name="card"></param>
        /// <returns></returns>
        public static bool IsPermanent(this Card card)
        {
            return card.IsAny(PermanentTypes);
        }

        /// <summary>
        /// Returns true if the card is visible to the given <paramref name="player"/>.
        /// </summary>
        /// <remarks>
        /// Null player means all players.
        /// </remarks>
        /// <param name="card"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool IsVisible(this Card card, Player player)
        {
            switch (card.ZoneId)
            {
                case Zone.Id.Graveyard:
                case Zone.Id.PhasedOut:
                case Zone.Id.Battlefield:
                case Zone.Id.Exile:
                case Zone.Id.Stack:
                    return true;

                case Zone.Id.Hand:
                    return card.Owner == player;

                case Zone.Id.Library:
                    return false;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion

        #region Actions

        #region Attach

        /// <summary>
        /// Attaches this card to the given target.
        /// </summary>
        public static void Attach(this Card attached, Card target)
        {
            EnsureAttachmentIsLegal(attached, target);
            attached.AttachedTo = target;
        }

        [Conditional("DEBUG")]
        private static void EnsureAttachmentIsLegal(Card attached, Card target)
        {
            if (target != null)
            {
                if (attached.IsAny(SubType.Aura, SubType.Equipment))
                {
                    Debug.Assert(target.Is(Type.Creature), "Can only attach auras/equipments on creatures");
                }
                else if (attached.Is(SubType.Fortification))
                {
                    Debug.Assert(target.Is(Type.Land), "Can only attach fortifications on lands");
                }
                else
                {
                    Debug.Fail("Can only attach aura, equipments and fortifications");
                }

                Debug.Assert(target.Zone.ZoneId == Zone.Id.Battlefield, "Can only attach to objects on the battlefield");
            }
        }

        #endregion

        #region Destroy

        public static void Destroy(this Card card)
        {
            Throw.InvalidArgumentIf(!card.IsPermanent(), "Cannot destroy a non-permanent!", "card");
            using (card.Manager.Controller.BeginCommandGroup())
            {
                card.Zone = card.Manager.Zones.Graveyard;
                card.ResetValue(Card.DamageProperty);
            }
        }

        #endregion

        #region ReturnToHand

        public static void ReturnToHand(this Card card)
        {
            card.Zone = card.Manager.Zones.Hand;
        }

        #endregion

        #region Sacrifice

        public static void Sacrifice(this Card card)
        {
            Debug.Assert(card.Zone == card.Manager.Zones.Battlefield);
            card.Zone = card.Manager.Zones.Graveyard;
        }

        #endregion

        #region Tap / Untap

        public static void Tap(this Card card)
        {
            TapImpl(card, true);
        }

        public static void Untap(this Card card)
        {
            TapImpl(card, false);
        }

        private static void TapImpl(Card card, bool tap)
        {
            Throw.InvalidArgumentIf(card.Zone != card.Manager.Zones.Battlefield, "Cannot (un)tap a card that is not on the battlefield", "card");
            card.Tapped = tap;
        }

        #endregion

        #endregion

        #region Abilities

        public static bool HasAbility<TAbility>(this Card card)
            where TAbility : Ability
        {
            return card.Abilities.OfType<TAbility>().Any();
        }

        #endregion

        #endregion
    }
}
