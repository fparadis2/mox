using System;
using System.Globalization;

namespace Mox.UI
{
    public class DateTimeOffset
    {
        #region Variables

        private readonly DateTime m_now;
        private readonly DateTime m_target;

        #endregion

        #region Constructor

        public DateTimeOffset(DateTime now, DateTime target)
        {
            m_now = now;
            m_target = target;
        }

        #endregion

        #region Properties

        private TimeSpan Span
        {
            get { return m_now.Subtract(m_target); }
        }

        #endregion

        #region Methods

        public string ToString(IFormatProvider formatProvider)
        {
            var timeSpan = Span;

            if (timeSpan < TimeSpan.FromMinutes(1))
            {
                return "just now";
            }
            if (timeSpan < TimeSpan.FromMinutes(2))
            {
                return "1 minute ago";
            }
            if (timeSpan < TimeSpan.FromHours(1))
            {
                return string.Format("{0} minutes ago", Math.Floor(timeSpan.TotalMinutes));
            }
            if (timeSpan < TimeSpan.FromHours(2))
            {
                return "1 hour ago";
            }
            if (timeSpan < TimeSpan.FromDays(1))
            {
                return string.Format("{0} hours ago", Math.Floor(timeSpan.TotalHours));
            }
            if (timeSpan < TimeSpan.FromDays(2))
            {
                return "yesterday";
            }

            return m_target.ToString("D", formatProvider);
        }

        public override string ToString()
        {
            return ToString(CultureInfo.CurrentUICulture);
        }

        #endregion
    }
}
