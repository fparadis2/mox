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

    // . SpellAbility should contain a spell definition
    // . Revisit all factory abilities to use new spell definition
    // . Redo spell stack, push/resolve mechanics to support new spell definition
    // . Check tests by that time...

    public class SpellDefinition
    {
        #region Variables

#if DEBUG
        private bool m_frozen;
#endif

        private readonly SpellDefinitionIdentifier m_identifier;
        private readonly List<Cost> m_costs = new List<Cost>();
        private readonly List<Action> m_actions = new List<Action>();

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

        #endregion

        #region Methods

        internal void AddCost(Cost cost)
        {
            ValidateNotFrozen();
            m_costs.Add(cost);
        }

        internal void AddAction(Action action)
        {
            ValidateNotFrozen();
            m_actions.Add(action);
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
