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
using Mox.Events;
using Mox.Flow;

namespace Mox.Database.Sets
{
    #region Other simple creatures
    
    public class FlyingCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<FlyingAbility>(card);
        }
    }

    public class ReachCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<ReachAbility>(card);
        }
    }

    public class FirstStrikeCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<FirstStrikeAbility>(card);
        }
    }

    public class VigilanceCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<VigilanceAbility>(card);
        }
    }

    public class HasteCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<HasteAbility>(card);
        }
    }

    public class DefenderCreatureFactory10E : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<DefenderAbility>(card);
        }
    }

    #endregion

    /*

    #region White

    [CardFactory("Ancestor's Chosen")]
    public class AncestorsChosenCardFactory : FirstStrikeCreatureFactory10E
    {
        #region Abilities

        [AbilityText(Text = "When Ancestor's Chosen comes into play, you gain 1 life for each card in your graveyard.")]
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.GainLife(s.Controller.Graveyard.Count);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Angel of Mercy")]
    public class AngelOfMercyCardFactory : FlyingCreatureFactory10E
    {
        #region Abilities

        [AbilityText(Text = "When Angel of Mercy comes into play, you gain 3 life.")]
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.GainLife(3);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Aven Cloudchaser")]
    public class AvenCloudchaserCardFactory : FlyingCreatureFactory10E
    {
        [AbilityText(Text = "When Aven Cloudchaser comes into play, destroy target enchantment.")]
        private class DestroyAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Card().OfAnyType(Type.Enchantment);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DestroyAbility>(card);
        }
    }

    [CardFactory("Aven Fisher")]
    public class AvenFisherCardFactory : FlyingCreatureFactory10E
    {
        // When Aven Fisher is put into a graveyard from play, you may draw a card.
        private class DrawAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.EffectPart = new MayDrawCardChoicePart();
            }

            private class MayDrawCardChoicePart : SpellEffectModalChoicePart
            {
                public MayDrawCardChoicePart()
                    : base(ModalChoiceContext.YesNo("Draw a card?", ModalChoiceResult.Yes, ModalChoiceImportance.Trivial))
                {
                }

                protected override Part Execute(Context context, ModalChoiceResult result, Spell spell)
                {
                    if (result == ModalChoiceResult.Yes)
                    {
                        spell.Controller.DrawCards(1);
                    }
                    return null;
                }
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Ghost Warden")]
    public class GhostWardenCardFactory : CardFactory
    {
        #region Abilities

        // T: Target creature gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(Source));

                var targetCreature = Target.Creature();
                spell.AddCost(targetCreature);

                spell.Effect = s =>
                {
                    Card card = s.Resolve(targetCreature);
                    AddEffect.On(card).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Honor Guard")]
    public class HonorGuardCardFactory : CardFactory
    {
        #region Abilities

        // W: Honor Guard gets +0/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("W"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+0, +1).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Icatian Priest")]
    public class IcatianPriestCardFactory : CardFactory
    {
        #region Abilities

        // 1WW: Target creature gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("1WW"));
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Soul Warden")]
    public class SoulWardenCardFactory : CardFactory
    {
        #region Abilities

        // Whenever another creature comes into play, you gain 1 life.
        private class GainLifeAbility : AnyCreatureComesIntoPlayAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.GainLife(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Spirit Weaver")]
    public class SpiritWeaverCardFactory : CardFactory
    {
        // 2: Target green or blue creature gets +0/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            private Color m_color = Color.Green | Color.Blue;
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<BoostAbility>("Color", a => a.m_color, PropertyFlags.Private);

            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2"));

                var target = Target.Creature().OfAnyColor(m_color);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+0, +1).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Starlight Invoker")]
    public class StarlightInvokerCardFactory : CardFactory
    {
        // 7W: You gain 5 life.
        private class GainLifeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("7W"));

                spell.Effect = s =>
                {
                    s.Controller.GainLife(5);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Venerable Monk")]
    public class VenerableMonkCardFactory : CardFactory
    {
        #region Abilities

        [AbilityText(Text = "When Venerable Monk comes into play, you gain 2 life.")]
        private class GainLifeAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.GainLife(2);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<GainLifeAbility>(card);
        }
    }

    #endregion

    #region Red

    [CardFactory("Bloodfire Colossus")]
    public class BloodfireColossusCardFactory : CardFactory
    {
        #region Abilities

        // R, Sacrifice Bloodfire Colossus: Bloodfire Colossus deals 6 damage to each creature and each player.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("R"));
                spell.AddCost(Sacrifice(spell.Source));

                spell.Effect = s =>
                {
                    const int Damage = 6;

                    foreach (Card creature in s.Game.Zones.Battlefield.AllCards.Where(card => card.Is(Type.Creature)))
                    {
                        creature.DealDamage(Damage);
                    }

                    foreach (Player player in s.Game.Players)
                    {
                        player.DealDamage(Damage);
                    }
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Bogardan Firefiend")]
    public class BogardanFirefiendCardFactory : CardFactory
    {
        #region Abilities

        [AbilityText(Text = "When Bogardan Firefiend is put into a graveyard from play, it deals 2 damage to target creature.")]
        private class GraveyardAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(2);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<GraveyardAbility>(card);
        }
    }

    [CardFactory("Flamewave Invoker")]
    public class FlamewaveInvokerCardFactory : CardFactory
    {
        // 7R: Flamewave Invoker deals 5 damage to target player.
        private class DamageAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("7R"));

                var target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(5);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
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
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("R"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, 0).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Kamahl, Pit Fighter")]
    public class KamahlPitFighterCardFactory : HasteCreatureFactory10E
    {
        private class TapdAbility : InPlayAbility
        {
            // T Kamahl, Pit Fighter deals 3 damage to target creature or player.
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));
                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(3);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<TapdAbility>(card);
        }
    }

    [CardFactory("Mogg Fanatic")]
    public class MoggFanaticCardFactory : CardFactory
    {
        #region Abilities

        // Sacrifice Mogg Fanatic: Mogg Fanatic deals 1 damage to target creature or player.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Sacrifice(spell.Source));

                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Orcish Artillery")]
    public class OrcishArtilleryCardFactory : CardFactory
    {
        // T Orcish Artillery deals 2 damage to target creature or player and 3 damage to you.
        private class DamageAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));

                var target = Target.Creature() | Target.Player();

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(2);
                    s.Controller.DealDamage(3);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Prodigal Pyromancer")]
    public class ProdigalPyromancerCardFactory : CardFactory
    {
        #region Abilities

        // T Prodigal Pyromancer deals 1 damage to target creature or player.
        private class DamageAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
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
            private Color m_color = Color.Black | Color.Green;
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<TapAbility>("Color", a => a.m_color, PropertyFlags.Private);

            public override void Play(Spell spell)
            {
                TargetCost<Card> target = Target.Creature().OfAnyColor(m_color);
                spell.AddCost(target);

                spell.AddCost(PayMana("2"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).GainAbility<HasteAbility>().UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<TapAbility>(card);
        }
    }

    [CardFactory("Rock Badger")]
    public class RockBadgerCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Mountain;
        }
    }

    [CardFactory("Shivan Dragon")]
    public class ShivanDragonCardFactory : CardFactory
    {
        // R: Shivan Dragon gets +1/+0 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("R"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Shivan Hellkite")]
    public class ShivanHellkiteCardFactory : FlyingCreatureFactory10E
    {
        // 1R: Shivan Hellkite deals 1 damage to target creature or player.
        private class DamageAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("1R"));
                var target = Target.Creature() | Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Viashino Sandscout")]
    public class ViashinoSandscoutCardFactory : HasteCreatureFactory10E
    {
        [AbilityText(Text = "At end of turn, return Viashino Sandscout to its owner's hand. (Return it only if it's in play.)")]
        private class ReturnToHandAbility : TriggeredAbility, IEventHandler<EndOfTurnEvent>
        {
            public override void Play(Spell spell)
            {
                spell.Effect = s =>
                {
                    if (s.Source.Zone.ZoneId == Zone.Id.Battlefield)
                    {
                        s.Source.ReturnToHand();
                    }
                };
            }

            public void HandleEvent(Game game, EndOfTurnEvent e)
            {
                Trigger(null);
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<ReturnToHandAbility>(card);
        }
    }

    [CardFactory("Wall of Fire")]
    public class WallOfFireCardFactory : DefenderCreatureFactory10E
    {
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("R"));
                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<BoostAbility>(card);
        }
    }

    #endregion

    #region Black

    [CardFactory("Bog Wraith")]
    public class BogWraithCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Swamp;
        }
    }

    [CardFactory("Festering Goblin")]
    public class FesteringGoblinCardFactory : CardFactory
    {
        // When Festering Goblin is put into a graveyard from play, target creature gets -1/-1 until end of turn.
        private class GraveyardAbility : GoesIntoGraveyardFromPlayAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Creature();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(-1, -1).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<GraveyardAbility>(card);
        }
    }

    [CardFactory("Hate Weaver")]
    public class HateWeaverCardFactory : CardFactory
    {
        // 2: Target blue or red creature gets +1/+0 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            private Color m_color = Color.Blue | Color.Red;
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<BoostAbility>("Color", a => a.m_color, PropertyFlags.Private);

            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2"));

                var target = Target.Creature().OfAnyColor(m_color);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Highway Robber")]
    public class HighwayRobberCardFactory : CardFactory
    {
        [AbilityText(Text = "When Highway Robber comes into play, target opponent loses 2 life and you gain 2 life.")]
        private class DenizenAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Player().Opponent(spell.Controller);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).LoseLife(2);
                    s.Controller.GainLife(2);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DenizenAbility>(card);
        }
    }

    [CardFactory("Looming Shade")]
    public class LoomingShadeCardFactory : CardFactory
    {
        // B: Looming Shade gets +1/+1 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("B"));
                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+1, +1).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<BoostAbility>(card);
        }
    }

    [CardFactory("Nantuko Husk")]
    public class NantukoHuskCardFactory : CardFactory
    {
        #region Abilities

        // Sacrifice a creature: Nantuko Husk gets +2/+2 until end of turn.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Target.Creature().Sacrifice());

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).ModifyPowerAndToughness(+2, +2).UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Nightmare")]
    public class NightmareCardFactory : FlyingCreatureFactory10E
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Nightmare's power and toughness are each equal to the number of Swamps you control.
            Func<Object, PowerAndToughness> pt = o =>
            {
                int value = ((Card)o).Controller.Battlefield.Count(c => c.Is(SubType.Swamp));
                return new PowerAndToughness { Power = value, Toughness = value };
            };
            AddEffect.On(card).SetPowerAndToughness(pt, Card.SubTypesProperty).Forever();
        }
    }

    [CardFactory("Phyrexian Rager")]
    public class PhyrexianRagerCardFactory : CardFactory
    {
        [AbilityText(Text = "When Phyrexian Rager comes into play, you draw a card and you lose 1 life.")]
        private class DrawAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.DrawCards(1);
                    s.Controller.LoseLife(1);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Plague Beetle")]
    public class PlagueBeetleCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Swamp;
        }
    }

    [CardFactory("Royal Assassin")]
    public class RoyalAssassinCardFactory : CardFactory
    {
        // T Destroy target tapped creature.
        private class AssassinateAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));

                var target = Target.Creature().Tapped();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<AssassinateAbility>(card);
        }
    }

    [CardFactory("Spineless Thug")]
    public class SpinelessThugCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<CannotBlockAbility>(card);
        }
    }

    #endregion

    #region Blue

    [CardFactory("Ambassador Laquatus")]
    public class AmbassadorLaquatusCardFactory : CardFactory
    {
        // 3: Target player puts the top three cards of his or her library into his or her graveyard.
        private class MillAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                var target = Target.Player();
                spell.AddCost(target);

                spell.AddCost(PayMana("3"));

                spell.Effect = s =>
                {
                    Player player = s.Resolve(target);
                    var top3Cards = player.Library.Top(3);
                    player.Graveyard.MoveToTop(top3Cards);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<MillAbility>(card);
        }
    }

    [CardFactory("Arcanis the Omnipotent")]
    public class ArcanisTheOmnipotentCardFactory : CardFactory
    {
        // T Draw three cards.
        private class DrawAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));

                spell.Effect = s =>
                {
                    s.Controller.DrawCards(3);
                };
            }
        }

        // 2UU: Return Arcanis the Omnipotent to its owner's hand.
        private class ReturnToHandAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2UU"));

                spell.Effect = s =>
                {
                    s.Source.ReturnToHand();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<DrawAbility>(card);
            CreateAbility<ReturnToHandAbility>(card);
        }
    }

    [CardFactory("Denizen of the Deep")]
    public class DenizenOfTheDeepCardFactory : CardFactory
    {
        [AbilityText(Text = "When Denizen of the Deep comes into play, return each other creature you control to its owner's hand.")]
        private class DenizenAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
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
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DenizenAbility>(card);
        }
    }

    [CardFactory("Horseshoe Crab")]
    public class HorseshoeCrabCardFactory : CardFactory
    {
        #region Abilities

        // U: Untap Horseshoe Crab.
        private class UntapAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("U"));

                spell.Effect = s =>
                {
                    s.Source.Untap();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<UntapAbility>(card);
        }
    }

    [CardFactory("Rootwater Commando")]
    public class RootwaterCommandoCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Island;
        }
    }

    [CardFactory("Sky Weaver")]
    public class SkyWeaverCardFactory : CardFactory
    {
        // 2: Target white or black creature gains flying until end of turn.
        private class GainFlyingAbility : InPlayAbility
        {
            private Color m_color = Color.White | Color.Black;
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<GainFlyingAbility>("Color", a => a.m_color, PropertyFlags.Private);

            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2"));
                var target = Target.Creature().OfAnyColor(m_color);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).GainAbility<FlyingAbility>().UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<GainFlyingAbility>(card);
        }
    }

    [CardFactory("Vedalken Mastermind")]
    public class VedalkenMastermindCardFactory : CardFactory
    {
        #region Abilities

        // U, T Return target permanent you control to its owner's hand.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("U"));

                spell.AddCost(Tap(spell.Source));

                var target = Target.Permanent().UnderControl(spell.Controller);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).ReturnToHand();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    #endregion

    #region Green

    [CardFactory("Elven Riders")]
    public class ElvenRidersCardFactory : CardFactory
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

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<BlockAbility>(card);
        }
    }

    [CardFactory("Femeref Archers")]
    public class FemerefArchersCardFactory : CardFactory
    {
        // T Femeref Archers deals 4 damage to target attacking creature with flying.
        private class DamageAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));

                var target = Target.Creature().Attacking().With<FlyingAbility>();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(4);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DamageAbility>(card);
        }
    }

    [CardFactory("Kavu Climber")]
    public class KavuClimberCardFactory : CardFactory
    {
        [AbilityText(Text = "When Kavu Climber comes into play, draw a card.")]
        private class DrawAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    s.Controller.DrawCards(1);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Llanowar Elves")]
    public class LlanowarElvesCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            var tapAbility = CreateAbility<TapForManaAbility>(card);
            tapAbility.Color = Color.Green;
        }
    }

    [CardFactory("Mirri, Cat Warrior")]
    public class MirriCatWarriorCardFactory : FirstStrikeCreatureFactory10E
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Forest;
            CreateAbility<VigilanceAbility>(card);
        }
    }

    [CardFactory("Rushwood Dryad")]
    public class RushwoodDryadCardFactory : CardFactory
    {
        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            var ability = CreateAbility<BasicLandWalkAbility>(card);
            ability.Type = SubType.Forest;
        }
    }

    [CardFactory("Viridian Shaman")]
    public class ViridianShamanCardFactory : CardFactory
    {
        [AbilityText(Text = "When Viridian Shaman comes into play, destroy target artifact.")]
        private class DestroyAbility : ThisCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                var target = Target.Card().OfAnyType(Type.Artifact);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Destroy();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<DestroyAbility>(card);
        }
    }

    #endregion

    */
}
