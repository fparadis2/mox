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
using System.Text;
using System.Globalization;

namespace Mox
{
    /// <summary>
    /// A log message.
    /// </summary>
    [Serializable]
    public struct LogMessage
    {
        #region Constants

        private const string ErrorText = "error";
        private const string WarningText = "warning";
        private const string MessageText = "message";

        private static readonly string[] NewLines = new string[] { "\r\n", "\n" };

        #endregion

        #region Properties

        /// <summary>
        /// Origin of the message, if any.
        /// </summary>
        public LogOrigin Origin
        {
            get;
            set;
        }

        /// <summary>
        /// Text associated with the message, if any.
        /// </summary>
        public string Text
        {
            get;
            set;
        }

        /// <summary>
        /// Importance of the message.
        /// </summary>
        public LogImportance Importance
        {
            get;
            set;
        }

        /// <summary>
        /// Sub category of the message, if any.
        /// </summary>
        public string SubCategory
        {
            get;
            set;
        }

        /// <summary>
        /// Code of the message, if any.
        /// </summary>
        public string Code
        {
            get;
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriden.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder lineBuilder = new StringBuilder();

            string origin = Origin.ToString();
            if (!string.IsNullOrEmpty(origin))
            {
                lineBuilder.Append("{0}: "); // File location
            }

            if (!string.IsNullOrEmpty(SubCategory))
            {
                lineBuilder.Append("{4} "); // Subcategory
            }

            lineBuilder.Append("{1} "); // Category
            lineBuilder.Append("{2}: "); // Code
            lineBuilder.Append("{3}"); // Message

            string lineFormat = lineBuilder.ToString();
            string[] messageLines = SplitStringOnNewLines(Text ?? string.Empty);
            StringBuilder finalBuilder = new StringBuilder();
            for (int numLine = 0; numLine < messageLines.Length; numLine++)
            {
                finalBuilder.Append(string.Format(CultureInfo.InvariantCulture, lineFormat, new object[] { origin, GetCategoryString(), Code, messageLines[numLine], SubCategory }));
                if (numLine < (messageLines.Length - 1))
                {
                    finalBuilder.AppendLine();
                }
            }
            return finalBuilder.ToString();
        }

        private string GetCategoryString()
        {
            switch (Importance)
            {
                case LogImportance.Error:
                    return ErrorText;

                case LogImportance.Warning:
                    return WarningText;

                default:
                    return MessageText;
            }
        }

        private static string[] SplitStringOnNewLines(string str)
        {
            return str.Split(NewLines, StringSplitOptions.None);
        }

        #endregion
    }
}
