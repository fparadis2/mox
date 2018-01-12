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
    public class SpellStack2 : IEnumerable<Spell2>
    {
        #region Variables

        private readonly Game m_game;
        private Stack<Spell2> m_stack = new Stack<Spell2>();

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
            get { return m_stack.Count; }
        }

        /// <summary>
        /// Returns whether the stack is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_stack.Count == 0; }
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
            return m_stack.Peek();
        }

        /// <summary>
        /// Pushes a spell on the stack.
        /// </summary>
        /// <param name="spell"></param>
        internal void Push(Spell2 spell)
        {
            Throw.IfNull(spell, "spell");
            Throw.InvalidArgumentIf(spell.Manager != m_game, "Cross-game operation", "spell");
            Controller.Execute(new PushCommand(spell));
        }

        private void PushInternal(Spell2 spell)
        {
            Debug.Assert(spell != null);
            Debug.Assert(spell.Manager == m_game);
            Debug.Assert(!m_stack.Contains(spell), "Spell cannot be pushed twice to the stack");

            m_stack.Push(spell);
            OnCollectionChanged(new CollectionChangedEventArgs<Spell2>(CollectionChangeAction.Add, new[] { spell }));
        }

        /// <summary>
        /// Pops a spell from the stack.
        /// </summary>
        internal Spell2 Pop()
        {
            Spell2 top = Peek();
            Controller.Execute(new PopCommand());
            return top;
        }

        private Spell2 PopInternal()
        {
            Spell2 top = m_stack.Pop();
            OnCollectionChanged(new CollectionChangedEventArgs<Spell2>(CollectionChangeAction.Remove, new[] { top }));
            return top;
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

        public event EventHandler<CollectionChangedEventArgs<Spell2>> CollectionChanged;

        private void OnCollectionChanged(CollectionChangedEventArgs<Spell2> e)
        {
            CollectionChanged.Raise(this, e);
        }

        #endregion

        #region IEnumerable<Spell> Members

        public IEnumerator<Spell2> GetEnumerator()
        {
            return m_stack.GetEnumerator();
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
        private class PopCommand : SpellStackCommand
        {
            #region Variables

            [NonSerialized]
            private Spell2 m_removedSpell;

            #endregion

            #region Overrides of Command

            /// <summary>
            ///  Executes (does or redoes) the command.
            /// </summary>
            public override void Execute(ObjectManager manager)
            {
                m_removedSpell = GetSpellStack(manager).PopInternal();
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                Debug.Assert(m_removedSpell != null);

                GetSpellStack(manager).PushInternal(m_removedSpell);
            }

            #endregion
        }

        [Serializable]
        private class PushCommand : SpellStackCommand
        {
            #region Variables

            private Resolvable<Spell2> m_spell;

            #endregion

            #region Constructor

            public PushCommand(Spell2 spell)
            {
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
                GetSpellStack(manager).PushInternal(GetSpell(manager));
            }

            /// <summary>
            ///  Unexecutes (undoes) the command.
            /// </summary>
            public override void Unexecute(ObjectManager manager)
            {
                GetSpellStack(manager).PopInternal();
            }

            #endregion
        }

        #endregion
    }
}
