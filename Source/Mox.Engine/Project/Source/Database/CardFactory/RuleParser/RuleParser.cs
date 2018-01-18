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
            return Parse(text);
        }

        // For tests
        public RuleParserResult Parse(string text)
        {
            text = text ?? string.Empty;

            foreach (var rule in SplitAndTrim(text, RuleSeparators))
            {
                var normalizedRule = RemoveReminderText(rule);

                // Todo: try complex parsing
                if (!ParseAbilityList(normalizedRule))
                {
                    m_unknownFragments.Add(normalizedRule);
                }
            }

            return new RuleParserResult { Abilities = m_abilities, UnknownFragments = m_unknownFragments };
        }

        private bool ParseAbilityList(string text)
        {
            List<string> unknownFragments = new List<string>();
            int count = 0;

            foreach (var ability in SplitAndTrim(text, AbilitySeparators))
            {
                count++;

                if (StaticAbility.TryGetCreator(ability, out IAbilityCreator creator))
                {
                    if (creator != null)
                        m_abilities.Add(creator);
                }
                else
                {
                    unknownFragments.Add(ability);
                }
            }

            if (unknownFragments.Count > 0 && unknownFragments.Count < count)
            {
                m_unknownFragments.AddRange(unknownFragments);
            }

            return unknownFragments.Count == 0;
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

        #endregion

        #region Inner Types

        private delegate void Initializer(Card card);

        #endregion
    }
}
