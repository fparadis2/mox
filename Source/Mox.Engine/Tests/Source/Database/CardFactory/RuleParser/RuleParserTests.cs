using NUnit.Framework;

namespace Mox.Database
{
    [TestFixture]
    public class RuleParserTests : BaseGameTests
    {
        #region Utilities

        private void ParsesWithoutError(string text)
        {
            RuleParser parser = new RuleParser();
            Assert.That(parser.Parse(text));
        }

        private void DoesntParse(string text)
        {
            RuleParser parser = new RuleParser();
            Assert.IsFalse(parser.Parse(text));
        }

        private Card CreateCard(string text)
        {
            var database = new CardDatabase();
            var cardInfo = database.AddCard("Potato", "R", Color.Red, SuperType.None, Type.Creature, null, "1", "1", text);

            var card = CreateCard(m_playerA, cardInfo.Name);

            RuleParserCardFactory factory = new RuleParserCardFactory();
            var result = factory.InitializeCard(card, cardInfo);
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

        [Test]
        public void Test_Functional_parsing_tests()
        {
            ParsesWithoutError("Flying");
            ParsesWithoutError("Flying, Reach");
        }

        #endregion
    }
}
