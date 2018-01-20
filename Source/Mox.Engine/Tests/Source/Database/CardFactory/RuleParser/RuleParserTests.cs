using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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

        private IEnumerable<string> GetUnknownFragments(string text)
        {
            RuleParser parser = new RuleParser("The Source");
            var result = parser.Parse(text);
            Assert.IsFalse(result.IsValid);
            return result.UnknownFragments;
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

        #region General

        [Test]
        public void Test_Invalid_abilities_cannot_be_parsed()
        {
            DoesntParse("asdyaosdiausgdiaubs");
            DoesntParse("Flying dutchman");
            DoesntParse("Reaching");
            DoesntParse("Breach");
        }

        [Test]
        public void Test_Parsing_is_not_case_sensitive()
        {
            var card = CreateCard("flying");
            Assert.That(card.HasAbility<FlyingAbility>());
        }

        [Test]
        public void Test_Unknown_rules_are_logged_as_unknown_fragments()
        {
            var unknownFragments = GetUnknownFragments("Unknown rule");
            Assert.Collections.AreEqual(new[] { "[Rule] Unknown rule" }, unknownFragments);
        }

        [Test]
        public void Test_Reminder_text_is_ignored()
        {
            var card = CreateCard("flying (reminder)");
            Assert.That(card.HasAbility<FlyingAbility>());

            ParsesWithoutError("(Only reminder text)");
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

        #region Static Abilities

        [Test]
        public void Test_Static_abilities_can_be_parsed()
        {
            var card = CreateCard("Flying, Reach");
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
        public void Test_Unknown_static_abilities_are_logged_as_unknown_fragments()
        {
            var unknownFragments = GetUnknownFragments("Flying, xyz");
            Assert.Collections.AreEqual(new[] { "[Rule] xyz" }, unknownFragments);
        }

        #endregion

        #region PlayCardAbility

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

        #endregion

        #region Activated Abilities

        [Test]
        public void Test_Unknown_costs_are_logged_as_unknown_fragments()
        {
            var unknownFragments = GetUnknownFragments("Unknown: Add {W} to your mana pool.");
            Assert.Collections.AreEqual(new[] { "[Cost] Unknown" }, unknownFragments);
        }

        [Test]
        public void Test_Unknown_effects_are_logged_as_unknown_fragments()
        {
            var unknownFragments = GetUnknownFragments("{T}: Unknown");
            Assert.Collections.AreEqual(new[] { "[Effect] Unknown" }, unknownFragments);
        }

        [Test]
        public void Test_Invalid_parts_in_effects_can_also_log_unknown_fragments()
        {
            var unknownFragments = GetUnknownFragments("{T}: Add Potato to your mana pool.");
            Assert.Collections.AreEqual(new[] { "[Mana] Potato" }, unknownFragments);
        }

        [Test]
        public void Test_Colons_inside_sub_abilities_are_handled_correctly()
        {
            var unknownFragments = GetUnknownFragments("Plum pudding \"{T}: Add one mana of any color to your mana pool.\"");
            Assert.Collections.AreEqual(new[] { "[Rule] Plum pudding \"{T}: Add one mana of any color to your mana pool.\"" }, unknownFragments);

            unknownFragments = GetUnknownFragments("{T}: Plum pudding \"{T}: Add one mana of any color to your mana pool.\"");
            Assert.Collections.AreEqual(new[] { "[Effect] Plum pudding \"{T}: Add one mana of any color to your mana pool.\"" }, unknownFragments);
        }

        [Test]
        public void Test_Multiple_effects_can_be_parsed()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool. ~ deals 1 damage to you.");
            var spell = card.Abilities.OfType<ActivatedAbility>().Single().SpellDefinition;
            Assert.AreEqual(2, spell.Actions.Count);
            Assert.IsInstanceOf<GainManaAction>(spell.Actions[0]);
            Assert.IsInstanceOf<DealDamageAction>(spell.Actions[1]);
        }

        #endregion

        #region Costs

        private T GetCostOfActivatedAbility<T>(Card card)
            where T : Cost
        {
            var ability = card.Abilities.OfType<ActivatedAbility>().Single();
            return (T)ability.SpellDefinition.Costs.Single();
        }

        [Test]
        public void Test_Cost_TapSelf()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool.");
            var tapCost = GetCostOfActivatedAbility<TapCost>(card);
            Assert.IsInstanceOf<SpellSourceObjectResolver>(tapCost.Card);
            Assert.That(tapCost.DoTap);
        }

        #endregion

        #region Actions

        private T GetActionOfActivatedAbility<T>(Card card)
            where T : Action
        {
            var ability = card.Abilities.OfType<ActivatedAbility>().Single();
            return (T)ability.SpellDefinition.Actions.Single();
        }

        #region DealDamage

        [Test]
        public void Test_Action_DealDamage_to_you()
        {
            var card = CreateCard("{T}: ~ deals 2 damage to you.");
            var dealDamageAction = GetActionOfActivatedAbility<DealDamageAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, dealDamageAction.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)dealDamageAction.Damage).Amount);
        }

        #endregion

        #region GainMana

        [Test]
        public void Test_Action_GainMana()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(Color.White, gainManaAction.Color);
        }

        [Test]
        public void Test_Action_GainMana_with_choice()
        {
            var card = CreateCard("{T}: Add {W} or {B} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(Color.White | Color.Black, gainManaAction.Color);
        }

        [Test]
        public void Test_Action_GainMana_with_any_mana()
        {
            var card = CreateCard("{T}: Add one mana of any color to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(ColorExtensions.AllColors, gainManaAction.Color);
        }

        #endregion

        #endregion
    }
}
