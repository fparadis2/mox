using System;
using System.Collections.Generic;

namespace Mox.Flow
{
    /// <summary>
    /// Makes the bridge between an <see cref="IChoiceDecisionMaker"/> and a <see cref="IClientInput"/>.
    /// </summary>
    public class ClientInputRouter : IChoiceDecisionMaker
    {
        #region Variables

        private readonly Game m_game;
        private readonly IClientInput m_clientInput;
        private static readonly Dictionary<System.Type, Dispatcher> ms_dispatchers = new Dictionary<System.Type, Dispatcher>();

        #endregion

        #region Constructor

        static ClientInputRouter()
        {
            ms_dispatchers.Add(typeof(ModalChoice), (i, g, c) => i.AskModalChoice(((ModalChoice)c).Context));
            ms_dispatchers.Add(typeof(GivePriorityChoice), (i, g, c) => i.GivePriority());
            ms_dispatchers.Add(typeof(PayManaChoice), (i, g, c) => i.PayMana(((PayManaChoice)c).ManaCost));
            ms_dispatchers.Add(typeof(MulliganChoice), (i, g, c) => i.Mulligan());
            ms_dispatchers.Add(typeof(TargetChoice), (i, g, c) => i.Target(((TargetChoice)c).Context));
            ms_dispatchers.Add(typeof(DeclareAttackersChoice), (i, g, c) => i.DeclareAttackers(((DeclareAttackersChoice)c).AttackContext));
            ms_dispatchers.Add(typeof(DeclareBlockersChoice), (i, g, c) => i.DeclareBlockers(((DeclareBlockersChoice)c).BlockContext));
        }

        public ClientInputRouter(Game game, IClientInput clientInput)
        {
            Throw.IfNull(game, "game");
            Throw.IfNull(clientInput, "clientInput");

            m_game = game;
            m_clientInput = clientInput;
        }

        #endregion

        #region Implementation of IChoiceDecisionMaker

        public object MakeChoiceDecision(Sequencer sequencer, Choice choice)
        {
            Dispatcher dispatcher;
            if (!ms_dispatchers.TryGetValue(choice.GetType(), out dispatcher))
            {
                throw new NotImplementedException(string.Format("Unknown choice: {0}", choice));
            }

            return dispatcher(m_clientInput, m_game, choice);
        }

        #endregion

        #region Inner Types

        private delegate object Dispatcher(IClientInput controller, Game game, Choice choice);

        #endregion
    }
}