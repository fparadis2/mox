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

        private void ParsesWithoutError(string text, Type type = Type.Creature)
        {
            RuleParser parser = new RuleParser("The Source");
            var result = parser.Parse(type, text);
            Assert.That(result.IsValid, result.UnknownFragments.FirstOrDefault());
        }

        private void DoesntParse(string text, Type type = Type.Creature)
        {
            RuleParser parser = new RuleParser("The Source");
            Assert.IsFalse(parser.Parse(type, text).IsValid);
        }

        private IEnumerable<string> GetUnknownFragments(string text, Type type = Type.Creature)
        {
            RuleParser parser = new RuleParser("The Source");
            var result = parser.Parse(type, text);
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

        private static T GetAbility<T>(Card card)
            where T : Ability
        {
            return card.Abilities.OfType<T>().Single();
        }

        private static void AssertCountEquals(int expected, AmountResolver actual)
        {
            Assert.AreEqual(expected, ((ConstantAmountResolver)actual).Amount);
        }

        private void AssertFilterEquals(Filter expected, FilterObjectResolver actual)
        {
            var actualFilter = actual.Filter;
            Assert.AreMemberwiseEqual(expected, actualFilter);
        }

        private void AssertFilterEquals(Filter expected, Filter actual)
        {
            Assert.AreMemberwiseEqual(expected, actual);
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
            ParsesWithoutError("Sacrifice ~: Add {W}{U}{B}{R}{G} to your mana pool.");

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

        [Test]
        public void Test_StaticAbility_Equip()
        {
            var card = CreateCard("Equip {2}");

            var activatedAbility = card.Abilities.OfType<ActivatedAbility>().Single();

            var spell = activatedAbility.SpellDefinition;
            Assert.AreEqual(AbilitySpeed.Sorcery, spell.Speed);

            Assert.AreEqual(2, spell.Costs.Count);
            var targetCost = (TargetCost)spell.Costs[0];
            AssertTargetEquals(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou, targetCost);
            var manaCost = (PayManaCost)spell.Costs[1];
            Assert.AreEqual(new ManaCost(2), manaCost.ManaCost);

            var action = (AttachAction)spell.Actions.Single();
            Assert.AreEqual(targetCost, ((TargetObjectResolver)action.Target).TargetCost);
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
        public void Test_The_PlayCardAbility_is_sorcery_or_instant()
        {
            var card = CreateCard(string.Empty, Type.Creature, "R");
            var ability = GetAbility<PlayCardAbility>(card);
            Assert.AreEqual(AbilitySpeed.Sorcery, ability.AbilitySpeed);

            card = CreateCard(string.Empty, Type.Instant, "R");
            ability = GetAbility<PlayCardAbility>(card);
            Assert.AreEqual(AbilitySpeed.Instant, ability.AbilitySpeed);
        }

        [Test]
        public void Test_Effects_without_costs_are_added_to_the_playcard_ability()
        {
            var card = CreateCard("~ deals 1 damage to you", Type.Instant);
            var playCardAbility = card.Abilities.OfType<PlayCardAbility>().Single();
            Assert.IsInstanceOf<DealDamageAction>(playCardAbility.SpellDefinition.Actions.Single());
        }

        [Test]
        public void Test_Enchant_ability()
        {
            var card = CreateCard("Enchant creature", Type.Enchantment);
            var playCardAbility = card.Abilities.OfType<PlayCardAbility>().Single();
            var attachAction = (AttachAction)playCardAbility.SpellDefinition.Actions.Single();
            var targetResolver = (TargetObjectResolver)attachAction.Target;
            Assert.Collections.Contains(targetResolver.TargetCost, playCardAbility.SpellDefinition.Costs);
            AssertTargetEquals(PermanentFilter.AnyCreature, targetResolver.TargetCost);
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

        [Test]
        public void Test_Activated_abilities_are_instant_in_general()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool. ~ deals 1 damage to you.");
            var ability = GetAbility<ActivatedAbility>(card);
            Assert.AreEqual(AbilitySpeed.Instant, ability.AbilitySpeed);
        }

        #endregion

        #region Continuous Abilities

        private T GetActionOfContinuousAbility<T>(Card card)
            where T : Action
        {
            return (T)GetActionsOfContinuousAbility(card).Single();
        }

        private IReadOnlyList<Action> GetActionsOfContinuousAbility(Card card)
        {
            var ability = card.Abilities.OfType<ContinuousAbility>().Single();
            return ability.SpellDefinition.Actions;
        }

        [Test]
        public void Test_Continuous_ability_Modify_PT()
        {
            var card = CreateCard("Creatures you control get +1/+1.", Type.Enchantment);
            var action = GetActionOfContinuousAbility<ModifyPowerAndToughnessAction>(card);

            var targets = (FilterObjectResolver)action.Targets;
            AssertFilterEquals(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou, targets);

            Assert.AreEqual(+1, ((ConstantAmountResolver)action.Power).Amount);
            Assert.AreEqual(+1, ((ConstantAmountResolver)action.Toughness).Amount);

            Assert.IsNull(action.ScopeType);
        }

        [Test]
        public void Test_Aura_Modify_PT()
        {
            var card = CreateCard("Enchanted creature get +1/+1.", Type.Enchantment);
            var action = GetActionOfContinuousAbility<ModifyPowerAndToughnessAction>(card);

            Assert.IsInstanceOf<AttachedToObjectResolver>(action.Targets);

            Assert.AreEqual(+1, ((ConstantAmountResolver)action.Power).Amount);
            Assert.AreEqual(+1, ((ConstantAmountResolver)action.Toughness).Amount);

            Assert.IsNull(action.ScopeType);
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

        [Test]
        public void Test_Cost_Discard_a_card()
        {
            var card = CreateCard("Discard a card: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<DiscardCost>(card);
            Assert.IsFalse(cost.AtRandom);
            AssertCountEquals(1, cost.Count);
            AssertFilterEquals(HandFilter.Any, cost.Filter);
        }

        [Test]
        public void Test_Cost_Discard_a_card_at_random()
        {
            var card = CreateCard("Discard a card at random: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<DiscardCost>(card);
            Assert.IsTrue(cost.AtRandom);
            AssertCountEquals(1, cost.Count);
            AssertFilterEquals(HandFilter.Any, cost.Filter);
        }

        [Test]
        public void Test_Cost_Discard_two_creature_cards()
        {
            var card = CreateCard("Discard two creature cards: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<DiscardCost>(card);
            AssertCountEquals(2, cost.Count);
            AssertFilterEquals(HandFilter.Any & CardFilter.WithType(Type.Creature), cost.Filter);
        }

        [Test]
        public void Test_Cost_Discard_your_hand()
        {
            var card = CreateCard("Discard your hand: Add {W} to your mana pool.");
            var cost = GetCostOfActivatedAbility<DiscardHandCost>(card);
        }

        #endregion

        #region Actions

        private T GetActionOfActivatedAbility<T>(Card card)
            where T : Action
        {
            return (T)GetActionsOfActivatedAbility(card).Single();
        }

        private IReadOnlyList<Action> GetActionsOfActivatedAbility(Card card)
        {
            var ability = card.Abilities.OfType<ActivatedAbility>().Single();
            return ability.SpellDefinition.Actions;
        }

        #region Counter

        [Test]
        public void Test_Action_Counter_target_spell()
        {
            var card = CreateCard("{T}: Counter target spell.");
            var action = GetActionOfActivatedAbility<CounterAction>(card);
            var targetResolver = (TargetObjectResolver)action.Spells;
            var targetCost = targetResolver.TargetCost;
            AssertTargetEquals(StackFilter.AnySpell, targetCost);
        }

        #endregion

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

        #region Discard

        [Test]
        public void Test_Action_Discard_hand()
        {
            var card = CreateCard("{T}: Target player discards his or her hand.");
            var action = GetActionOfActivatedAbility<DiscardAction>(card);
            Assert.IsInstanceOf<TargetObjectResolver>(action.Targets);
            Assert.AreEqual(int.MaxValue, ((ConstantAmountResolver)action.Count).Amount);
        }

        [Test]
        public void Test_Action_Discard_your_hand()
        {
            var card = CreateCard("{T}: Discard your hand.");
            var action = GetActionOfActivatedAbility<DiscardAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, action.Targets);
            Assert.AreEqual(int.MaxValue, ((ConstantAmountResolver)action.Count).Amount);
        }

        [Test]
        public void Test_Action_Discard_cards()
        {
            var card = CreateCard("{T}: Target player discards one card.");
            var action = GetActionOfActivatedAbility<DiscardAction>(card);
            Assert.IsInstanceOf<TargetObjectResolver>(action.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)action.Count).Amount);
        }

        [Test]
        public void Test_Action_Discard_cards_you()
        {
            var card = CreateCard("{T}: Discard two cards.");
            var action = GetActionOfActivatedAbility<DiscardAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, action.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)action.Count).Amount);
        }

        [Test]
        public void Test_Action_Discard_cards_at_random()
        {
            var card = CreateCard("{T}: Target player discards one card at random.");
            var action = GetActionOfActivatedAbility<DiscardAtRandomAction>(card);
            Assert.IsInstanceOf<TargetObjectResolver>(action.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)action.Count).Amount);
        }

        [Test]
        public void Test_Action_Discard_cards_you_at_random()
        {
            var card = CreateCard("{T}: Discard two cards at random.");
            var action = GetActionOfActivatedAbility<DiscardAtRandomAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, action.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)action.Count).Amount);
        }

        #endregion

        #region Draw

        [Test]
        public void Test_Action_Draw_card()
        {
            var card = CreateCard("{T}: Draw a card.");
            var action = GetActionOfActivatedAbility<DrawCardsAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, action.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)action.Amount).Amount);
        }

        [Test]
        public void Test_Action_Draw_cards()
        {
            var card = CreateCard("{T}: Draw 2 cards.");
            var action = GetActionOfActivatedAbility<DrawCardsAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, action.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)action.Amount).Amount);
        }

        [Test]
        public void Test_Action_Draw_card_target()
        {
            var card = CreateCard("{T}: Target player draws 2 cards.");
            var action = GetActionOfActivatedAbility<DrawCardsAction>(card);
            Assert.IsInstanceOf<TargetObjectResolver>(action.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)action.Amount).Amount);
        }

        #endregion

        #region Draw & Discard

        [Test]
        public void Test_Draw_and_Discard_target()
        {
            var card = CreateCard("{T}: Target player draws 3 cards, then discards one card.");

            var actions = GetActionsOfActivatedAbility(card);
            Assert.AreEqual(2, actions.Count);

            var drawAction = (DrawCardsAction)actions[0];
            Assert.IsInstanceOf<TargetObjectResolver>(drawAction.Targets);
            Assert.AreEqual(3, ((ConstantAmountResolver)drawAction.Amount).Amount);

            var discardAction = (DiscardAction)actions[1];
            Assert.IsInstanceOf<TargetObjectResolver>(discardAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)discardAction.Count).Amount);
        }

        [Test]
        public void Test_Draw_and_Discard_you()
        {
            var card = CreateCard("{T}: Draw 1 card, then discard two cards.");

            var actions = GetActionsOfActivatedAbility(card);
            Assert.AreEqual(2, actions.Count);

            var drawAction = (DrawCardsAction)actions[0];
            Assert.AreEqual(ObjectResolver.SpellController, drawAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)drawAction.Amount).Amount);

            var discardAction = (DiscardAction)actions[1];
            Assert.AreEqual(ObjectResolver.SpellController, discardAction.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)discardAction.Count).Amount);
        }

        [Test]
        public void Test_Draw_and_Discard_at_random()
        {
            var card = CreateCard("{T}: Draw 1 card, then discard two cards at random.");

            var actions = GetActionsOfActivatedAbility(card);
            Assert.AreEqual(2, actions.Count);

            var drawAction = (DrawCardsAction)actions[0];
            Assert.AreEqual(ObjectResolver.SpellController, drawAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)drawAction.Amount).Amount);

            var discardAction = (DiscardAtRandomAction)actions[1];
            Assert.AreEqual(ObjectResolver.SpellController, discardAction.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)discardAction.Count).Amount);
        }

        #endregion

        #region Effect - PT

        [Test]
        public void Test_Action_Modify_PT()
        {
            var card = CreateCard("{T}: Target creature gets +1/-1 until end of turn.");
            var action = GetActionOfActivatedAbility<ModifyPowerAndToughnessAction>(card);

            var targetResolver = (TargetObjectResolver)action.Targets;
            var targetCost = targetResolver.TargetCost;
            AssertTargetEquals(PermanentFilter.AnyCreature, targetCost);

            Assert.AreEqual(+1, ((ConstantAmountResolver)action.Power).Amount);
            Assert.AreEqual(-1, ((ConstantAmountResolver)action.Toughness).Amount);

            Assert.AreEqual(typeof(UntilEndOfTurnScope), action.ScopeType);
        }

        #endregion

        #region GainLife

        [Test]
        public void Test_Action_GainLife_You()
        {
            var card = CreateCard("{T}: You gain 1 life.");
            var gainLifeAction = GetActionOfActivatedAbility<GainLifeAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, gainLifeAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)gainLifeAction.Life).Amount);
        }

        [Test]
        public void Test_Action_Lose_life_and_you_gain_life()
        {
            var card = CreateCard("Sacrifice ~: Target player loses 1 life and you gain 2 life.");

            var actions = GetActionsOfActivatedAbility(card);
            Assert.AreEqual(2, actions.Count);

            var loseLifeAction = (LoseLifeAction)actions[0];
            AssertTargetEquals(PlayerFilter.Any, GetTarget(loseLifeAction.Targets));
            Assert.AreEqual(1, ((ConstantAmountResolver)loseLifeAction.Life).Amount);

            var gainLifeAction = (GainLifeAction)actions[1];
            Assert.AreEqual(ObjectResolver.SpellController, gainLifeAction.Targets);
            Assert.AreEqual(2, ((ConstantAmountResolver)gainLifeAction.Life).Amount);
        }

        #endregion

        #region GainMana

        [Test]
        public void Test_Action_GainMana()
        {
            var card = CreateCard("{T}: Add {W} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(new[] { new ManaAmount { White = 1 } }, gainManaAction.Amounts);
        }

        [Test]
        public void Test_Action_GainMana_with_choice()
        {
            var card = CreateCard("{T}: Add {W} or {B} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(new[] { new ManaAmount { White = 1 }, new ManaAmount { Black = 1 } }, gainManaAction.Amounts);
        }

        [Test]
        public void Test_Action_GainMana_with_any_mana()
        {
            var card = CreateCard("{T}: Add one mana of any color to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);

            var expectedAmounts = new[]
            {
                new ManaAmount { White = 1 },
                new ManaAmount { Blue = 1 },
                new ManaAmount { Black = 1 },
                new ManaAmount { Red = 1 },
                new ManaAmount { Green = 1 },
            };

            Assert.AreEqual(expectedAmounts, gainManaAction.Amounts);
        }

        [Test]
        public void Test_Action_GainMana_with_double_mana()
        {
            var card = CreateCard("{T}: Add {W}{W} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(new[] { new ManaAmount { White = 2 } }, gainManaAction.Amounts);
        }

        [Test]
        public void Test_Action_GainMana_with_multiple_mana()
        {
            var card = CreateCard("{T}: Add {W}{U} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);
            Assert.AreEqual(new[] { new ManaAmount { White = 1, Blue = 1 } }, gainManaAction.Amounts);
        }

        [Test]
        public void Test_Action_GainMana_with_multiple_mana_and_choices()
        {
            var card = CreateCard("{T}: Add {W}{U}, {W}{W} or {U}{U} to your mana pool.");
            var gainManaAction = GetActionOfActivatedAbility<GainManaAction>(card);

            var expectedAmounts = new[]
            {
                new ManaAmount { White = 1, Blue = 1 },
                new ManaAmount { White = 2 },
                new ManaAmount { Blue = 2 },
            };

            Assert.AreEqual(expectedAmounts, gainManaAction.Amounts);
        }

        #endregion

        #region LoseLife

        [Test]
        public void Test_Action_LoseLife_You()
        {
            var card = CreateCard("{T}: You lose 1 life.");
            var loseLifeAction = GetActionOfActivatedAbility<LoseLifeAction>(card);
            Assert.AreEqual(ObjectResolver.SpellController, loseLifeAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)loseLifeAction.Life).Amount);
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

        private TargetCost GetTarget(ObjectResolver resolver)
        {
            var targetResolver = (TargetObjectResolver)resolver;
            return targetResolver.TargetCost;
        }

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
        public void Test_Target_Each_player_1()
        {
            var card = CreateCard("{T}: ~ deals 1 damage to each player.");
            var dealDamageAction = GetActionOfActivatedAbility<DealDamageAction>(card);
            AssertFilterEquals(PlayerFilter.Any, (FilterObjectResolver)dealDamageAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)dealDamageAction.Damage).Amount);
        }

        [Test]
        public void Test_Target_Each_player_2()
        {
            var card = CreateCard("{T}: Each player draws a card.");
            var drawAction = GetActionOfActivatedAbility<DrawCardsAction>(card);
            AssertFilterEquals(PlayerFilter.Any, (FilterObjectResolver)drawAction.Targets);
            Assert.AreEqual(1, ((ConstantAmountResolver)drawAction.Amount).Amount);
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

        [Test]
        public void Test_Multiple_targets()
        {
            var card = CreateCard("Target creature gets -3/-0 until end of turn.\nTarget creature gets -0/-3 until end of turn.", Type.Instant);

            var ability = (PlayCardAbility)card.Abilities.Single();

            var targetCosts = ability.SpellDefinition.Costs.OfType<TargetCost>().ToList();
            var actions = ability.SpellDefinition.Actions;

            Assert.AreEqual(2, targetCosts.Count);
            Assert.AreEqual(2, actions.Count);

            var action1 = (ModifyPowerAndToughnessAction)actions[0];
            var action2 = (ModifyPowerAndToughnessAction)actions[1];

            Assert.AreEqual(targetCosts[0], ((TargetObjectResolver)action1.Targets).TargetCost);
            Assert.AreEqual(targetCosts[1], ((TargetObjectResolver)action2.Targets).TargetCost);
        }

        #endregion

        #region Filters

        [Test]
        public void Test_Filter_creature_colored()
        {
            var targetCost = GetTargetOfActivatedAbility("target black creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.WithColor(Color.Black), targetCost);
        }

        [Test]
        public void Test_Filter_creature_noncolored()
        {
            var targetCost = GetTargetOfActivatedAbility("target nonblack creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.NotWithColor(Color.Black), targetCost);
        }

        [Test]
        public void Test_Filter_creature_of_type()
        {
            var targetCost = GetTargetOfActivatedAbility("target artifact creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.WithType(Type.Artifact), targetCost);
        }

        [Test]
        public void Test_Filter_creature_of_nontype()
        {
            var targetCost = GetTargetOfActivatedAbility("target nonartifact creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.NotWithType(Type.Artifact), targetCost);
        }

        [Test]
        public void Test_Filter_creatures_with_subtype()
        {
            var targetCost = GetTargetOfActivatedAbility("target merfolk creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.WithSubType(SubType.Merfolk), targetCost);
        }

        [Test]
        public void Test_Filter_creatures_with_nonsubtype()
        {
            var targetCost = GetTargetOfActivatedAbility("target non-merfolk creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.NotWithSubType(SubType.Merfolk), targetCost);
        }

        [Test]
        public void Test_Filter_creatures_with_supertype()
        {
            var targetCost = GetTargetOfActivatedAbility("target legendary creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.WithSuperType(SuperType.Legendary), targetCost);
        }

        [Test]
        public void Test_Filter_creatures_with_nonsupertype()
        {
            var targetCost = GetTargetOfActivatedAbility("target nonbasic creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.NotWithSuperType(SuperType.Basic), targetCost);
        }

        [Test]
        public void Test_Filter_creature_with_mixed_prefix()
        {
            var targetCost = GetTargetOfActivatedAbility("target black nonartifact creature");
            AssertTargetEquals(PermanentFilter.AnyCreature & CardFilter.WithColor(Color.Black) & CardFilter.NotWithType(Type.Artifact), targetCost);
        }

        [Test]
        public void Test_Filter_creature_you_control_with_mixed_prefix()
        {
            var targetCost = GetTargetOfActivatedAbility("target black nonartifact creature you control");
            AssertTargetEquals(PermanentFilter.AnyCreature & PermanentFilter.ControlledByYou & CardFilter.WithColor(Color.Black) & CardFilter.NotWithType(Type.Artifact), targetCost);
        }

        [Test]
        public void Test_Filter_card_from_your_hand()
        {
            var targetCost = GetTargetOfActivatedAbility("target card from your hand");
            AssertTargetEquals(HandFilter.Any, targetCost);
        }

        [Test]
        public void Test_Filter_card_from_your_hand_with_mixed_prefix()
        {
            var targetCost = GetTargetOfActivatedAbility("target blue nonland creature card from your hand");
            AssertTargetEquals(HandFilter.Any & CardFilter.WithColor(Color.Blue) & CardFilter.NotWithType(Type.Land) & CardFilter.WithType(Type.Creature), targetCost);
        }

        #endregion
    }
}
