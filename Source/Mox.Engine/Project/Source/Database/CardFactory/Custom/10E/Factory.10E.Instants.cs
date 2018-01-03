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
using Mox.Flow;

namespace Mox.Database.Sets
{
    #region Instants

    #region White

    [CardFactory("Afflict")]
    public class AfflictCardFactory : CardFactory
    {
        // Target creature gets -1/-1 until end of turn.
        // Draw a card.

        #region Abilities

        private class AfflictAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Card card = s.Resolve(target);
                    
                    AddEffect.On(card).ModifyPowerAndToughness(-1, -1).UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<AfflictAbility>(card);
        }

        #endregion
    }

    [CardFactory("Aggressive Urge")]
    public class AggressiveUrgeCardFactory : CardFactory
    {
        // Target creature gets +1/+1 until end of turn.
        // Draw a card.

        #region Abilities

        private class AggressiveUrgeAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Card card = s.Resolve(target);

                    AddEffect.On(card).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<AggressiveUrgeAbility>(card);
        }

        #endregion
    }

    [CardFactory("Beacon of Immortality")]
    public class BeaconOfImmortalityCardFactory : CardFactory
    {
        #region Abilities

        private class BeaconOfImmortalityAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Double target player's life total. Shuffle Beacon of Immortality into its owner's library.
            protected override void PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
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

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BeaconOfImmortalityAbility>(card);
        }

        #endregion
    }

    [CardFactory("Condemn")]
    public class CondemnCardFactory : CardFactory
    {
        #region Abilities

        // Put target attacking creature on the bottom of its owner's library. Its controller gains life equal to its toughness.
        private class CondemnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                TargetCost<Card> target = Target.Creature().Attacking();
                spell.AddCost(target);

                spell.Effect = s =>
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

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<CondemnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Demystify")]
    public class DemystifyCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target enchantment.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Enchantment);
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

    [CardFactory("Reviving Dose")]
    public class RevivingDoseCardFactory : CardFactory
    {
        #region Abilities

        private class GainLifeAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // You gain 3 life.
            // Draw a card.
            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    s.Controller.GainLife(3);
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GainLifeAbility>(card);
        }

        #endregion
    }

    [CardFactory("Righteousness")]
    public class RighteousnessCardFactory : CardFactory
    {
        #region Abilities

        private class BoostAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target blocking creature gets +7/+7 until end of turn.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature().Blocking();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+7, +7).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BoostAbility>(card);
        }

        #endregion
    }

    [CardFactory("Tempest of Light")]
    public class TempestOfLightCardFactory : CardFactory
    {
        #region Abilities

        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Destroy all enchantments.
            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    var enchantments = s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Enchantment)).ToList();
                    enchantments.ForEach(card => card.Destroy());
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

    #region Red

    [CardFactory("Beacon of Destruction")]
    public class BeaconOfDestructionCardFactory : CardFactory
    {
        #region Abilities

        private class BeaconOfDestructionAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Beacon of Destruction deals 5 damage to target creature or player. Shuffle Beacon of Destruction into its owner's library.
            protected override void PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player() | Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(5);
                    s.Source.Zone = s.Game.Zones.Library;
                    s.Controller.Library.Shuffle();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BeaconOfDestructionAbility>(card);
        }

        #endregion
    }

    [CardFactory("Fists of the Anvil")]
    public class FistsOfTheAnvilCardFactory : CardFactory
    {
        #region Abilities

        private class BoostAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +4/+0 until end of turn.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+4, +0).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<BoostAbility>(card);
        }

        #endregion
    }

    [CardFactory("Shock")]
    public class ShockCardFactory : CardFactory
    {
        #region Abilities

        private class ShockAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                TargetCost target = Target.Player() | Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(2);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ShockAbility>(card);
        }

        #endregion
    }

    [CardFactory("Smash")]
    public class SmashCardFactory : CardFactory
    {
        #region Abilities

        private class SmashAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Destroy target artifact.
            // Draw a card.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<SmashAbility>(card);
        }

        #endregion
    }

    [CardFactory("Soulblast")]
    public class SoulblastCardFactory : CardFactory
    {
        #region Abilities

        // As an additional cost to play Soulblast, sacrifice all creatures you control.
        // Soulblast deals damage to target creature or player equal to the total power of the sacrificed creatures.
        private class SoulblastAbility : PlayCardAbility
        {
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                int totalPower = SacrificeAllCreaturesCost.GetControlledCreatures(spell.Controller).Sum(card => card.Power);

                spell.AddCost(new SacrificeAllCreaturesCost());

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(totalPower);
                };
            }
        }

        private class SacrificeAllCreaturesCost : Cost
        {
            #region Overrides of Cost

            public override bool CanExecute(Game game, ExecutionEvaluationContext evaluationContext)
            {
                return true;
            }

            public override void Execute(Part.Context context, Player activePlayer)
            {
                foreach (var creature in GetControlledCreatures(activePlayer).ToList())
                {
                    creature.Sacrifice();
                }
                PushResult(context, true);
            }

            internal static IEnumerable<Card> GetControlledCreatures(Player player)
            {
                return from card in player.Battlefield
                       where card.Is(Type.Creature)
                       select card;
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<SoulblastAbility>(card);
        }

        #endregion
    }

    [CardFactory("Stun")]
    public class StunCardFactory : CardFactory
    {
        #region Abilities

        private class StunAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature can't block this turn.
            // Draw a card.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).GainAbility<CannotBlockAbility>().UntilEndOfTurn();
                    s.Controller.DrawCards(1);
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<StunAbility>(card);
        }

        #endregion
    }

    [CardFactory("Sudden Impact")]
    public class SuddenImpactCardFactory : CardFactory
    {
        #region Abilities

        private class DamageAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Sudden Impact deals damage equal to the number of cards in target player's hand to that player.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Player player = s.Resolve(target);
                    player.DealDamage(player.Hand.Count);
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

    #endregion

    #region Green

    [CardFactory("Giant Growth")]
    public class GiantGrowthCardFactory : CardFactory
    {
        #region Abilities

        private class GrowthAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +3/+3 until end of turn.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+3, +3).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GrowthAbility>(card);
        }

        #endregion
    }

    [CardFactory("Might of Oaks")]
    public class MightOfOaksCardFactory : CardFactory
    {
        #region Abilities

        private class GrowthAbility : PlayCardAbility
        {
            #region Overrides of Ability

            // Target creature gets +7/+7 until end of turn.
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+7, +7).UntilEndOfTurn();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<GrowthAbility>(card);
        }

        #endregion
    }

    [CardFactory("Naturalize")]
    public class NaturalizeCardFactory : CardFactory
    {
        #region Abilities

        // Destroy target artifact or enchantment.
        private class DestroyAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Card().OfAnyType(Type.Artifact | Type.Enchantment);
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

    #region Blue

    [CardFactory("Boomerang")]
    public class BoomerangCardFactory : CardFactory
    {
        #region Abilities

        // Return target permanent to its owner's hand.
        private class ReturnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Permanent();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ReturnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Deluge")]
    public class DelugeCardFactory : CardFactory
    {
        #region Abilities

        // Tap all creatures without flying.
        private class TapAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    foreach (Card creature in s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature) && !card.HasAbility<FlyingAbility>()))
                    {
                        creature.Tap();
                    }
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Evacuation")]
    public class EvacuationCardFactory : CardFactory
    {
        #region Abilities

        // Return all creatures to their owners' hands.
        private class ReturnAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                spell.Effect = s =>
                {
                    var creatures = s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature)).ToList();
                    foreach (Card creature in creatures)
                    {
                        creature.ReturnToHand();
                    }
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<ReturnAbility>(card);
        }

        #endregion
    }

    [CardFactory("Hurkyl's Recall")]
    public class HurkylsRecallCardFactory : CardFactory
    {
        #region Abilities

        // Return all artifacts target player owns to his or her hand.
        private class RecallAbility : PlayCardAbility
        {
            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
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

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<RecallAbility>(card);
        }

        #endregion
    }

    [CardFactory("Unsummon")]
    public class UnsummonCardFactory : CardFactory
    {
        #region Abilities

        // Return target creature to its owner's hand.
        private class UnsummonAbility : PlayCardAbility
        {
            #region Overrides of Ability

            protected override void PlaySpecific(Spell spell)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<UnsummonAbility>(card);
        }

        #endregion
    }

    #endregion

    #endregion
}
