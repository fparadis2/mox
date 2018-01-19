using NUnit.Framework;
using System.Linq;

using Mox.Abilities;

namespace Mox.Database
{
    [TestFixture]
    public class RuleParserTests : BaseGameTests
    {
        #region Utilities

        private void ParsesWithoutError(string text)
        {
            RuleParser parser = new RuleParser("The Source");
            Assert.That(parser.Parse(text).IsValid);
        }

        private void DoesntParse(string text)
        {
            RuleParser parser = new RuleParser("The Source");
            Assert.IsFalse(parser.Parse(text).IsValid);
        }

        private Card CreateCard(string text, Type type = Type.Creature, string cost = "R")
        {
            var database = new CardDatabase();
            var cardInfo = database.AddCard("Potato", cost, Color.Red, SuperType.None, type, null, "1", "1", text);

            var card = CreateCard(m_playerA, cardInfo.Name);

            var factory = RuleParserCardFactory.Create(cardInfo);
            var result = factory.InitializeCard(card);
            Assert.That(result.Type == CardFactoryResult.ResultType.Success);

            return card;
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Invalid_abilities_cannot_be_parsed()
        {
            DoesntParse("asdyaosdiausgdiaubs");
            DoesntParse("Flying dutchman");
            DoesntParse("Reaching");
            DoesntParse("Breach");
        }

        [Test]
        public void Test_Static_abilities_can_be_parsed()
        {
            var card = CreateCard("Flying, Reach");
            Assert.That(card.HasAbility<FlyingAbility>());
        }

        [Test]
        public void Test_Parsing_is_not_case_sensitive()
        {
            var card = CreateCard("flying");
            Assert.That(card.HasAbility<FlyingAbility>());
        }

        [Test]
        public void Test_Reminder_text_is_ignored()
        {
            var card = CreateCard("flying (reminder)");
            Assert.That(card.HasAbility<FlyingAbility>());

            ParsesWithoutError("(Only reminder text)");
        }

        [Test]
        public void Test_Static_abilities_can_be_separated_by_newlines()
        {
            var card = CreateCard("Flying\nReach");
            Assert.That(card.HasAbility<FlyingAbility>());
            Assert.That(card.HasAbility<ReachAbility>());
        }

        [Test]
        public void Test_Static_abilities_can_be_separated_with_commas_and_empty_space()
        {
            var card = CreateCard("Flying, Reach");
            Assert.That(card.HasAbility<FlyingAbility>());
            Assert.That(card.HasAbility<ReachAbility>());

            card = CreateCard("   Flying ,Reach   ");
            Assert.That(card.HasAbility<FlyingAbility>());
            Assert.That(card.HasAbility<ReachAbility>());
        }

        private void Test_A_PlayCardAbility_is_created_for_every_card(Type type)
        {
            var card = CreateCard(string.Empty, type, "R");
            var playCardAbility = card.Abilities.OfType<PlayCardAbility>().Single();

            var payManaCost = playCardAbility.SpellDefinition.Costs.OfType<PayManaCost>().Single();
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R), payManaCost.ManaCost);
        }

        [Test]
        public void Test_A_PlayCardAbility_is_created_for_every_card()
        {
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Creature);
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Enchantment);
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Artifact);
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Instant);
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Sorcery);
            Test_A_PlayCardAbility_is_created_for_every_card(Type.Land);
        }

        [Test]
        public void Test_Functional_parsing_tests()
        {
            // Static
            ParsesWithoutError("Flying");
            ParsesWithoutError("Flying, Reach");

            // Activated
            ParsesWithoutError("{T}: Add {W} to your mana pool.");
        }

        #endregion
    }
}
