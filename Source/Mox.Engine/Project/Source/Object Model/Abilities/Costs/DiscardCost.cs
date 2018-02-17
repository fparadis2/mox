using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    public class DiscardCost : Cost
    {
        public DiscardCost(AmountResolver count, Filter filter, bool atRandom)
        {
            Debug.Assert(filter.FilterType == FilterType.Hand);

            Count = count;
            Filter = filter;
            AtRandom = atRandom;
        }

        public AmountResolver Count { get; }
        public Filter Filter { get; }
        public bool AtRandom { get; }

        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            return true;
        }

        public override void Execute(Part.Context context, Spell2 spell)
        {
            int amount = Count.Resolve(spell);
            Debug.Assert(amount >= 0, "Count should be positive");

            List<GameObject> cards = new List<GameObject>();
            Filter.EnumerateObjects(context.Game, spell.Controller, cards);

            if (amount > cards.Count)
            {
                PushResult(context, false);
                return;
            }

            if (AtRandom)
            {
                context.Schedule(new RandomDiscardPart(spell.Controller, Filter, amount));
            }
            else
            {
                context.Schedule(new MainDiscardPart(spell.Controller, Filter, 0, amount));
            }
        }

        private class MainDiscardPart : PlayerPart
        {
            private readonly Filter m_filter;
            private readonly int m_cardIndex;
            private readonly int m_numCards;

            public MainDiscardPart(Resolvable<Player> player, Filter filter, int cardIndex, int numCards)
                : base(player)
            {
                m_filter = filter;
                m_cardIndex = cardIndex;
                m_numCards = numCards;
            }

            public override Part Execute(Context context)
            {
                if (m_cardIndex > 0)
                {
                    var result = PopResult(context);
                    if (!result)
                    {
                        PushResult(context, result);
                        return null;
                    }
                }

                if (m_cardIndex >= m_numCards)
                {
                    PushResult(context, true); // Done
                    return null;
                }

                List<GameObject> cards = new List<GameObject>();
                m_filter.EnumerateObjects(context.Game, GetPlayer(context), cards);
                var targets = cards.Select(c => c.Identifier).ToArray();

                var targetContext = new TargetContext(true, targets, TargetContextType.Discard);
                context.Schedule(new DiscardPart(ResolvablePlayer, targetContext));
                return new MainDiscardPart(ResolvablePlayer, m_filter, m_cardIndex + 1, m_numCards);
            }
        }

        private class DiscardPart : ChoicePart<TargetResult>
        {
            private readonly TargetContext m_context;

            public DiscardPart(Resolvable<Player> player, TargetContext context)
                : base(player)
            {
                m_context = context;
            }

            public override Choice GetChoice(Sequencer sequencer)
            {
                Player player = GetPlayer(sequencer.Game);
                return new TargetChoice(player, m_context);
            }

            public override Part Execute(Context context, TargetResult targetToDiscard)
            {
                if (!targetToDiscard.IsValid)
                {
                    PushResult(context, false);
                    return null;
                }

                if (!m_context.IsValid(targetToDiscard))
                {
                    return this;
                }

                Player player = GetPlayer(context);

                Card card = targetToDiscard.Resolve<Card>(context.Game);
                Debug.Assert(player.Hand.Contains(card));
                player.Discard(card);

                PushResult(context, true);
                return null;
            }
        }

        private class RandomDiscardPart : PlayerPart
        {
            private readonly int m_amount;
            private readonly Filter m_filter;

            public RandomDiscardPart(Player player, Filter filter, int amount) 
                : base(player)
            {
                m_filter = filter;
                m_amount = amount;
            }

            public override Part Execute(Context context)
            {
                var player = GetPlayer(context);

                for (int i = 0; i < m_amount; i++)
                {
                    List<GameObject> cards = new List<GameObject>();
                    m_filter.EnumerateObjects(context.Game, player, cards);

                    var card = (Card)context.Game.Random.Choose(cards);
                    player.Discard(card);
                }

                PushResult(context, true);
                return null;
            }
        }
    }

    public class DiscardHandCost : Cost
    {
        public override bool CanExecute(Ability ability, AbilityEvaluationContext evaluationContext)
        {
            return true;
        }

        public override void Execute(Part.Context context, Spell2 spell)
        {
            context.Schedule(new DiscardHandPart(spell.Controller));
            PushResult(context, true); // Always ok to discard hand
        }

        private class DiscardHandPart : Part
        {
            private readonly Resolvable<Player> m_player;

            public DiscardHandPart(Resolvable<Player> player)
            {
                m_player = player;
            }

            public override Part Execute(Context context)
            {
                var player = m_player.Resolve(context.Game);
                if (player != null)
                {
                    while (player.Hand.Count > 0)
                    {
                        player.Discard(player.Hand[0]);
                    }
                }

                return null;
            }
        }
    }
}
