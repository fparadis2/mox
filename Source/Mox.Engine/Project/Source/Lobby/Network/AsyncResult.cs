﻿using System;
using System.Diagnostics;
using System.Threading;

namespace Mox.Lobby
{
    internal abstract class AsyncResult
    {
        #region Variables

        private bool m_isCompleted;
        private bool m_isErroneous;

        private ManualResetEvent m_completedEvent;

        #endregion

        #region Properties

        public bool IsCompleted
        {
            get { return m_isCompleted; }
        }

        public bool IsErroneous
        {
            get 
            { 
                WaitForCompletion();
                return m_isErroneous;
            }
        }

        #endregion

        #region Methods

        internal virtual void Complete(object value)
        {
            OnComplete();
        }

        internal void MakeErroneous()
        {
            m_isErroneous = true;
            OnComplete();
        }

        protected void WaitForCompletion()
        {
            if (m_isCompleted)
            {
                return;
            }

            lock (this)
            {
                if (m_isCompleted)
                {
                    return;
                }

                if (m_completedEvent == null)
                {
                    m_completedEvent = new ManualResetEvent(false);
                }
            }

            Debug.Assert(m_completedEvent != null);
            m_completedEvent.WaitOne();
        }

        private void OnComplete()
        {
            m_isCompleted = true;

            lock (this)
            {
                if (m_completedEvent != null)
                {
                    m_completedEvent.Set();
                }
            }

            OnCompleted();
        }

        #endregion

        #region Events

        private event System.Action CompletedImpl;

        public event System.Action Completed
        {
            add
            {
                if (!m_isCompleted)
                {
                    lock (this)
                    {
                        if (!m_isCompleted)
                        {
                            CompletedImpl += value;
                            return;
                        }
                    }
                }

                value();
            }
            remove
            {
                CompletedImpl -= value;
            }
        }

        private void OnCompleted()
        {
            var completed = CompletedImpl;
            if (completed != null)
            {
                completed();
            }
        }

        #endregion
    }

    internal class AsyncResult<T> : AsyncResult, IAsyncResult<T>
    {
        #region Variables

        private T m_value;

        #endregion

        #region Properties

        object IAsyncResult.Value
        {
            get { return Value; }
        }

        public T Value
        {
            get
            {
                WaitForCompletion();
                return m_value;
            }
        }

        #endregion

        #region Methods

        internal override void Complete(object value)
        {
            m_value = (T)value;
            base.Complete(value);
        }

        #endregion
    }
}
