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
using System.Linq;
using Mox.Database.Library;
using Mox.Events;
using Mox.Flow;

namespace Mox.Database.Sets
{
    #region Vanilla

    [CardFactory("Craw Wurm")]
    [CardFactory("Dross Crocodile")]
    [CardFactory("Earth Elemental")]
    [CardFactory("Enormous Baloth")]
    [CardFactory("Fugitive Wizard")]
    [CardFactory("Goblin Piker")]
    [CardFactory("Grizzly Bears")]
    [CardFactory("Hill Giant")]
    [CardFactory("Lumengrid Warden")]
    [CardFactory("Mass of Ghouls")]
    [CardFactory("Scathe Zombies")]
    [CardFactory("Spined Wurm")]
    public class VanillaCards10E : MTGCardFactory
    {
    }

    #endregion

    #region Other simple creatures

    [CardFactory("Air Elemental")]
    [CardFactory("Dusk Imp")]
    [CardFactory("Goblin Sky Raider")]
    [CardFactory("Mahamoti Djinn")]
    [CardFactory("Ornithopter")]
    [CardFactory("Snapping Drake")]
    [CardFactory("Suntail Hawk")]
    [CardFactory("Wild Griffin")]
    public class FlyingCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FlyingAbility>(card);
        }
    }

    [CardFactory("Canopy Spider")]
    [CardFactory("Giant Spider")]
    public class ReachCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<ReachAbility>(card);
        }
    }

    [CardFactory("Anaba Bodyguard")]
    [CardFactory("Youthful Knight")]
    [CardFactory("Tundra Wolves")]
    public class FirstStrikeCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FirstStrikeAbility>(card);
        }
    }

    [CardFactory("Steadfast Guard")]
    public class VigilanceCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<VigilanceAbility>(card);
        }
    }

    [CardFactory("Lightning Elemental")]
    [CardFactory("Raging Goblin")]
    [CardFactory("Thundering Giant")]
    public class HasteCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<HasteAbility>(card);
        }
    }

    [CardFactory("Wall of Wood")]
    public class DefenderCreatureFactory10E : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<DefenderAbility>(card);
        }
    }

    #endregion

    #region White

    [CardFactory("Ancestor's Chosen")]
    public class AncestorsChosenCardFactory : FirstStrikeCreatureFactory10E
    {
        #region Abilities

        // When Ancestor's Chosen comes into play, you gain 1 life for each card in your graveyard.
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(s.Controller.Graveyard.Count);
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Angel of Mercy")]
    public class AngelOfMercyCardFactory : FlyingCreatureFactory10E
    {
        #region Abilities

        // When Angel of Mercy comes into play, you gain 3 life.
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(3);
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Angelic Wall")]
    public class AngelicWallCardFactory : DefenderCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FlyingAbility>(card);
        }
    }

    [CardFactory("Aven Cloudchaser")]
    public class AvenCloudchaserCardFactory : FlyingCreatureFactory10E
    {
        // When Aven Cloudchaser comes into play, destroy target enchantment.
        private class DestroyAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Card().OfAnyType(Type.Enchantment);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DestroyAbility>(card);
        }
    }

    [CardFactory("Aven Fisher")]
    public class AvenFisherCardFactory : FlyingCreatureFactory10E
    {
        // When Aven Fisher is put into a graveyard from play, you may draw a card.
        private class DrawAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    ModalChoiceContext choice = ModalChoiceContext.YesNo("Draw a card?", ModalChoiceResult.Yes, ModalChoiceImportance.Trivial);

                    if (c.Controller.AskModalChoice(c, s.Controller, choice) == ModalChoiceResult.Yes)
                    {
                        s.Controller.DrawCards(1);
                    }
                };

                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Benalish Knight")]
    public class BenalishKnightCardFactory : FirstStrikeCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            CreateAbility<FlashAbility>(card);

            base.Initialize(card, context);
        }
    }

    [CardFactory("Ghost Warden")]
    public class GhostWardenCardFactory : MTGCardFactory
    {
        #region Abilities

        // T: Target creature gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(Source);

                var targetCreature = Target.Creature();
                yield return targetCreature;

                spell.Effect = (s, c) =>
                {
                    Card card = s.Resolve(targetCreature);
                    AddEffect.On(card).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Honor Guard")]
    public class HonorGuardCardFactory : MTGCardFactory
    {
        #region Abilities

        // W: Honor Guard gets +0/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("W"));

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+0, +1).UntilEndOfTurn();
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Icatian Priest")]
    public class IcatianPriestCardFactory : MTGCardFactory
    {
        #region Abilities

        // 1WW: Target creature gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("1WW"));
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Loxodon Mystic")]
    public class LoxodonMysticCardFactory : MTGCardFactory
    {
        #region Abilities

        // W, T Tap target creature.
        private class TapAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("W"));
                yield return Tap(spell.Source);

                TargetCost targetCreature = Target.Creature();
                yield return targetCreature;

                spell.Effect = (s, c) =>
                {
                    ((Card)s.Resolve(targetCreature)).Tap();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<TapAbility>(card);
        }
    }

    [CardFactory("Serra Angel")]
    public class SerraAngelCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<VigilanceAbility>(card);
        }
    }

    [CardFactory("Skyhunter Patrol")]
    public class SkyhunterPatrolCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FirstStrikeAbility>(card);
        }
    }

    [CardFactory("Skyhunter Prowler")]
    public class SkyhunterProwlerCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<VigilanceAbility>(card);
        }
    }

    [CardFactory("Skyhunter Skirmisher")]
    public class SkyhunterSkirmisherCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<DoubleStrikeAbility>(card);
        }
    }

    [CardFactory("Soul Warden")]
    public class SoulWardenCardFactory : MTGCardFactory
    {
        #region Abilities

        // Whenever another creature comes into play, you gain 1 life.
        private class GainLifeAbility : AnyCreatureComesIntoPlayAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(1);
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Spirit Weaver")]
    public class SpiritWeaverCardFactory : MTGCardFactory
    {
        // 2: Target green or blue creature gets +0/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(BoostAbility), PropertyFlags.Private, Color.Green | Color.Blue);

            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("2"));

                var target = Target.Creature().OfAnyColor(GetValue(ColorProperty));
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+0, +1).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Starlight Invoker")]
    public class StarlightInvokerCardFactory : MTGCardFactory
    {
        // 7W: You gain 5 life.
        private class GainLifeAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("7W"));

                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(5);
                };

                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Tangle Spider")]
    public class TangleSpiderCardFactory : ReachCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            CreateAbility<FlashAbility>(card);

            base.Initialize(card, context);
        }
    }

    [CardFactory("Venerable Monk")]
    public class VenerableMonkCardFactory : MTGCardFactory
    {
        #region Abilities

        // When Venerable Monk comes into play, you gain 2 life.
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.GainLife(2);
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Wall of Swords")]
    public class WallOfSwordsCardFactory : DefenderCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FlyingAbility>(card);
        }
    }

    #endregion

    #region Red

    [CardFactory("Bloodfire Colossus")]
    public class BloodfireColossusCardFactory : MTGCardFactory
    {
        #region Abilities

        // R, Sacrifice Bloodfire Colossus: Bloodfire Colossus deals 6 damage to each creature and each player.
        private class SacrificeAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Sacrifice(spell.Source);
                spell.DelayedCosts.Add(PayMana("R"));

                spell.Effect = (s, c) =>
                {
                    const int Damage = 6;

                    foreach (Card creature in c.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature)))
                    {
                        creature.DealDamage(Damage);
                    }

                    foreach (Player player in c.Game.Players)
                    {
                        player.DealDamage(Damage);
                    }
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Bogardan Firefiend")]
    public class BogardanFirefiendCardFactory : MTGCardFactory
    {
        #region Abilities

        // When Bogardan Firefiend is put into a graveyard from play, it deals 2 damage to target creature.
        private class GraveyardAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(2);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<GraveyardAbility>(card);
        }
    }

    [CardFactory("Flamewave Invoker")]
    public class FlamewaveInvokerCardFactory : MTGCardFactory
    {
        // 7R: Flamewave Invoker deals 5 damage to target player.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("7R"));

                var target = Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(5);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Furnace Whelp")]
    public class FurnaceWhelpCardFactory : FlyingCreatureFactory10E
    {
        #region Abilities

        // R: Furnace Whelp gets +1/+0 until end of turn.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("R"));

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, 0).UntilEndOfTurn();
                };

                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Kamahl, Pit Fighter")]
    public class KamahlPitFighterCardFactory : HasteCreatureFactory10E
    {
        private class TapdAbility : InPlayAbility
        {
            // T Kamahl, Pit Fighter deals 3 damage to target creature or player.
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(spell.Source);
                var target = Target.Creature() | Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(3);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<TapdAbility>(card);
        }
    }

    [CardFactory("Mogg Fanatic")]
    public class MoggFanaticCardFactory : MTGCardFactory
    {
        #region Abilities

        // Sacrifice Mogg Fanatic: Mogg Fanatic deals 1 damage to target creature or player.
        private class SacrificeAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Sacrifice(spell.Source);

                var target = Target.Creature() | Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Orcish Artillery")]
    public class OrcishArtilleryCardFactory : MTGCardFactory
    {
        // T Orcish Artillery deals 2 damage to target creature or player and 3 damage to you.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(spell.Source);

                var target = Target.Creature() | Target.Player();

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(2);
                    s.Controller.DealDamage(3);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Prodigal Pyromancer")]
    public class ProdigalPyromancerCardFactory : MTGCardFactory
    {
        #region Abilities

        // T Prodigal Pyromancer deals 1 damage to target creature or player.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                var target = Target.Creature() | Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Rage Weaver")]
    public class RageWeaverCardFactory : FlyingCreatureFactory10E
    {
        #region Abilities

        // 2: Target black or green creature gains haste until end of turn. (It can attack and T this turn.)
        private class TapAbility : InPlayAbility
        {
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(TapAbility), PropertyFlags.Private, Color.Black | Color.Green);

            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("2"));

                TargetCost<Card> target = Target.Creature().OfAnyColor(GetValue(ColorProperty));
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).GainAbility<HasteAbility>().UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<TapAbility>(card);
        }
    }

    [CardFactory("Rock Badger")]
    public class RockBadgerCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Mountain;
        }
    }

    [CardFactory("Shivan Dragon")]
    public class ShivanDragonCardFactory : MTGCardFactory
    {
        // R: Shivan Dragon gets +1/+0 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("R"));

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };

                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Shivan Hellkite")]
    public class ShivanHellkiteCardFactory : FlyingCreatureFactory10E
    {
        // 1R: Shivan Hellkite deals 1 damage to target creature or player.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("1R"));
                var target = Target.Creature() | Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Viashino Sandscout")]
    public class ViashinoSandscoutCardFactory : HasteCreatureFactory10E
    {
        private class ReturnToHandAbility : TriggeredAbility, IEventHandler<EndOfTurnEvent>
        {
            // At end of turn, return Viashino Sandscout to its owner's hand. (Return it only if it's in play.)
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.Effect = (s, c) =>
                {
                    if (s.Source.Zone.ZoneId == Zone.Id.Battlefield)
                    {
                        s.Source.ReturnToHand();
                    }
                };

                yield break;
            }

            public void HandleEvent(Game game, EndOfTurnEvent e)
            {
                Trigger(null);
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<ReturnToHandAbility>(card);
        }
    }

    [CardFactory("Wall of Fire")]
    public class WallOfFireCardFactory : DefenderCreatureFactory10E
    {
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("R"));
                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }
    }

    #endregion

    #region Black

    [CardFactory("Bog Wraith")]
    public class BogWraithCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Swamp;
        }
    }

    [CardFactory("Festering Goblin")]
    public class FesteringGoblinCardFactory : MTGCardFactory
    {
        // When Festering Goblin is put into a graveyard from play, target creature gets -1/-1 until end of turn.
        private class GraveyardAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Creature();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(-1, -1).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<GraveyardAbility>(card);
        }
    }

    [CardFactory("Hate Weaver")]
    public class HateWeaverCardFactory : MTGCardFactory
    {
        // 2: Target blue or red creature gets +1/+0 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(BoostAbility), PropertyFlags.Private, Color.Blue | Color.Red);

            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("2"));

                var target = Target.Creature().OfAnyColor(GetValue(ColorProperty));
                yield return target;

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Highway Robber")]
    public class HighwayRobberCardFactory : MTGCardFactory
    {
        // When Highway Robber comes into play, target opponent loses 2 life and you gain 2 life.
        private class DenizenAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Player().Opponent(spell.Controller);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).LoseLife(2);
                    s.Controller.GainLife(2);
                };
                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DenizenAbility>(card);
        }
    }

    [CardFactory("Looming Shade")]
    public class LoomingShadeCardFactory : MTGCardFactory
    {
        // B: Looming Shade gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("B"));
                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };

                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Nantuko Husk")]
    public class NantukoHuskCardFactory : MTGCardFactory
    {
        #region Abilities

        // Sacrifice a creature: Nantuko Husk gets +2/+2 until end of turn.
        private class SacrificeAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Target.Creature().Sacrifice();

                spell.Effect = (s, c) =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+2, +2).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Nightmare")]
    public class NightmareCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Nightmare's power and toughness are each equal to the number of Swamps you control.
            Func<Object, PowerAndToughness> pt = o =>
            {
                int value = ((Card)o).Controller.Battlefield.Where(c => c.Is(SubType.Swamp)).Count();
                return new PowerAndToughness { Power = value, Toughness = value };
            };
            AddEffect.On(card).SetPowerAndToughness(pt, Card.SubTypesProperty).Forever();
        }
    }

    [CardFactory("Phyrexian Rager")]
    public class PhyrexianRagerCardFactory : MTGCardFactory
    {
        // When Phyrexian Rager comes into play, you draw a card and you lose 1 life.
        private class DrawAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.DrawCards(1);
                    s.Controller.LoseLife(1);
                };
                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Plague Beetle")]
    public class PlagueBeetleCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Swamp;
        }
    }

    [CardFactory("Royal Assassin")]
    public class RoyalAssassinCardFactory : MTGCardFactory
    {
        // T Destroy target tapped creature.
        private class AssassinateAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(spell.Source);

                var target = Target.Creature().Tapped();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<AssassinateAbility>(card);
        }
    }

    [CardFactory("Spineless Thug")]
    public class SpinelessThugCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<CannotBlockAbility>(card);
        }
    }

    #endregion

    #region Blue

    [CardFactory("Ambassador Laquatus")]
    public class AmbassadorLaquatusCardFactory : MTGCardFactory
    {
        // 3: Target player puts the top three cards of his or her library into his or her graveyard.
        private class MillAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("3"));
                var target = Target.Player();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    Player player = s.Resolve(target);
                    var top3Cards = player.Library.Top(3);
                    player.Graveyard.MoveToTop(top3Cards);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<MillAbility>(card);
        }
    }

    [CardFactory("Arcanis the Omnipotent")]
    public class ArcanisTheOmnipotentCardFactory : MTGCardFactory
    {
        // T Draw three cards.
        private class DrawAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(spell.Source);

                spell.Effect = (s, c) =>
                {
                    s.Controller.DrawCards(3);
                };
            }
        }

        // 2UU: Return Arcanis the Omnipotent to its owner's hand.
        private class ReturnToHandAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("2UU"));

                spell.Effect = (s, c) =>
                {
                    s.Source.ReturnToHand();
                };

                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<DrawAbility>(card);
            CreateAbility<ReturnToHandAbility>(card);
        }
    }

    [CardFactory("Denizen of the Deep")]
    public class DenizenOfTheDeepCardFactory : MTGCardFactory
    {
        // When Denizen of the Deep comes into play, return each other creature you control to its owner's hand.
        private class DenizenAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    var creatures = s.Controller.Battlefield.Where(controlledCard => controlledCard.Is(Type.Creature)).ToList();
                    foreach (Card creature in creatures)
                    {
                        if (creature != card.Resolve(s.Game))
                        {
                            creature.ReturnToHand();
                        }
                    }
                };
                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DenizenAbility>(card);
        }
    }

    [CardFactory("Horseshoe Crab")]
    public class HorseshoeCrabCardFactory : MTGCardFactory
    {
        #region Abilities

        // U: Untap Horseshoe Crab.
        private class UntapAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("U"));

                spell.Effect = (s, c) =>
                {
                    s.Source.Untap();
                };
                yield break;
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<UntapAbility>(card);
        }
    }

    [CardFactory("Rootwater Commando")]
    public class RootwaterCommandoCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Island;
        }
    }

    [CardFactory("Sky Weaver")]
    public class SkyWeaverCardFactory : MTGCardFactory
    {
        // 2: Target white or black creature gains flying until end of turn.
        private class GainFlyingAbility : InPlayAbility
        {
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof(GainFlyingAbility), PropertyFlags.Private, Color.White | Color.Black);

            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("2"));
                var target = Target.Creature().OfAnyColor(GetValue(ColorProperty));
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<GainFlyingAbility>(card);
        }
    }

    [CardFactory("Vedalken Mastermind")]
    public class VedalkenMastermindCardFactory : MTGCardFactory
    {
        #region Abilities

        // U, T Return target permanent you control to its owner's hand.
        private class SacrificeAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                spell.DelayedCosts.Add(PayMana("U"));

                yield return Tap(spell.Source);

                var target = Target.Permanent().UnderControl(spell.Controller);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Wall of Air")]
    public class WallOfAirCardFactory : DefenderCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<FlyingAbility>(card);
        }
    }

    #endregion

    #region Green

    [CardFactory("Elven Riders")]
    public class ElvenRidersCardFactory : MTGCardFactory
    {
        #region Abilities

        // Elven Riders can't be blocked except by Walls and/or creatures with flying.
        private class BlockAbility : EvasionAbility
        {
            public override bool CanBlock(Card attacker, Card blocker)
            {
                return blocker.Is(SubType.Wall) || blocker.HasAbility<FlyingAbility>();
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<BlockAbility>(card);
        }
    }

    [CardFactory("Femeref Archers")]
    public class FemerefArchersCardFactory : MTGCardFactory
    {
        // T Femeref Archers deals 4 damage to target attacking creature with flying.
        private class DamageAbility : InPlayAbility
        {
            public override IEnumerable<ImmediateCost> Play(Spell spell)
            {
                yield return Tap(spell.Source);

                var target = Target.Creature().Attacking().With<FlyingAbility>();
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).DealDamage(4);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Kavu Climber")]
    public class KavuClimberCardFactory : MTGCardFactory
    {
        // When Kavu Climber comes into play, draw a card.
        private class DrawAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = (s, c) =>
                {
                    s.Controller.DrawCards(1);
                };
                yield break;
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Llanowar Elves")]
    public class LlanowarElvesCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            var tapAbility = CreateAbility<TapForManaAbility>(card);
            tapAbility.Color = Color.Green;
        }
    }

    [CardFactory("Mirri, Cat Warrior")]
    public class MirriCatWarriorCardFactory : FirstStrikeCreatureFactory10E
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Forest;
            CreateAbility<VigilanceAbility>(card);
        }
    }

    [CardFactory("Rushwood Dryad")]
    public class RushwoodDryadCardFactory : MTGCardFactory
    {
        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Forest;
        }
    }

    [CardFactory("Viridian Shaman")]
    public class ViridianShamanCardFactory : MTGCardFactory
    {
        // When Viridian Shaman comes into play, destroy target artifact.
        private class DestroyAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override IEnumerable<ImmediateCost> Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Card().OfAnyType(Type.Artifact);
                yield return target;

                spell.Effect = (s, c) =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DestroyAbility>(card);
        }
    }

    #endregion
}
