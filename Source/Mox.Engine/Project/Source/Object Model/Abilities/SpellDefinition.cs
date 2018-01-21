using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public struct SpellDefinitionIdentifier
    {
        public string SourceName;
        public int Id;
    }

    public class SpellDefinition
    {
        #region Variables

#if DEBUG
        private bool m_frozen;
#endif

        private readonly SpellDefinitionIdentifier m_identifier;
        
        private readonly List<Cost> m_costs = new List<Cost>();
        private readonly List<Action> m_actions = new List<Action>();

        private readonly ActionManaOutcome m_manaOutcome = new ActionManaOutcome();

        #endregion

        #region Static

        private static readonly SpellDefinition ms_empty = new SpellDefinition(new SpellDefinitionIdentifier());
        public static SpellDefinition Empty => ms_empty;

        #endregion

        #region Constructor

        public SpellDefinition(SpellDefinitionIdentifier identifier)
        {
            m_identifier = identifier;
        }

        #endregion

        #region Properties

        public IReadOnlyList<Cost> Costs => m_costs;
        public IReadOnlyList<Action> Actions => m_actions;

        public bool IsManaAbility
        {
            get
            {
                return !m_manaOutcome.IsEmpty;
            }
        }

        #endregion

        #region Methods

        public void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            if (m_manaOutcome.AnythingCanHappen)
            {
                outcome.AddAny();
                return;
            }

            foreach (var amount in m_manaOutcome.Amounts)
            {
                outcome.Add(amount);
            }
        }

        internal void AddCost(Cost cost)
        {
            ValidateNotFrozen();
            m_costs.Add(cost);
        }

        internal void AddAction(Action action)
        {
            ValidateNotFrozen();
            m_actions.Add(action);
            m_manaOutcome.Consider(action);
        }

        [Conditional("DEBUG")]
        internal void Freeze()
        {
            m_frozen = true;
        }

        [Conditional("DEBUG")]
        private void ValidateNotFrozen()
        {
            Debug.Assert(!m_frozen, "Invalid operation, already frozen");
        }

        #endregion
    }
}
