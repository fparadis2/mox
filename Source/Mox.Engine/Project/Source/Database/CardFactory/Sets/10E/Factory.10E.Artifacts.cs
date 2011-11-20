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
using Mox.Database.Library;
using Mox.Flow;

namespace Mox.Database.Sets
{
    #region Helpers

    // Whenever a player plays a COLOR spell, you may gain 1 life.
    internal class GainLifeWhenSpellPlayedCardFactory : SpellPlayedTriggeredAbility
    {
        private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof (GainLifeWhenSpellPlayedCardFactory), PropertyFlags.Private);

        public Color Color
        {
            get { return GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        public override void Play(Spell spell)
        {
            spell.EffectPart = new GainLifeChoicePart();
        }

        protected override sealed bool TriggersOn(Spell spell)
        {
            return spell.Source.Is(Color);
        }

        private class GainLifeChoicePart : SpellEffectModalChoicePart
        {
            public GainLifeChoicePart()
                : base(ModalChoiceContext.YesNo("Gain 1 life?", ModalChoiceResult.Yes, ModalChoiceImportance.Trivial))
            {
            }

            protected override Part Execute(Context context, ModalChoiceResult result, Spell spell)
            {
                if (result == ModalChoiceResult.Yes)
                {
                    spell.Controller.GainLife(1);
                }
                return null;
            }
        }
    }

    #endregion

    #region Artifacts

    [CardFactory("Angel's Feather")]
    public class AngelsFeatherCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Whenever a player plays a white spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.White;
        }

        #endregion
    }

    [CardFactory("Bottle Gnomes")]
    public class BottleGnomesCardFactory : MTGCardFactory
    {
        #region Abilities

        // Sacrifice Bottle Gnomes: You gain 3 life.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(Sacrifice(spell.Source));

                spell.Effect = s =>
                {
                    s.Controller.GainLife(3);
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

    [CardFactory("Composite Golem")]
    public class CompositeGolemCardFactory : MTGCardFactory
    {
        #region Abilities

        // Sacrifice Composite Golem: Add WUBRG to your mana pool.
        private class SacrificeAbility : InPlayAbility
        {
            public override ManaAbilityOutcome ManaOutcome
            {
                get
                {
                    return ManaAbilityOutcome.Any;
                }
            }

            public override void Play(Spell spell)
            {
                spell.Costs.Add(Sacrifice(spell.Source));

                spell.Effect = s =>
                {
                    var manapool = s.Controller.ManaPool;
                    manapool[Color.White] += 1;
                    manapool[Color.Blue] += 1;
                    manapool[Color.Black] += 1;
                    manapool[Color.Red] += 1;
                    manapool[Color.Green] += 1;
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

    [CardFactory("Demon's Horn")]
    public class DemonsHornCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Whenever a player plays a black spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Black;
        }

        #endregion
    }

    [CardFactory("Doubling Cube")]
    public class DoublingCubeCardFactory : MTGCardFactory
    {
        #region Abilities

        // 3, T Double the amount of each type of mana in your mana pool.
        private class ManaAbility : InPlayAbility
        {
            public override ManaAbilityOutcome ManaOutcome
            {
                get
                {
                    return ManaAbilityOutcome.Any;
                }
            }

            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("3"));
                spell.Costs.Add(Tap(spell.Source));

                spell.Effect = s =>
                {
                    var manapool = s.Controller.ManaPool;

                    foreach (Color color in Enum.GetValues(typeof(Color)))
                    {
                        manapool[color] *= 2;
                    }
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<ManaAbility>(card);
        }
    }

    [CardFactory("Dragon's Claw")]
    public class DragonsClawCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Whenever a player plays a red spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Red;
        }

        #endregion
    }

    [CardFactory("Fountain of Youth")]
    public class FountainOfYouthCardFactory : MTGCardFactory
    {
        #region Abilities

        // 2, T You gain 1 life.
        private class GainLifeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("2"));
                spell.Costs.Add(Tap(spell.Source));

                spell.Effect = s =>
                {
                    s.Controller.GainLife(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<GainLifeAbility>(card);
        }
    }

    [CardFactory("Icy Manipulator")]
    public class IcyManipulatorCardFactory : MTGCardFactory
    {
        #region Abilities

        // 1, T Tap target artifact, creature, or land.
        private class TapAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("1"));
                spell.Costs.Add(Tap(spell.Source));

                TargetCost<Card> target = Target.Card().OfAnyType(Type.Artifact | Type.Creature | Type.Land);
                spell.Costs.Add(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Tap();
                };
            }
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Jayemdae Tome")]
    public class JayemdaeTomeCardFactory : MTGCardFactory
    {
        #region Abilities

        // 4, T Draw a card.
        private class DrawAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("4"));
                spell.Costs.Add(Tap(spell.Source));

                spell.Effect = s =>
                {
                    s.Controller.DrawCards(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<DrawAbility>(card);
        }
    }

    [CardFactory("Kraken's Eye")]
    public class KrakensEyeCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Whenever a player plays a blue spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Blue;
        }

        #endregion
    }

    [CardFactory("Leonin Scimitar")]
    public class LeoninScimitarCardFactory : MTGCardFactory
    {
        // Equipped creature gets +1/+1.
        private class BoostAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.ModifyPowerAndToughness(+1, +1);
            }
        }

        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
            CreateAbility<EquipAbility>(card).ManaCost = ManaCost.Parse("1");
        }

        #endregion
    }

    [CardFactory("Mantis Engine")]
    public class MantisEngineCardFactory : MTGCardFactory
    {
        #region Abilities

        // 2: Mantis Engine gains flying until end of turn.
        private class GainFlyingAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("2"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).GainAbility<FlyingAbility>().UntilEndOfTurn();
                };
            }
        }

        // 2: Mantis Engine gains first strike until end of turn.
        private class GainFirstStrikeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("2"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).GainAbility<FirstStrikeAbility>().UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<GainFlyingAbility>(card);
            CreateAbility<GainFirstStrikeAbility>(card);
        }
    }

    [CardFactory("Millstone")]
    public class MillstoneCardFactory : MTGCardFactory
    {
        // 2, T Target player puts the top two cards of his or her library into his or her graveyard.
        private class MillAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("2"));
                spell.Costs.Add(Tap(spell.Source));

                var target = Target.Player();
                spell.Costs.Add(target);

                spell.Effect = s =>
                {
                    Player player = s.Resolve(target);
                    var top2Cards = player.Library.Top(2);
                    player.Graveyard.MoveToTop(top2Cards);
                };
            }
        }

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<MillAbility>(card);
        }
    }

    [CardFactory("Mind Stone")]
    public class MindStoneCardFactory : MTGCardFactory
    {
        #region Abilities

        // 1, T, Sacrifice Mind Stone: Draw a card.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("1"));
                spell.Costs.Add(Tap(spell.Source));
                spell.Costs.Add(Sacrifice(spell.Source));

                spell.Effect = s =>
                {
                    s.Controller.DrawCards(1);
                };
            }
        }

        #endregion

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            
            var manaAbility = CreateAbility<TapForManaAbility>(card);
            manaAbility.Color = Color.None;

            CreateAbility<SacrificeAbility>(card);
        }
    }

    [CardFactory("Phyrexian Vault")]
    public class PhyrexianVaultCardFactory : MTGCardFactory
    {
        #region Abilities

        // 2, T, Sacrifice a creature: Draw a card.
        private class TapAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(Tap(spell.Source));
                spell.Costs.Add(Target.Creature().Sacrifice());
                spell.Costs.Add(PayMana("2"));

                spell.Effect = s =>
                {
                    s.Controller.DrawCards(1);
                };
            }
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Rod of Ruin")]
    public class RodOfRuinCardFactory : MTGCardFactory
    {
        #region Abilities

        // 3, T Rod of Ruin deals 1 damage to target creature or player.
        private class TapAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.Costs.Add(PayMana("3"));
                spell.Costs.Add(Tap(spell.Source));

                var target = Target.Creature() | Target.Player();
                spell.Costs.Add(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).DealDamage(1);
                };
            }
        }

        #endregion

        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Wurm's Tooth")]
    public class WurmsToothCardFactory : MTGCardFactory
    {
        #region Overrides of MTGCardFactory

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            // Whenever a player plays a green spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Green;
        }

        #endregion
    }

    #endregion
}
