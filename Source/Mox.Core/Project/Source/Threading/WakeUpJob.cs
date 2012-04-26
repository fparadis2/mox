using System;
using System.Threading;
using System.Windows.Threading;

namespace Mox.Threading
{
    public abstract class WakeUpJob
    {
        #region Variables

        private Action m_action;
        private State m_state = State.Stopped;

        #endregion

        #region Properties

        public Action Action
        {
            get { return m_action ?? DoNothing; }
            set { m_action = value; }
        }

        #endregion

        #region Methods

        public void WakeUp()
        {
            State oldState = DoAndSetState(DoNothing, State.Running);

            if (!oldState.IsRunning)
            {
                QueueUserWorkItem(Execute);
            }
        }

        private void Execute(object dummy)
        {
            DoAndSetState(Action, State.Stopped);
        }

        private State DoAndSetState(Action action, State newState)
        {
            State oldState;
            do
            {
                oldState = m_state;
                action();
            }
            while (!AssignNewState(oldState, newState));

            return oldState;
        }

        private static void DoNothing()
        { }

        private bool AssignNewState(State currentState, State newState)
        {
            return Interlocked.CompareExchange(ref m_state, newState, currentState) == currentState;
        }

        #endregion

        #region Abstract

        protected abstract void QueueUserWorkItem(WaitCallback waitCallBack);

        #endregion

        #region Creation

        public static WakeUpJob FromThreadPool()
        {
            return new ThreadPoolWakeUpJob();
        }

        public static WakeUpJob FromDispatcher(Dispatcher currentDispatcher)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Nested

        private class ThreadPoolWakeUpJob : WakeUpJob
        {
            protected override void QueueUserWorkItem(WaitCallback waitCallBack)
            {
                ThreadPool.QueueUserWorkItem(waitCallBack);
            }
        }

        private class State
        {
            private readonly bool m_isRunning;

            private State(bool isRunning)
            {
                m_isRunning = isRunning;
            }

            public bool IsRunning
            {
                get { return m_isRunning; }
            }

            public static State Running
            {
                get
                {
                    // Need to return a new object since we dont want two requests to be considered equals.
                    return new State(true);
                }
            }

            public static State Stopped
            {
                get
                {
                    return new State(false);
                }
            }
        }

        #endregion
    }
}
