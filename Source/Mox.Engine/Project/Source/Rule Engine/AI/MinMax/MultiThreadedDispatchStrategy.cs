// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Threading;
using Mox.Replication;

namespace Mox.AI
{
    public sealed class MultiThreadedDispatchStrategy : IDispatchStrategy, IDisposable
    {
        #region Inner Types

        private class AIWorkerThread
        {
            #region Variables

            private readonly Thread m_thread;
            private readonly MultiThreadedDispatchStrategy m_owner;
            private readonly ReplicationClient<Game> m_client;

            private CountdownLatch m_finishedLatch;

            private readonly AutoResetEvent m_goEvent = new AutoResetEvent(false);
            private readonly ManualResetEvent m_stopEvent = new ManualResetEvent(false);

            #endregion

            #region Constructor

            public AIWorkerThread(MultiThreadedDispatchStrategy owner, ReplicationSource<Player> source, int index)
            {
                m_owner = owner;

                m_client = new ReplicationClient<Game>();
                source.Register(null, m_client);

                m_thread = new Thread(Run)
                {
                    Name = "AI Worker Thread #" + index,
                    IsBackground = true,
                    Priority = ThreadPriority.BelowNormal
                };

                m_thread.Start();
            }

            #endregion

            #region Properties

            private Game Game
            {
                get { return m_client.Host; }
            }

            private Queue<IWorkOrder> QueuedJobs
            {
                get { return m_owner.m_queuedJobs; }
            }

            #endregion

            #region Methods

            public void StartWork(CountdownLatch latch)
            {
                m_finishedLatch = latch;
                m_goEvent.Set();
            }

            public void Stop()
            {
                m_stopEvent.Set();
            }

            private void Run()
            {
                while (true)
                {
                    int theEvent = WaitHandle.WaitAny(new WaitHandle[] { m_goEvent, m_stopEvent });

                    if (theEvent == 1)
                    {
                        break;
                    }

                    try
                    {
                        IWorkOrder workOrder;
                        while ((workOrder = GetNextJob()) != null)
                        {
                            // Do the work
                            workOrder.Evaluate(Game);
                        }
                    }
                    finally
                    {
                        m_finishedLatch.Signal();
                    }
                }

                m_goEvent.Close();
                m_stopEvent.Close();
            }

            private IWorkOrder GetNextJob()
            {
                if (QueuedJobs.Count > 0)
                {
                    lock (QueuedJobs)
                    {
                        if (QueuedJobs.Count > 0)
                        {
                            return QueuedJobs.Dequeue();
                        }
                    }
                }

                return null;
            }

            #endregion
        }

        /// <summary>
        /// Used to circumvent the limitation on WaitHandle.WaitAll (MTA only)
        /// </summary>
        /// <remarks>
        /// Wait() will wait until the latch has been signaled N times.
        /// </remarks>
        private sealed class CountdownLatch : IDisposable
        {
            #region Variables

            private readonly ManualResetEvent m_event;
            private int m_remaining;

            #endregion

            #region Ctor

            public CountdownLatch(int count)
            {
                m_remaining = count;
                m_event = new ManualResetEvent(false);
            }

            public void Dispose()
            {
                m_event.Close();
            }

            #endregion

            #region Methods

            public void Signal()
            {
                // The last thread to signal also sets the event.
                if (Interlocked.Decrement(ref m_remaining) == 0)
                {
                    m_event.Set();
                }
            }

            public void Wait()
            {
                m_event.WaitOne();
            }

            #endregion
        }

        #endregion

        #region Variables

        private readonly Game m_game;
        private readonly ReplicationSource<Player> m_source;
        private readonly List<AIWorkerThread> m_workerThreads = new List<AIWorkerThread>();
        private readonly Queue<IWorkOrder> m_queuedJobs = new Queue<IWorkOrder>();

        #endregion

        #region Constructor

        public MultiThreadedDispatchStrategy(Game game)
        {
            m_game = game;
            m_source = new ReplicationSource<Player>(game, new OpenVisibilityStrategy<Player>());

            CreateWorkerThreads();
        }

        public void Dispose()
        {
            DeleteWorkerThreads();

            m_source.Dispose();
        }

        #endregion

        #region Methods

        public void Dispatch(IWorkOrder work)
        {
            lock (m_queuedJobs)
            {
                m_queuedJobs.Enqueue(work);
            }
        }

        public void Wait()
        {
            // Start the work and wait for results
            using (m_game.EnforceThreadAffinity()) // Helps detect (bad) access to game on main thread
            using (CountdownLatch latch = new CountdownLatch(m_workerThreads.Count))
            {
                m_workerThreads.ForEach(workerThread => workerThread.StartWork(latch));
                latch.Wait();
            }
        }

        private void CreateWorkerThreads()
        {
            int numThreads = Environment.ProcessorCount;

            for (int i = 0; i < numThreads; i++)
            {
                m_workerThreads.Add(new AIWorkerThread(this, m_source, i));
            }
        }

        private void DeleteWorkerThreads()
        {
            m_workerThreads.ForEach(thread => thread.Stop());
        }

        #endregion
    }
}