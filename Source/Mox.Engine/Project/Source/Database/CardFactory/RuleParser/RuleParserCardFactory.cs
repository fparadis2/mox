using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database
{
    public class RuleParserCardFactory : BaseCardFactory, ICardFactory
    {
        public CardFactoryResult InitializeCard(Card card, CardInfo cardInfo)
        {
            var parser = CreateRuleParser(cardInfo);

            if (parser == null)
                return CardFactoryResult.NotImplemented($"Card {card.Name} is not supported by rule parser");

            if (parser.UnknownFragments.Count > 0)
                return CardFactoryResult.NotImplemented($"Card {card.Name} has unknown fragment: {parser.UnknownFragments[0]}");

            InitializeFromDatabase(card, cardInfo);

            parser.Initialize(card);

            return CardFactoryResult.Success;
        }

        public RuleParser CreateRuleParser(CardInfo cardInfo)
        {
            if (cardInfo.SuperType.Is(SuperType.Basic))
                return null;

            // Todo: possibly cache
            RuleParser parser = new RuleParser();
            parser.Parse(cardInfo);
            return parser;
        }
    }
}
