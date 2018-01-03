using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database
{
    public partial class RuleParser
    {
        #region Constants

        private static readonly char[] RuleSeparators = new[] { '\n' };
        private static readonly char[] AbilitySeparators = new[] { ',' };

        #endregion

        #region Variables

        private readonly List<Initializer> m_initializers = new List<Initializer>();
        private readonly List<string> m_unknownFragments = new List<string>();

        private Initializer m_playCardInitializer;

        #endregion

        #region Properties

        public IReadOnlyList<string> UnknownFragments => m_unknownFragments;

        #endregion

        #region Methods

        public void Initialize(Card card)
        {
            foreach (var initializer in m_initializers)
            {
                initializer(card);
            }
        }

        public bool Parse(CardInfo cardInfo)
        {
            AddPlayAbility(cardInfo);
            return Parse(cardInfo.Text);
        }

        public bool Parse(string text)
        {
            text = text ?? string.Empty;

            foreach (var rule in SplitAndTrim(text, RuleSeparators))
            {
                // Todo: try complex parsing
                if (!ParseAbilityList(rule))
                {
                    m_unknownFragments.Add(rule);
                }
            }

            return m_unknownFragments.Count == 0;
        }

        private bool ParseAbilityList(string text)
        {
            List<string> unknownFragments = new List<string>();
            int count = 0;

            foreach (var ability in SplitAndTrim(text, AbilitySeparators))
            {
                count++;
                var initializer = StaticAbility.GetInitializer(ability);

                if (initializer != null)
                {
                    m_initializers.Add(initializer);
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

        private static IEnumerable<string> SplitAndTrim(string text, char[] separators)
        {
            var tokens = text.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                string trimmed = token.Trim();
                if (trimmed.Length == 0)
                    continue;

                yield return trimmed;
            }
        }

        #endregion

        #region Helpers

        private void AddPlayAbility(CardInfo cardInfo)
        {
            m_playCardInitializer = card => 
            {
                var playCardAbility = AddAbility<PlayCardAbility>(card);
                playCardAbility.ManaCost = ManaCost.Parse(cardInfo.ManaCost);
            };

            m_initializers.Add(m_playCardInitializer);
        }

        private static TAbility AddAbility<TAbility>(Card card)
            where TAbility : Ability, new()
        {
            return card.Manager.CreateAbility<TAbility>(card);
        }

        #endregion

        #region Inner Types

        private delegate void Initializer(Card card);

        #endregion
    }
}
