﻿using System.Collections.Generic;
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

            if (!string.IsNullOrEmpty(result.Error))
            {
                Assert.Fail(result.Error);
            }

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
            ParsesWithoutError("{W}: Add {W} to your mana pool.");

            // Doesn't parse
            DoesntParse("{W/T}: Add {W} to your mana pool.");
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

        [Test]
        public void Test_Effects_without_costs_are_added_to_the_playcard_ability()
        {
            var card = CreateCard("~ deals 1 damage to you");
            var playCardAbility = card.Abilities.OfType<PlayCardAbility>().Single();
            Assert.IsInstanceOf<DealDamageAction>(playCardAbility.SpellDefinition.Actions.Single());
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
            return ability.SpellDefinition.Costs.OfType<T>().Single();
        }

        [Test]
        public void Test_Cost_TapSelf()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool.");
            var tapCost = GetCostOfActivatedAbility<TapSelfCost>(card);
            Assert.That(tapCost.DoTap);
        }

        [Test]
        public void Test_Cost_ManaCost_0()
        {
            var card = CreateCard("{0}: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<PayManaCost>(card);
            Assert.AreEqual(new ManaCost(0), cost.ManaCost);
        }

        [Test]
        public void Test_Cost_ManaCost_Generic()
        {
            var card = CreateCard("{2}: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<PayManaCost>(card);
            Assert.AreEqual(new ManaCost(2), cost.ManaCost);
        }

        [Test]
        public void Test_Cost_ManaCost_Single()
        {
            var card = CreateCard("{R}: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<PayManaCost>(card);
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R), cost.ManaCost);
        }

        [Test]
        public void Test_Cost_ManaCost_Multiple()
        {
            var card = CreateCard("{R}{G}: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<PayManaCost>(card);
            Assert.AreEqual(new ManaCost(0, ManaSymbol.R, ManaSymbol.G), cost.ManaCost);
        }

        [Test]
        public void Test_Cost_ManaCost_Hybrid()
        {
            var card = CreateCard("{R/G}{2/G}: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<PayManaCost>(card);
            Assert.AreEqual(new ManaCost(0, ManaSymbol.RG, ManaSymbol.G2), cost.ManaCost);
        }

        [Test]
        public void Test_Cost_Sacrifice_Self()
        {
            var card = CreateCard("Sacrifice ~: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<SacrificeCost>(card);
            Assert.AreEqual(ObjectResolver.SpellSource, cost.Cards);
        }

        [Test]
        public void Test_Cost_Sacrifice_a_creature()
        {
            var card = CreateCard("Sacrifice a creature: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<SacrificeCost>(card);
            var targetCost = ((TargetObjectResolver)cost.Cards).TargetCost;
            Assert.AreEqual(TargetContextType.SacrificeCost, targetCost.Type);
            Assert.AreMemberwiseEqual(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou, targetCost.Filter);
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

        #region Tap

        [Test]
        public void Test_Action_Tap_target()
        {
            var card = CreateCard("{T}: Tap target creature.");
            var action = GetActionOfActivatedAbility<TapAction>(card);
            var targetResolver = (TargetObjectResolver)action.Cards;
            var targetCost = targetResolver.TargetCost;
            AssertTargetEquals(PermanentFilter.AnyCreature, targetCost);
        }

        #endregion

        #endregion

        #region Targets

        private TargetCost GetTargetOfActivatedAbility(string targetText)
        {
            var card = CreateCard("{T}: ~ deals 1 damage to " + targetText);
            var ability = card.Abilities.OfType<ActivatedAbility>().Single();

            var costs = ability.SpellDefinition.Costs;
            var action = (DealDamageAction)ability.SpellDefinition.Actions.Single();

            var targetResolver = (TargetObjectResolver)action.Targets;
            var targetCost = targetResolver.TargetCost;
            Assert.Collections.Contains(targetCost, costs);

            return targetCost;
        }

        private void AssertTargetEquals(Filter expected, TargetCost actual)
        {
            var actualFilter = actual.Filter;
            Assert.AreMemberwiseEqual(expected, actualFilter);
        }

        [Test]
        public void Test_Target_You_is_the_spell_controller()
        {
            var card = CreateCard("{T}: ~ deals 1 damage to you.");
            var dealDamageAction = GetActionOfActivatedAbility<DealDamageAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, dealDamageAction.Targets);
        }

        [Test]
        public void Test_Target_Creature()
        {
            var targetCost = GetTargetOfActivatedAbility("target creature");
            AssertTargetEquals(PermanentFilter.AnyCreature, targetCost);
        }

        [Test]
        public void Test_Target_Player()
        {
            var targetCost = GetTargetOfActivatedAbility("target player");
            AssertTargetEquals(PlayerFilter.Any, targetCost);
        }

        [Test]
        public void Test_Target_Creature_or_Player()
        {
            var targetCost = GetTargetOfActivatedAbility("target creature or player");
            AssertTargetEquals(new CreatureOrPlayerFilter(), targetCost);
        }

        [Test]
        public void Test_Target_Creature_you_control()
        {
            var targetCost = GetTargetOfActivatedAbility("target creature you control");
            AssertTargetEquals(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou, targetCost);
        }

        #endregion
    }
}
