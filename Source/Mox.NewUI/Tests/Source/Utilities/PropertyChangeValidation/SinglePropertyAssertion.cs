using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mox.UI
{
    public class SinglePropertyAssertion<T> : PropertyAssertionBase<T>
        where T : class, INotifyPropertyChanged
    {
        #region Variables

        private readonly List<PropertyInfo> m_properties = new List<PropertyInfo>();

        #endregion

        #region Constructor

        internal SinglePropertyAssertion(INotifyPropertyChanged propertyOwner)
            : base(propertyOwner)
        {
        }

        #endregion

        #region Methods

        protected override IEnumerable<PropertyInfo> GetCandidateProperties()
        {
            return m_properties;
        }

        public SinglePropertyAssertion<T> SetValue(object valueToSet)
        {
            AddPropertyValue(m_properties.Last(), valueToSet);
            return this;
        }

        public SinglePropertyAssertion<T> AndProperty<K>(Expression<Func<T, K>> property)
        {
            m_properties.Add(GetPropertyInfo(property));
            return this;
        }

        public void RaisesChangeNotification()
        {
            Execute();
        }

        public void RaisesChangeNotificationWhen(System.Action action)
        {
            var properties = m_properties.Distinct().ToList();

            var sink = DoesPropertyRaiseNotification(action);

            foreach (var property in properties)
            {
                PropertyInfo localProperty = property;
                Assert.That(sink.EventArgs.Any(e => e.PropertyName == localProperty.Name), "Property '{0}' did not raise change notification", localProperty.Name);
            }
        }

        public void FailsValidation(string expectedError)
        {
            Assert.IsInstanceOf<IDataErrorInfo>(Owner);
            IDataErrorInfo errorInfo = (IDataErrorInfo)Owner;

            foreach (var property in m_properties)
            {
                Assert.AreEqual(expectedError, errorInfo[property.Name]);
            }
        }

        public void PassesValidation()
        {
            Assert.IsInstanceOf<IDataErrorInfo>(Owner);
            IDataErrorInfo errorInfo = (IDataErrorInfo)Owner;

            foreach (var property in m_properties)
            {
                Assert.IsNull(errorInfo[property.Name]);
            }
        }

        #endregion
    }
}