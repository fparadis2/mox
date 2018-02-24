using Mox.Collections;
using Mox.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    /// <summary>
    /// The spell stack
    /// </summary>
    /// <remarks>
    /// Not to be confused with the stack zone (which is where the cards on the stack are); this stack holds spells only.
    /// </remarks>
    public class SpellStack2 : IReadOnlyList<Spell2>
    {
        #region Variables

        private readonly Game m_game;
        private readonly List<Spell2> m_spells = new List<Spell2>();

        #endregion

        #region Constructor

        public SpellStack2(Game game)
        {
            Throw.IfNull(game, "game");
            m_game = game;
        }

        #endregion

        #region Properties

        public int Count
        {
            get { return m_spells.Count; }
        }

        public Spell2 this[int index]
        {
            get { return m_spells[index]; }
        }

        /// <summary>
        /// Returns whether the stack is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_spells.Count == 0; }
        }

        private IObjectController Controller
        {
            get { return m_game.Controller; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the top-most spell on the stack. Null if stack is empty.
        /// </summary>
        /// <returns></returns>
        public Spell2 Peek()
        {
            return m_spells.LastOrDefault();
        }

        /// <summary>
        /// Pushes a spell on the stack.
        /// </summary>
        /// <param name="spell"></param>
        internal void Push(Spell2 spell)
        {
            Throw.IfNull(spell, "spell");
            Throw.InvalidArgumentIf(spell.Manager != m_game, "Cross-game operation", "spell");
            Controller.Execute(new AddCommand(m_spells.Count, spell));
        }

        /// <summary>
        /// Pops a spell from the stack.
        /// </summary>
        internal Spell2 Pop()
        {
            int index = m_spells.Count - 1;
            Throw.InvalidOperationIf(index < 0, "Stack is empty");

            Spell2 top = m_spells[index];
            Controller.Execute(new RemoveCommand(index));
            return top;
        }

        /// <summary>
        /// Removes a spell from the stack.
        /// </summary>
        internal bool Remove(Spell2 spell)
        {
            int index = m_spells.IndexOf(spell);
            if (index < 0)
                return false;

            Controller.Execute(new RemoveCommand(index));

            return true;
        }

        private void AddInternal(int index, Spell2 spell)
        {
            Debug.Assert(spell != null);
            Debug.Assert(spell.Manager == m_game);
            Debug.Assert(!m_spells.Contains(spell), "Spell cannot be pushed twice to the stack");

            m_spells.Insert(index, spell);
            OnCollectionChanged(new ChangedEventArgs(CollectionChangeAction.Add, spell, index));
        }

        private Spell2 RemoveInternal(int index)
        {
            Spell2 spell = m_spells[index];
            m_spells.RemoveAt(index);
            OnCollectionChanged(new ChangedEventArgs(CollectionChangeAction.Remove, spell, index));
            return spell;
        }

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            // To be consistent with Zone.ToString()
            return "[Zone: SpellStack]";
        }

        #endregion

        #region Events

        public event EventHandler<ChangedEventArgs> Changed;

        private void OnCollectionChanged(ChangedEventArgs e)
        {
            Changed.Raise(this, e);
        }

        #endregion

        #region IEnumerable<Spell> Members

        public IEnumerator<Spell2> GetEnumerator()
        {
            return m_spells.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Nested Types

        [Serializable]
        private abstract class SpellStackCommand : Command
        {
            protected static SpellStack2 GetSpellStack(ObjectManager manager)
            {
                return ((Game)manager).SpellStack2;
            }
        }

        [Serializable]
        private class RemoveCommand : SpellStackCommand
        {
            #region Variables

            private int m_index;

            [NonSerialized]
            private Spell2 m_removedSpell;

            #endregion

            #region Constructor

            public RemoveCommand(int index)
            {
                m_index = index;
            }

            #endregion

            #region Overrides of Command

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                m_removedSpell = GetSpellStack(manager).RemoveInternal(m_index);
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                Debug.Assert(m_removedSpell != null);

                GetSpellStack(manager).AddInternal(m_index, m_removedSpell);
            }

            #endregion
        }

        [Serializable]
        private class AddCommand : SpellStackCommand
        {
            #region Variables

            private int m_index;
            private Resolvable<Spell2> m_spell;

            #endregion

            #region Constructor

            public AddCommand(int index, Spell2 spell)
            {
                m_index = index;
                m_spell = spell;
            }

            #endregion

            #region Methods

            private Spell2 GetSpell(ObjectManager manager)
            {
                return m_spell.Resolve(manager);
            }

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                GetSpellStack(manager).AddInternal(m_index, GetSpell(manager));
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                GetSpellStack(manager).RemoveInternal(m_index);
            }

            #endregion
        }

        public class ChangedEventArgs : EventArgs
        {
            public ChangedEventArgs(CollectionChangeAction action, Spell2 spell, int index)
            {
                Action = action;
                Spell = spell;
                Index = index;
            }

            public CollectionChangeAction Action { get; }
            public Spell2 Spell { get; }
            public int Index { get; }
        }

        #endregion
    }
}
