using Mox.Flow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class DiscardAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly AmountResolver m_count;

        public DiscardAction(ObjectResolver targets, AmountResolver count)
        {
            Throw.IfNull(targets, "targets");

            m_targets = targets;
            m_count = count;
        }

        public ObjectResolver Targets => m_targets;
        public AmountResolver Count => m_count;

        public override Part ResolvePart(Spell2 spell)
        {
            int amount = m_count.Resolve(spell);
            Debug.Assert(amount >= 0, "Count should be positive");

            List<Resolvable<Player>> players = new List<Resolvable<Player>>();

            foreach (var player in m_targets.Resolve<Player>(spell))
            {
                players.Add(player);
            }

            return new MainDiscardPart(players.ToArray(), amount);
        }

        private class MainDiscardPart : Part
        {
            private readonly Resolvable<Player>[] m_players;
            private readonly int m_numCards;

            public MainDiscardPart(Resolvable<Player>[] players, int numCards)
            {
                m_players = players;
                m_numCards = numCards;
            }

            public override Part Execute(Context context)
            {
                foreach (var player in m_players)
                {
                    var resolvedPlayer = player.Resolve(context.Game);
                    if (resolvedPlayer.Hand.Count <= m_numCards)
                    {
                        context.Schedule(new DiscardHandPart(player));
                    }
                    else
                    {
                        for (int i = 0; i < m_numCards; i++)
                        {
                            context.Schedule(new DiscardPart(player));
                        }
                    }
                }

                return null;
            }
        }

        private class DiscardPart : ChoicePart<TargetResult>
        {
            public DiscardPart(Resolvable<Player> player)
                : base(player)
            {}

            public override Choice GetChoice(Sequencer sequencer)
            {
                Player player = GetPlayer(sequencer.Game);
                int[] targets = player.Hand.Select(card => card.Identifier).ToArray();
                TargetContext targetInfo = new TargetContext(false, targets, TargetContextType.Discard);
                return new TargetChoice(player, targetInfo);
            }

            public override Part Execute(Context context, TargetResult targetToDiscard)
            {
                Player player = GetPlayer(context);

                if (targetToDiscard.IsValid)
                {
                    Card card = targetToDiscard.Resolve<Card>(context.Game);
                    if (player.Hand.Contains(card))
                    {
                        player.Discard(card);
                        return null;
                    }
                }

                // retry
                return this;
            }
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

    public class DiscardAtRandomAction : Action
    {
        private readonly ObjectResolver m_targets;
        private readonly AmountResolver m_count;

        public DiscardAtRandomAction(ObjectResolver targets, AmountResolver count)
        {
            Throw.IfNull(targets, "targets");

            m_targets = targets;
            m_count = count;
        }

        public ObjectResolver Targets => m_targets;
        public AmountResolver Count => m_count;

        protected override void Resolve(Spell2 spell)
        {
            base.Resolve(spell);

            int amount = m_count.Resolve(spell);
            Debug.Assert(amount >= 0, "Count should be positive");

            foreach (var player in m_targets.Resolve<Player>(spell))
            {
                for (int i = 0; i < amount; i++)
                {
                    var card = spell.Manager.Random.Choose(player.Hand);
                    player.Discard(card);
                }
            }
        }
    }
}
