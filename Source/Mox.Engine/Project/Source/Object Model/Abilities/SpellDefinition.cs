using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    [Serializable]
    public struct SpellDefinitionIdentifier : IHashable, IEquatable<SpellDefinitionIdentifier>
    {
        public string SourceName;
        public int Id;

        public void ComputeHash(Hash hash, HashContext context)
        {
            hash.Add(SourceName);
            hash.Add(Id);
        }

        public override bool Equals(object obj)
        {
            return Equals((SpellDefinitionIdentifier)obj);
        }

        public bool Equals(SpellDefinitionIdentifier other)
        {
            return string.Equals(SourceName, other.SourceName) && Id == other.Id;
        }

        public override int GetHashCode()
        {
            int hash = 23;
            hash = hash * 37 + SourceName.GetHashCode();
            hash = hash * 37 + Id;
            return hash;
        }

        public override string ToString()
        {
            return SourceName + $" [{Id}]";
        }
    }

    public class SpellDefinition : IHashable
    {
        #region Variables

#if DEBUG
        private bool m_frozen;
#endif

        private readonly SpellDefinitionIdentifier m_identifier;
        
        private readonly List<Cost> m_costs = new List<Cost>();
        private readonly List<Action> m_actions = new List<Action>();

        private readonly ActionManaOutcome m_manaOutcome = new ActionManaOutcome();

        private AbilitySpeed m_speed = AbilitySpeed.Instant;
        private Trigger m_trigger;

        #endregion

        #region Static

        private static readonly SpellDefinition ms_empty = new SpellDefinition(new SpellDefinitionIdentifier());
        public static SpellDefinition Empty => ms_empty;

        private static readonly CostComparer ms_costComparer = new CostComparer();

        #endregion

        #region Constructor

        public SpellDefinition(SpellDefinitionIdentifier identifier)
        {
            m_identifier = identifier;
        }

        #endregion

        #region Properties

        public SpellDefinitionIdentifier Identifier => m_identifier;

        public IReadOnlyList<Cost> Costs => m_costs;
        public IReadOnlyList<Action> Actions => m_actions;

        public bool IsManaAbility
        {
            get
            {
                return !m_manaOutcome.IsEmpty;
            }
        }
        
        public AbilitySpeed Speed
        {
            get { return m_speed; }
            set
            {
                ValidateNotFrozen();
                m_speed = value;
            }
        }

        public Trigger Trigger
        {
            get { return m_trigger; }
            set
            {
                ValidateNotFrozen();
                m_trigger = value;
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

            int index = m_costs.UpperBound(cost, ms_costComparer);
            m_costs.Insert(index, cost);
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
#if DEBUG
            m_frozen = true;
#endif
        }

        [Conditional("DEBUG")]
        private void ValidateNotFrozen()
        {
#if DEBUG
            Debug.Assert(!m_frozen, "Invalid operation, already frozen");
#endif
        }

        public void ComputeHash(Hash hash, HashContext context)
        {
            m_identifier.ComputeHash(hash, context);
        }

        #endregion

        #region Nested Types

        private class CostComparer : IComparer<Cost>
        {
            public int Compare(Cost x, Cost y)
            {
                return x.Order.CompareTo(y.Order);
            }
        }

        #endregion
    }

    public class SpellDefinitionRepository
    {
        private readonly Dictionary<SpellDefinitionIdentifier, SpellDefinition> m_spellDefinitions = new Dictionary<SpellDefinitionIdentifier, SpellDefinition>();

        public void Register(SpellDefinition spellDefinition)
        {
            m_spellDefinitions.Add(spellDefinition.Identifier, spellDefinition);
        }

        public SpellDefinition GetSpellDefinition(SpellDefinitionIdentifier identifier)
        {
            SpellDefinition result;
            if (!m_spellDefinitions.TryGetValue(identifier, out result))
            {
                result = GetOrCreateSpellDefinition(identifier);
                Debug.Assert(Equals(result.Identifier, identifier));
                Register(result);
            }
            return result;
        }

        protected virtual SpellDefinition GetOrCreateSpellDefinition(SpellDefinitionIdentifier identifier)
        {
            throw new InvalidOperationException("Spell definition not found: " + identifier);
        }
    }
}
