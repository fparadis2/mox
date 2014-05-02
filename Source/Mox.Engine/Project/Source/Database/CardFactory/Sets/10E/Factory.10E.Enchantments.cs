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

namespace Mox.Database.Sets
{
    #region Enchantments

    #region White

    [CardFactory("Angelic Chorus")]
    public class AngelicChorusCardFactory : MTGCardFactory
    {
        #region Inner Types

        // Whenever a creature comes into play under your control, you gain life equal to its toughness.
        private class GainLifeAbility : AnyCreatureComesIntoPlayUnderControlAbility
        {
            protected override void Play(Spell spell, Resolvable<Card> card)
            {
                spell.Effect = s =>
                {
                    Card resolvedCard = card.Resolve(s.Game);
                    s.Controller.GainLife(resolvedCard.Toughness);
                };
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<GainLifeAbility>(card);
        }

        #endregion
    }

    [CardFactory("Glorious Anthem")]
    public class GloriousAnthemCardFactory : MTGCardFactory
    {
        // Creatures you control get +1/+1.
        private class BoostAbility : ContinuousAbility
        {
            protected override IEnumerable<IEffectCreator> AddEffects()
            {
                yield return AddEffect.OnCreatures(Manager, Condition.ControlledBySameController(Source)).ModifyPowerAndToughness(+1, +1);
            }
        }

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }

        #endregion
    }

    [CardFactory("Holy Strength")]
    public class HolyStrengthCardFactory : MTGCardFactory
    {
        #region Inner Types

        private class BoostAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.ModifyPowerAndToughness(+1, +2);
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    [CardFactory("Megrim")]
    public class MegrimCardFactory : MTGCardFactory
    {
        #region Inner Types

        // Whenever an opponent discards a card from his or her hand, Megrim deals 2 damage to that player. 
        private class DamageAbility : TriggeredAbility, IEventHandler<Events.PlayerDiscardedEvent>
        {
            public override void Play(Spell spell)
            {
                // Deal damage
                Resolvable<Player> player = (Resolvable<Player>)spell.Context;

                spell.Effect = s =>
                {
                    player.Resolve(s.Game).DealDamage(2);
                };
            }

            public void HandleEvent(Game game, Events.PlayerDiscardedEvent e)
            {
                Resolvable<Player> player = e.Player;
                if (e.Player != Controller)
                {
                    Trigger(player);
                }
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    [CardFactory("Pacifism")]
    public class PacifismCardFactory : MTGCardFactory
    {
        #region Inner Types

        private class PacifismAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.GainAbility<CannotAttackAbility>();
                yield return cardEffectHost.GainAbility<CannotBlockAbility>();
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<PacifismAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    [CardFactory("Serra's Embrace")]
    public class SerrasEmbraceCardFactory : MTGCardFactory
    {
        #region Inner Types

        private class SerrasEmbraceAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.ModifyPowerAndToughness(+2, +2);
                yield return cardEffectHost.GainAbility<FlyingAbility>();
                yield return cardEffectHost.GainAbility<VigilanceAbility>();
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<SerrasEmbraceAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Blue

    [CardFactory("Persuasion")]
    public class PersuasionCardFactory : MTGCardFactory
    {
        #region Inner Types

        private class PersuasionAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.GainControl(Controller);
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<PersuasionAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    [CardFactory("Shimmering Wings")]
    public class ShimmeringWingsCardFactory : MTGCardFactory
    {
        #region Inner Types

        // Enchanted creature has flying. (It can't be blocked except by creatures with flying or reach.)
        private class WingsAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.GainAbility<FlyingAbility>();
            }
        }

        // U: Return Shimmering Wings to its owner's hand.
        private class ReturnAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("U"));

                spell.Effect = s =>
                {
                    s.Source.ReturnToHand();
                };
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<ReturnAbility>(card);
            CreateAbility<WingsAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Black

    [CardFactory("Underworld Dreams")]
    public class UnderworldDreamsCardFactory : MTGCardFactory
    {
        #region Inner Types

        // Whenever an opponent draws a card, Underworld Dreams deals 1 damage to him or her.
        private class DamageAbility : TriggeredAbility, IEventHandler<Events.DrawCardEvent>
        {
            public override void Play(Spell spell)
            {
                // Deal damage
                Resolvable<Player> player = (Resolvable<Player>)spell.Context;

                spell.Effect = s =>
                {
                    player.Resolve(s.Game).DealDamage(1);
                };
            }

            public void HandleEvent(Game game, Events.DrawCardEvent e)
            {
                Resolvable<Player> player = e.Player;
                if (e.Player != Controller)
                {
                    Trigger(player);
                }
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);

            CreateAbility<DamageAbility>(card);
        }

        #endregion
    }

    [CardFactory("Unholy Strength")]
    public class UnholyStrengthCardFactory : MTGCardFactory
    {
        #region Inner Types

        private class BoostAttachmentAbility : AttachmentAbility
        {
            protected override IEnumerable<IEffectCreator> Attach(ILocalEffectHost<Card> cardEffectHost)
            {
                yield return cardEffectHost.ModifyPowerAndToughness(+2, +1);
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAttachmentAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    #endregion

    #region Red

    [CardFactory("Firebreathing")]
    public class FirebreathingCardFactory : MTGCardFactory
    {
        #region Inner Types

        // R: Enchanted creature gets +1/+0 until end of turn.
        private class BoostAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(PayMana("R"));

                spell.Effect = s =>
                {
                    AddEffect.On(s.Source.AttachedTo).ModifyPowerAndToughness(+1, +0).UntilEndOfTurn();
                };
            }
        }

        #endregion

        #region Methods

        protected override void Initialize(Card card, InitializationContext context)
        {
            base.Initialize(card, context);
            CreateAbility<BoostAbility>(card);
        }

        protected override PlayCardAbility CreatePlayCardAbility(Card card)
        {
            return CreateAbility<EnchantAbility>(card);
        }

        #endregion
    }

    #endregion

    #endregion
}
