using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Mox.UI
{
    public class DataErrorInfo : IDataErrorInfo
    {
        #region Variables

        private readonly Dictionary<string, string> m_errors = new Dictionary<string, string>();

        #endregion

        #region Properties

        public string this[string columnName]
        {
            get
            {
                string error;
                m_errors.TryGetValue(columnName, out error);
                return error;
            }
        }

        public string Error
        {
            get { return m_errors.Values.FirstOrDefault(); }
        }

        #endregion

        #region Methods

        public void SetError<T>(Expression<Func<T>> property, string error)
        {
            string propertyName = GetPropertyName(property);

            if (string.IsNullOrEmpty(error))
            {
                m_errors.Remove(propertyName);
            }
            else
            {
                m_errors[propertyName] = error;
            }
        }

        private static string GetPropertyName<T>(Expression<Func<T>> property)
        {
            var memberExpression = (MemberExpression)property.Body;
            return memberExpression.Member.Name;
        }

        #endregion
    }
}
