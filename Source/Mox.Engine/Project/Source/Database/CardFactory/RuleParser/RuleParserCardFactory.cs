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
        private readonly RuleParserResult? m_creator;

        private RuleParserCardFactory(CardInfo cardInfo, RuleParserResult? creator)
        {
            CardInfo = cardInfo;
            m_creator = creator;
        }

        public CardFactoryResult InitializeCard(Card card)
        {
            if (m_creator == null)
                return CardFactoryResult.NotImplemented($"Card {card.Name} is not supported by the rule parser.");

            var creator = m_creator.Value;
            if (creator.UnknownFragments.Count > 0)
                return CardFactoryResult.NotImplemented($"Card {card.Name} has unknown fragment: {creator.UnknownFragments[0]}");

            InitializeFromDatabase(card);

            creator.Initialize(card);
            return CardFactoryResult.Success;
        }

        public static ICardFactory Create(CardInfo cardInfo)
        {
            return new RuleParserCardFactory(cardInfo, CreateParser(cardInfo));
        }

        public static RuleParserResult? CreateParser(CardInfo cardInfo)
        {
            if (cardInfo.SuperType.HasFlag(SuperType.Basic | SuperType.Snow))
                return null;

            RuleParser parser = new RuleParser(cardInfo.Name);
            return parser.Parse(cardInfo);
        }
    }
}
