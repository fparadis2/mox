using System;
using System.Diagnostics;
using System.Threading;

namespace Mox
{
    public class ReadWriteLock
    {
        #region Variables

        private readonly ReaderWriterLockSlim m_lock;
        private readonly IDisposable m_readHandle;
        private readonly IDisposable m_writeHandle;

        #endregion

        #region Constructor

        private ReadWriteLock(LockRecursionPolicy policy)
        {
            m_lock = new ReaderWriterLockSlim(policy);

            m_readHandle = new DisposableHelper(LeaveRead);
            m_writeHandle = new DisposableHelper(LeaveWrite);
        }

        #endregion

        #region Properties

        public IDisposable Write
        {
            get
            {
                EnterWrite();
                return m_writeHandle;
            }
        }

        public IDisposable Read
        {
            get 
            { 
                EnterRead();
                return m_readHandle;
            }
        }

        #endregion

        #region Methods

        private void EnterRead()
        {
            m_lock.EnterReadLock();
        }

        private void LeaveRead()
        {
            m_lock.ExitReadLock();
        }

        private void EnterWrite()
        {
            m_lock.EnterWriteLock();
        }

        private void LeaveWrite()
        {
            m_lock.ExitWriteLock();
        }

        public static ReadWriteLock Create()
        {
            return new ReadWriteLock(LockRecursionPolicy.SupportsRecursion);
        }

        public static ReadWriteLock CreateNoRecursion()
        {
            return new ReadWriteLock(LockRecursionPolicy.NoRecursion);
        }

        [Conditional("DEBUG")]
        public void AssertCanRead()
        {
            Throw.InvalidOperationIf(!m_lock.IsReadLockHeld, "Attempted to read data outside of read lock");
        }

        #endregion
    }
}