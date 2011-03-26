using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Mox.UI
{
    public class SinglePropertyAssertion<T, K> : PropertyAssertionBase<T>
        where T : class, INotifyPropertyChanged
    {
        #region Variables

        private readonly Expression<Func<T, K>> m_property;

        #endregion

        #region Constructor

        internal SinglePropertyAssertion(INotifyPropertyChanged propertyOwner, Expression<Func<T, K>> property)
            : base(propertyOwner)
        {
            this.m_property = property;
        }

        #endregion

        #region Methods

        protected override IEnumerable<PropertyInfo> GetCandidateProperties()
        {
            yield return GetPropertyInfo(m_property);
        }

        public SinglePropertyAssertion<T, K> SetValue(K valueToSet)
        {
            Values[GetPropertyInfo(m_property)] = valueToSet;
            return this;
        }

        public void RaisesChangeNotification()
        {
            Execute();
        }

        #endregion
    }
}