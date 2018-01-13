using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database
{
    public class RuleParserCardFactory : BaseCardFactory, ICardFactory
    {
        private readonly RuleParser m_parser;

        public RuleParserCardFactory(CardInfo cardInfo, RuleParser parser)
        {
            CardInfo = cardInfo;
            m_parser = parser;
        }

        public CardFactoryResult InitializeCard(Card card)
        {
            if (m_parser == null)
                return CardFactoryResult.NotImplemented($"Card {card.Name} is not supported by the rule parser.");

            if (m_parser.UnknownFragments.Count > 0)
                return CardFactoryResult.NotImplemented($"Card {card.Name} has unknown fragment: {m_parser.UnknownFragments[0]}");

            InitializeFromDatabase(card);

            m_parser.Initialize(card);
            return CardFactoryResult.Success;
        }

        public static ICardFactory Create(CardInfo cardInfo)
        {
#warning todo spell_v2 probably don't need to keep the whole parser around?
            return new RuleParserCardFactory(cardInfo, CreateParser(cardInfo));
        }

        public static RuleParser CreateParser(CardInfo cardInfo)
        {
            if (cardInfo.SuperType.HasFlag(SuperType.Basic | SuperType.Snow))
                return null;

            RuleParser parser = new RuleParser();
            parser.Parse(cardInfo);
            return parser;
        }
    }
}
