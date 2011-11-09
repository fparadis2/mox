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
using System.Diagnostics;

namespace Mox
{
    /// <summary>
    /// Acts as a reference to an object, which can be resolved for other managers.
    /// </summary>
    /// <typeparam name="TObject"></typeparam>
    [Serializable]
    public struct Resolvable<TObject>
        where TObject : class, IObject
    {
        #region Variables

        private readonly int m_identifier;

        #endregion

        #region Constructor

        public Resolvable(TObject obj)
        {
            Debug.Assert(obj != null);
            m_identifier = obj.Identifier;
        }

        private Resolvable(int identifier)
        {
            m_identifier = identifier;
        }

        #endregion

        #region Properties

        public bool IsEmpty
        {
            get { return m_identifier == ObjectManager.InvalidIdentifier; }
        }

        public int Identifier
        {
            get { return m_identifier; }
        }

        #endregion

        #region Methods

        #region Public methods

        /// <summary>
        /// Resolves the current resolvable into the given <paramref name="objectManager"/>.
        /// </summary>
        /// <param name="objectManager"></param>
        /// <returns></returns>
        public TObject Resolve(ObjectManager objectManager)
        {
            Debug.Assert(!IsEmpty);
            return Resolve(objectManager, m_identifier);
        }

        /// <summary>
        /// Returns true if this resolvable resolves to the given object (faster than resolving & comparing).
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Is(TObject obj)
        {
            return obj.Identifier == m_identifier;
        }

        public Resolvable<U> Cast<U>()
            where U : class, TObject
        {
            return new Resolvable<U>(m_identifier);
        }

        /// <summary>
        /// Resolves the object identified by the given <paramref name="objectIdentifier"/> into the given <paramref name="objectManager"/>.
        /// </summary>
        /// <param name="objectManager"></param>
        /// <param name="objectIdentifier"></param>
        /// <returns></returns>
        public static TObject Resolve(ObjectManager objectManager, int objectIdentifier)
        {
            Debug.Assert(objectManager != null);
            return objectManager.GetObjectByIdentifier<TObject>(objectIdentifier);
        }

        /// <summary>
        /// Resolves the given <paramref name="obj"/> into the given <paramref name="objectManager"/>.
        /// </summary>
        /// <param name="objectManager"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static TObject Resolve(ObjectManager objectManager, TObject obj)
        {
            Debug.Assert(objectManager != null);
            Debug.Assert(obj != null);

            if (obj.Manager == objectManager)
            {
                return obj;
            }

            return Resolve(objectManager, obj.Identifier);
        }

        #endregion

        #region Conversions

        public static implicit operator Resolvable<TObject>(TObject obj)
        {
            return new Resolvable<TObject>(obj);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return string.Format("[Resolvable: {0}]", m_identifier);
        }

        #endregion

        #endregion
    }
}
