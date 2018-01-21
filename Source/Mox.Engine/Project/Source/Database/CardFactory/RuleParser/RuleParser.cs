using System;
using System.Collections.Generic;

using Mox.Abilities;

namespace Mox.Database
{
    public struct RuleParserResult
    {
        public IReadOnlyList<IAbilityCreator> Abilities { get; set; }
        public IReadOnlyList<string> UnknownFragments { get; set; }

        public bool IsValid => UnknownFragments.Count == 0;

        public void Initialize(Card card)
        {
            foreach (var abilityCreator in Abilities)
            {
                abilityCreator.Create(card);
            }
        }
    }

    public partial class RuleParser
    {
        #region Variables

        private readonly string m_spellSourceName;
        private int m_spellIndex;

        private readonly List<IAbilityCreator> m_abilities = new List<IAbilityCreator>();
        private readonly List<string> m_unknownFragments = new List<string>();

        private readonly SpellDefinition m_playCardSpellDefinition;

        #endregion

        #region Constructor

        public RuleParser(string spellSourceName)
        {
            m_spellSourceName = spellSourceName;
            m_playCardSpellDefinition = CreateSpellDefinition();
        }

        #endregion

        #region Methods

        public RuleParserResult Parse(CardInfo cardInfo)
        {
            SetupPlayAbility(cardInfo);

            string text = cardInfo.Text;
            text = RemoveThisName(cardInfo, text);

            if (cardInfo.Type.HasFlag(Type.Land))
            {
                foreach (var rule in AddIntrinsicLandAbilities(cardInfo))
                {
                    text += '\n' + rule;
                }
            }

            return Parse(text);
        }

        // For tests
        public RuleParserResult Parse(string text)
        {
            text = text ?? string.Empty;

            foreach (var rawRule in SplitAndTrim(text, RuleSeparators))
            {
                var rule = RemoveReminderText(rawRule);
                if (!ParseRule(rule))
                {
                    AddUnknownFragment("Rule", rule);
                }
            }

            return new RuleParserResult { Abilities = m_abilities, UnknownFragments = m_unknownFragments };
        }

        private bool ParseRule(string text)
        {
            if (ParseAbility(text, out IAbilityCreator creator))
            {
                if (creator != null)
                    m_abilities.Add(creator);

                return true;
            }

            if (ParseStaticAbilityList(text))
                return true;

            return ParseEffects(text, m_playCardSpellDefinition, false);
        }

        #endregion

        #region Helpers

        private SpellDefinition CreateSpellDefinition()
        {
            var identifier = new SpellDefinitionIdentifier { SourceName = m_spellSourceName, Id = m_spellIndex++ };
            return new SpellDefinition(identifier);
        }

        private void SetupPlayAbility(CardInfo cardInfo)
        {
            var manaCost = ManaCost.Parse(cardInfo.ManaCost);
            m_playCardSpellDefinition.AddCost(new PayManaCost(manaCost));

            m_abilities.Add(new AbilityCreator<PlayCardAbility> { SpellDefinition = m_playCardSpellDefinition });
        }

        private void AddUnknownFragment(string category, string fragment)
        {
            m_unknownFragments.Add($"[{category}] {fragment}");
        }

        #endregion

        #region Implicit

        private const string ImplicitLandAbility = "{{T}}: Add {0} to your mana pool.";

        private static IEnumerable<string> AddIntrinsicLandAbilities(CardInfo cardInfo)
        {
            foreach (var subType in cardInfo.SubTypes)
            {
                switch (subType)
                {
                    case SubType.Plains:
                        yield return string.Format(ImplicitLandAbility, "{W}");
                        break;
                    case SubType.Island:
                        yield return string.Format(ImplicitLandAbility, "{U}");
                        break;
                    case SubType.Swamp:
                        yield return string.Format(ImplicitLandAbility, "{B}");
                        break;
                    case SubType.Mountain:
                        yield return string.Format(ImplicitLandAbility, "{R}");
                        break;
                    case SubType.Forest:
                        yield return string.Format(ImplicitLandAbility, "{G}");
                        break;

                    default:
                        break;
                }
            }
        }

        #endregion

        #region Inner Types

        private delegate void Initializer(Card card);

        #endregion
    }
}
