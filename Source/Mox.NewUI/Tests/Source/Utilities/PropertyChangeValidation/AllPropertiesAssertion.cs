using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mox.UI
{
    public class AllPropertiesAssertion<T> : PropertyAssertionBase<T> where T : class, INotifyPropertyChanged
    {
        #region Variables

        private readonly IList<string> m_ignored = new List<string>();

        #endregion

        #region Constructor

        internal AllPropertiesAssertion(INotifyPropertyChanged propertyOwner) 
            : base(propertyOwner)
        {
            // PropertyChangedBase
            m_ignored.Add("IsNotifying");
        }

        #endregion

        #region Methods

        public AllPropertiesAssertion<T> Ignoring<K>(Expression<Func<T, K>> property)
        {
            m_ignored.Add(GetPropertyInfo(property).Name);
            return this;
        }

        protected override IEnumerable<PropertyInfo> GetCandidateProperties()
        {
            return from property in typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                   where !m_ignored.Contains(property.Name) && property.CanWrite
                   select property;
        }

        public AllPropertiesAssertion<T> SetValue<K>(Expression<Func<T, K>> property, K valueToSet)
        {
            Values[GetPropertyInfo(property)] = valueToSet;
            return this;
        }

        /// <summary>
        /// Raises the change notification.
        /// </summary>
        public void RaiseChangeNotification()
        {
            Execute();
        }

        #endregion
    }
}