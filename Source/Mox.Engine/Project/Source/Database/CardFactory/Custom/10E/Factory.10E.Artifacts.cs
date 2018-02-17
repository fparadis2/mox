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
using Mox.Abilities;
using Mox.Flow;

namespace Mox.Database.Sets
{
#warning todo spell_v2
    /*
    #region Helpers

    // Whenever a player plays a COLOR spell, you may gain 1 life.
    internal class GainLifeWhenSpellPlayedCardFactory : SpellPlayedTriggeredAbility
    {
        private Color m_color;
        private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty<GainLifeWhenSpellPlayedCardFactory>("Color", f => f.m_color, PropertyFlags.Private);

        public override string AbilityText
        {
            get { return string.Format("Whenever a player plays a {0} spell, you may gain 1 life.", Color.ToString().ToLower()); }
        }

        public Color Color
        {
            get { return m_color; }
            set { SetValue(ColorProperty, value, ref m_color); }
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
    public class AngelsFeatherCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Whenever a player plays a white spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.White;
        }

        #endregion
    }

    [CardFactory("Demon's Horn")]
    public class DemonsHornCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Whenever a player plays a black spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Black;
        }

        #endregion
    }

    [CardFactory("Doubling Cube")]
    public class DoublingCubeCardFactory : CardFactory
    {
        #region Abilities

        // 3, T Double the amount of each type of mana in your mana pool.
        private class ManaAbility : InPlayAbility
        {
            public override bool IsManaAbility => true;

            public override void FillManaOutcome(IManaAbilityOutcome outcome)
            {
                outcome.AddAny();
            }

            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("3"));
                spell.AddCost(Tap(spell.Source));

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

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<ManaAbility>(card);
        }
    }

    [CardFactory("Dragon's Claw")]
    public class DragonsClawCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Whenever a player plays a red spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Red;
        }

        #endregion
    }

    [CardFactory("Icy Manipulator")]
    public class IcyManipulatorCardFactory : CardFactory
    {
        #region Abilities

        // 1, T Tap target artifact, creature, or land.
        private class TapAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("1"));
                spell.AddCost(Tap(spell.Source));

                TargetCost<Card> target = Target.Card().OfAnyType(Type.Artifact | Type.Creature | Type.Land);
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    s.Resolve(target).Tap();
                };
            }
        }

        #endregion

        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            CreateAbility<TapAbility>(card);
        }

        #endregion
    }

    [CardFactory("Kraken's Eye")]
    public class KrakensEyeCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Whenever a player plays a blue spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Blue;
        }

        #endregion
    }

    [CardFactory("Mantis Engine")]
    public class MantisEngineCardFactory : CardFactory
    {
        #region Abilities

        // 2: Mantis Engine gains flying until end of turn.
        private class GainFlyingAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2"));

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
                spell.AddCost(PayMana("2"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source).GainAbility<FirstStrikeAbility>().UntilEndOfTurn();
                };
            }
        }

        #endregion

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<GainFlyingAbility>(card);
            CreateAbility<GainFirstStrikeAbility>(card);
        }
    }

    [CardFactory("Millstone")]
    public class MillstoneCardFactory : CardFactory
    {
        // 2, T Target player puts the top two cards of his or her library into his or her graveyard.
        private class MillAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("2"));
                spell.AddCost(Tap(spell.Source));

                var target = Target.Player();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    Player player = s.Resolve(target);
                    var top2Cards = player.Library.Top(2);
                    player.Graveyard.MoveToTop(top2Cards);
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<MillAbility>(card);
        }
    }

    [CardFactory("Wurm's Tooth")]
    public class WurmsToothCardFactory : CardFactory
    {
        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            // Whenever a player plays a green spell, you may gain 1 life.
            var ability = CreateAbility<GainLifeWhenSpellPlayedCardFactory>(card);
            ability.Color = Color.Green;
        }

        #endregion
    }

    #endregion
    */
}
