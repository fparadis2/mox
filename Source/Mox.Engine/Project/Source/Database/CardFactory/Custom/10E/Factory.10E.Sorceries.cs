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
using System.Linq;

using Mox.Abilities;

namespace Mox.Database.Sets
{
#warning todo spell_v2
    /*
    #region Sorceries

    #region Red

    [CardFactory("Cone of Flame")]
    public class ConeOfFlameCardFactory : CardFactory
    {
        #region Abilities

        // Cone of Flame deals 1 damage to target creature or player, 2 damage to another target creature or player, and 3 damage to a third target creature or player.
        private class DamageAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target1 = Target.Creature() | Target.Player();
                spell.AddCost(target1);

                var target2 = target1.ExceptThisResult();
                spell.AddCost(target2);

                var target3 = target2.ExceptThisResult();
                spell.AddCost(target3);

                spell.Effect = s =>
                {
                    s.Resolve(target1).DealDamage(1);
                    s.Resolve(target2).DealDamage(2);
                    s.Resolve(target3).DealDamage(3);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    [CardFactory("Cryoclasm")]
    public class CryoclasmCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target Plains or Island. Cryoclasm deals 3 damage to that land's controller.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnySubType(SubType.Plains, SubType.Island);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Card land = s.Resolve(target);
                    Player controller = land.Controller;
                    land.Destroy();
                    controller.DealDamage(3);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Demolish")]
    public class DemolishCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target artifact or land.
        private class DestroyAbility : PlayCardAbility
        {
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact | Type.Land);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Goblin Lore")]
    public class GoblinLoreCardFactory : CardFactory
    {
        #region Abilities

        // Draw four cards, then discard three cards at random.
        private class DrawAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    Player player = s.Controller;
                    player.DrawCards(4);

                    for (int i = 0; i < 3; i++)
                    {
                        player.Discard(player.Manager.Random.Choose(player.Hand));
                    }
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DrawAbility>(card);
        }

        #endregion
    }

    [CardFactory("Pyroclasm")]
    public class PyroclasmCardFactory : CardFactory
    {
        #region Abilities

        // Pyroclasm deals 2 damage to each creature.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    foreach (Card creature in s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature)))
                    {
                        creature.DealDamage(2);
                    }
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Spitting Earth")]
    public class SpittingEarthCardFactory : CardFactory
    {
        #region Abilities

        // Spitting Earth deals damage equal to the number of Mountains you control to target creature.
        private class SpittingEarthAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    int numMountains = s.Controller.Battlefield.Where(card => card.Is(SubType.Mountain)).Count();
                    s.Resolve(target).DealDamage(numMountains);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<SpittingEarthAbility>(card);
        }

        #endregion
    }

    [CardFactory("Threaten")]
    public class ThreatenCardFactory : CardFactory
    {
        #region Abilities

        // Untap target creature and gain control of it until end of turn. That creature gains haste until end of turn.
        private class ThreatenAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    var creature = s.Resolve(target);
                    creature.Untap();
                    AddEffect.On(creature).GainControl(s.Controller).UntilEndOfTurn();
                    AddEffect.On(creature).GainAbility<HasteAbility>().UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ThreatenAbility>(card);
        }

        #endregion
    }

    #endregion

    #region White

    [CardFactory("Angelic Blessing")]
    public class AngelicBlessingCardFactory : CardFactory
    {
        #region Abilities

        // Target creature gets +3/+3 and gains flying until end of turn.
        private class BlessingAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    var creature = s.Resolve(target);
                    AddEffect.On(creature).ModifyPowerAndToughness(+3, +3).UntilEndOfTurn();
                    AddEffect.On(creature).GainAbility<FlyingAbility>().UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BlessingAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Black

    [CardFactory("Assassinate")]
    public class AssassinateCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target tapped creature.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature().Tapped();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Deathmark")]
    public class DeathmarkCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target green or white creature.
        private class DestroyAbility : PlayCardAbility
        {
            private Color m_color = Color.Green | Color.White;
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<DestroyAbility>("Color", a => a.m_color, PropertyFlags.Private);

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature().OfAnyColor(m_color);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Essence Drain")]
    public class EssenceDrainCardFactory : CardFactory
    {
        #region Abilities

        // Essence Drain deals 3 damage to target creature or player and you gain 3 life.
        private class DamageAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(3);
                    s.Controller.GainLife(3);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    [CardFactory("Rain of Tears")]
    public class RainOfTearsCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target land.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Land);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Stronghold Discipline")]
    public class StrongholdDisciplineCardFactory : CardFactory
    {
        #region Abilities

        // Each player loses 1 life for each creature he or she controls.
        private class DamageAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    foreach (Player player in s.Game.Players)
                    {
                        player.LoseLife(CountCreatures(player));
                    }
                };
            }

            private static int CountCreatures(Player player)
            {
                return player.Battlefield.Where(c => c.Is(Type.Creature)).Count();
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Blue

    [CardFactory("Traumatize")]
    public class TraumatizeCardFactory : CardFactory
    {
        #region Abilities

        // Target player puts the top half of his or her library, rounded down, into his or her graveyard.
        private class TraumatizeAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Player player = s.Resolve(target);
                    int numCards = (int)Math.Floor(player.Library.Count / 2.0f);
                    player.Graveyard.MoveToTop(player.Library.Top(numCards));
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<TraumatizeAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Green

    [CardFactory("Creeping Mold")]
    public class CreepingMoldCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target artifact, enchantment, or land.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact | Type.Enchantment | Type.Land);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    #endregion

    #endregion

    */
}
