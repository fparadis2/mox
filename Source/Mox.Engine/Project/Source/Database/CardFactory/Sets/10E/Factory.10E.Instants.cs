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
using System.Linq;
using Mox.Database.Library;

namespace Mox.Database.Sets
{
    #region Instants

    #region White

    [CardFactory("Afflict")]
    public class AfflictCardFactory : MTGCardFactory
    {
        // Target creature gets -1/-1 until end of turn.
        // Draw a card.

        #region Abilities

        private class AfflictAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Card card = s.Resolve(target);
                    
                    AddEffect.On(card).ModifyPowerAndToughness(-1, -1).UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<AfflictAbility>(card);
        }

        #endregion
    }

    [CardFactory("Aggressive Urge")]
    public class AggressiveUrgeCardFactory : MTGCardFactory
    {
        // Target creature gets +1/+1 until end of turn.
        // Draw a card.

        #region Abilities

        private class AggressiveUrgeAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Card card = s.Resolve(target);

                    AddEffect.On(card).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<AggressiveUrgeAbility>(card);
        }

        #endregion
    }

    [CardFactory("Beacon of Immortality")]
    public class BeaconOfImmortalityCardFactory : MTGCardFactory
    {
        #region Abilities

        private class BeaconOfImmortalityAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Double target player's life total. Shuffle Beacon of Immortality into its owner's library.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Player player = ((Player) s.Resolve(target));
                    player.GainLife(player.Life);

                    s.Source.Zone = s.Game.Zones.Library;
                    s.Controller.Library.Shuffle();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BeaconOfImmortalityAbility>(card);
        }

        #endregion
    }

    [CardFactory("Condemn")]
    public class CondemnCardFactory : MTGCardFactory
    {
        #region Abilities

        // Put target attacking creature on the bottom of its owner's library. Its controller gains life equal to its toughness.
        private class CondemnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                TargetCost<Card> target = Target.Creature().Attacking();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Card card = s.Resolve(target);
                    int toughness = card.Toughness;
                    Player controller = card.Controller;

                    //card.Controller = card.Owner;
                    card.Owner.Library.MoveToBottom(new[] { card });
                    controller.GainLife(toughness);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<CondemnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Demystify")]
    public class DemystifyCardFactory : MTGCardFactory
    {
        #region Abilities

        // Destroy target enchantment.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Enchantment);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    [CardFactory("Reviving Dose")]
    public class RevivingDoseCardFactory : MTGCardFactory
    {
        #region Abilities

        private class GainLifeAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // You gain 3 life.
            // Draw a card.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(3);
                    s.Controller.DrawCards(1);
                };
                yield break;
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GainLifeAbility>(card);
        }

        #endregion
    }

    [CardFactory("Righteousness")]
    public class RighteousnessCardFactory : MTGCardFactory
    {
        #region Abilities

        private class BoostAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target blocking creature gets +7/+7 until end of turn.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature().Blocking();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+7, +7).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BoostAbility>(card);
        }

        #endregion
    }

    [CardFactory("Tempest of Light")]
    public class TempestOfLightCardFactory : MTGCardFactory
    {
        #region Abilities

        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Destroy all enchantments.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                spell.Effect = (s, c) =>
                {
                    var enchantments = s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Enchantment)).ToList();
                    enchantments.ForEach(card => card.Destroy());
                };
                yield break;
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Red

    [CardFactory("Beacon of Destruction")]
    public class BeaconOfDestructionCardFactory : MTGCardFactory
    {
        #region Abilities

        private class BeaconOfDestructionAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Beacon of Destruction deals 5 damage to target creature or player. Shuffle Beacon of Destruction into its owner's library.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player() | Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(5);
                    s.Source.Zone = s.Game.Zones.Library;
                    s.Controller.Library.Shuffle();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BeaconOfDestructionAbility>(card);
        }

        #endregion
    }

    [CardFactory("Fists of the Anvil")]
    public class FistsOfTheAnvilCardFactory : MTGCardFactory
    {
        #region Abilities

        private class BoostAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +4/+0 until end of turn.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+4, +0).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BoostAbility>(card);
        }

        #endregion
    }

    [CardFactory("Shock")]
    public class ShockCardFactory : MTGCardFactory
    {
        #region Abilities

        private class ShockAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player() | Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(2);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ShockAbility>(card);
        }

        #endregion
    }

    [CardFactory("Smash")]
    public class SmashCardFactory : MTGCardFactory
    {
        #region Abilities

        private class SmashAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Destroy target artifact.
            // Draw a card.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<SmashAbility>(card);
        }

        #endregion
    }

    [CardFactory("Soulblast")]
    public class SoulblastCardFactory : MTGCardFactory
    {
        #region Abilities

        // As an additional cost to play Soulblast, sacrifice all creatures you control.
        // Soulblast deals damage to target creature or player equal to the total power of the sacrificed creatures.
        private class SoulblastAbility : PlayCardAbility
        {
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature() | Target.Player();
                yield return target;

                int totalPower = GetControlledCreatures(spell.Controller).Sum(card => card.Power);

                spell.PreEffect = (s, c) =>
                {
                    foreach (var creature in GetControlledCreatures(s.Controller).ToList())
                    {
                        creature.Sacrifice();
                    }
                };

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(totalPower);
                };
            }

            private static IEnumerable<Card> GetControlledCreatures(Player player)
            {
                return from card in player.Battlefield
                       where card.Is(Type.Creature)
                       select card;
            }
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<SoulblastAbility>(card);
        }

        #endregion
    }

    [CardFactory("Stun")]
    public class StunCardFactory : MTGCardFactory
    {
        #region Abilities

        private class StunAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature can't block this turn.
            // Draw a card.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).GainAbility<CannotBlockAbility>().UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<StunAbility>(card);
        }

        #endregion
    }

    [CardFactory("Sudden Impact")]
    public class SuddenImpactCardFactory : MTGCardFactory
    {
        #region Abilities

        private class DamageAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Sudden Impact deals damage equal to the number of cards in target player's hand to that player.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Player player = s.Resolve(target);
                    player.DealDamage(player.Hand.Count);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Green

    [CardFactory("Giant Growth")]
    public class GiantGrowthCardFactory : MTGCardFactory
    {
        #region Abilities

        private class GrowthAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +3/+3 until end of turn.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+3, +3).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GrowthAbility>(card);
        }

        #endregion
    }

    [CardFactory("Might of Oaks")]
    public class MightOfOaksCardFactory : MTGCardFactory
    {
        #region Abilities

        private class GrowthAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +7/+7 until end of turn.
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+7, +7).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GrowthAbility>(card);
        }

        #endregion
    }

    [CardFactory("Naturalize")]
    public class NaturalizeCardFactory : MTGCardFactory
    {
        #region Abilities

        // Destroy target artifact or enchantment.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact | Type.Enchantment);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<DestroyAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Blue

    [CardFactory("Boomerang")]
    public class BoomerangCardFactory : MTGCardFactory
    {
        #region Abilities

        // Return target permanent to its owner's hand.
        private class ReturnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Permanent();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ReturnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Deluge")]
    public class DelugeCardFactory : MTGCardFactory
    {
        #region Abilities

        // Tap all creatures without flying.
        private class TapAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                spell.Effect = (s, c) =>
                {
                    foreach (Card creature in s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature) && !card.HasAbility<FlyingAbility>()))
                    {
                        creature.Tap();
                    }
                };
                yield break;
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Evacuation")]
    public class EvacuationCardFactory : MTGCardFactory
    {
        #region Abilities

        // Return all creatures to their owners' hands.
        private class ReturnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                spell.Effect = (s, c) =>
                {
                    var creatures = s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature)).ToList();
                    foreach (Card creature in creatures)
                    {
                        creature.ReturnToHand();
                    }
                };
                yield break;
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ReturnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Hurkyl's Recall")]
    public class HurkylsRecallCardFactory : MTGCardFactory
    {
        #region Abilities

        // Return all artifacts target player owns to his or her hand.
        private class RecallAbility : PlayCardAbility
        {
            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Player owner = s.Resolve(target);
                    var ownedArtifacts = from card in s.Game.Zones.Battlefield.AllCards
                                         where card.Is(Type.Artifact) && card.Owner == owner
                                         select card;

                    foreach (Card artifact in ownedArtifacts.ToList())
                    {
                        artifact.ReturnToHand();
                    }
                };
            }
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<RecallAbility>(card);
        }

        #endregion
    }

    [CardFactory("Unsummon")]
    public class UnsummonCardFactory : MTGCardFactory
    {
        #region Abilities

        // Return target creature to its owner's hand.
        private class UnsummonAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override IEnumerable<ImmediateCost> PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<UnsummonAbility>(card);
        }

        #endregion
    }

    #endregion

    #endregion
}
