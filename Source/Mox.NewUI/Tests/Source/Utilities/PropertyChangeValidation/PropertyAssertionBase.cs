using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mox.UI
{
    public abstract class PropertyAssertionBase<T> 
        where T : class, INotifyPropertyChanged
    {
        #region Variables

        private readonly INotifyPropertyChanged m_propertyOwner;
        private readonly IDictionary<PropertyInfo, object> m_values = new Dictionary<PropertyInfo, object>();

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyAssertionBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="propertyOwner">The property owner.</param>
        protected PropertyAssertionBase(INotifyPropertyChanged propertyOwner)
        {
            m_propertyOwner = propertyOwner;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the values to set.
        /// </summary>
        /// <value>The values.</value>
        protected IDictionary<PropertyInfo, object> Values
        {
            get { return m_values; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the constructed set of assertions.
        /// </summary>
        protected void Execute()
        {
            var candidates = GetCandidateProperties();

            AssertThatClassHasCandidateProperties(candidates);

            var failures = candidates.Where(p => !DoesPropertyRaiseNotification(p)).ToList();

            if (failures.Any())
            {
                var msg = string.Format("{1}The following properties on {0} did not raise change notification:{1}", typeof (T).Name, Environment.NewLine);

                failures.ForEach(failure =>
                {
                    var setValue = GetSetterValueForProperty(failure);
                    msg += string.Format("\t{0} with the value set to '{1}'{2}", failure.Name, setValue, Environment.NewLine);
                });

                Assert.Fail(msg);
            }
        }

        protected abstract IEnumerable<PropertyInfo> GetCandidateProperties();

        private static void AssertThatClassHasCandidateProperties(IEnumerable<PropertyInfo> candidates)
        {
            if (candidates.Any())
            {
                return;
            }
            
            var msg = string.Format("{1}{0} does not have any public properties with setters.{1}Asserting change notification is without meaning.", typeof(T).Name, Environment.NewLine);
            Assert.Fail(msg);
        }

        private bool DoesPropertyRaiseNotification(PropertyInfo propertyInfo)
        {
            var sink = DoesPropertyRaiseNotification(() =>
            {
                var valueToSet = GetSetterValueForProperty(propertyInfo);
                propertyInfo.SetValue(m_propertyOwner, valueToSet, null);
                Assert.AreEqual(valueToSet, propertyInfo.GetValue(m_propertyOwner, null));
            });

            return sink.TimesCalled == 1 && sink.LastEventArgs.PropertyName == propertyInfo.Name;
        }

        protected EventSink<System.ComponentModel.PropertyChangedEventArgs> DoesPropertyRaiseNotification(System.Action action)
        {
            EventSink<System.ComponentModel.PropertyChangedEventArgs> sink = new EventSink<System.ComponentModel.PropertyChangedEventArgs>(m_propertyOwner);

            try
            {
                m_propertyOwner.PropertyChanged += sink.Handler;

                action();
            }
            finally
            {
                m_propertyOwner.PropertyChanged -= sink.Handler;
            }

            return sink;
        }

        private object GetSetterValueForProperty(PropertyInfo propertyInfo)
        {
            object value;
            if (!m_values.TryGetValue(propertyInfo, out value))
            {
                value = GenerateDefaultValue(propertyInfo);
                m_values.Add(propertyInfo, value);
            }
            return value;
        }

        private object GenerateDefaultValue(PropertyInfo propertyInfo)
        {
            if (typeof(bool).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return !((bool)propertyInfo.GetValue(m_propertyOwner, null));
            }

            if (typeof(string).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return "This is the test string used to test out property change notifications";
            }

            if (typeof(DateTime).IsAssignableFrom(propertyInfo.PropertyType))
            {
                return DateTime.Now.Add(TimeSpan.FromDays(10));
            }

            if (typeof(IConvertible).IsAssignableFrom(propertyInfo.PropertyType))
            {
                try
                {
                    return Convert.ChangeType(42, propertyInfo.PropertyType);
                }
                catch
                {
                }
            }

            if (propertyInfo.PropertyType.IsValueType)
            {
                return Activator.CreateInstance(propertyInfo.PropertyType);
            }

            return null;
        }

        protected static PropertyInfo GetPropertyInfo<TT, K>(Expression<Func<TT, K>> property)
        {
            var memberExpression = (MemberExpression)property.Body;
            return (PropertyInfo)memberExpression.Member;
        }

        #endregion
    }
}