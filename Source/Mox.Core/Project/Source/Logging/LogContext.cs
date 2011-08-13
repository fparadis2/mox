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

namespace Mox
{
    /// <summary>
    /// A logging context.
    /// </summary>
    public class LogContext : ILog
    {
        #region Variables

        private static readonly ILog ms_empty = new EmptyLog();

        private readonly List<LogMessage> m_allMessages = new List<LogMessage>();
        private readonly List<LogMessage> m_messages = new List<LogMessage>();
        private readonly List<LogMessage> m_errors = new List<LogMessage>();
        private readonly List<LogMessage> m_warnings = new List<LogMessage>();

        #endregion

        #region Properties

        /// <summary>
        /// All messages logged into this context.
        /// </summary>
        public IList<LogMessage> AllMessages
        {
            get { return m_allMessages.AsReadOnly(); }
        }

        /// <summary>
        /// Simple messages logged into this context (i.e. non error/warning messages)
        /// </summary>
        public IList<LogMessage> Messages
        {
            get { return m_messages.AsReadOnly(); }
        }

        /// <summary>
        /// Errors logged into this context.
        /// </summary>
        public IList<LogMessage> Errors
        {
            get { return m_errors.AsReadOnly(); }
        }

        /// <summary>
        /// Warnings logged into this context.
        /// </summary>
        public IList<LogMessage> Warnings
        {
            get { return m_warnings.AsReadOnly(); }
        }

        public static ILog Empty
        {
            get { return ms_empty; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Logs the given <paramref name="message"/>.
        /// </summary>
        public void Log(LogMessage message)
        {
            m_allMessages.Add(message);

            ICollection<LogMessage> specificCollection;
            switch (message.Importance)
            {
                case LogImportance.Error:
                    specificCollection = m_errors; break;

                case LogImportance.Warning:
                    specificCollection = m_warnings; break;

                default:
                    specificCollection = m_messages; break;
            }
            System.Diagnostics.Debug.Assert(specificCollection != null);
            specificCollection.Add(message);
        }

        #endregion

        #region Inner Types

        private class EmptyLog : ILog
        {
            public void Log(LogMessage message)
            {
            }
        }

        #endregion
    }
}
