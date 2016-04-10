using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Mox.AI;

namespace Mox.Flow
{
    public class RandomGameInput : IChoiceDecisionMaker
    {
        private readonly IRandom m_random = Random.New();

        public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
        {
            var choiceEnumeratorProvider = new AttributedChoiceEnumeratorProvider();
            var choiceEnumerator = choiceEnumeratorProvider.GetEnumerator(choice);

            ICollection<object> choiceResults = choiceEnumerator.EnumerateChoices(sequencer.Game, choice).ToArray();
            
            if (choiceResults.Count == 0)
                return choice.DefaultValue;

            return m_random.Choose(choiceResults);
        }
    }
}
