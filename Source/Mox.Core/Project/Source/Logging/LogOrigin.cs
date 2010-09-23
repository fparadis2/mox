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

namespace Mox
{
    /// <summary>
    /// The origin of a log message.
    /// </summary>
    [Serializable]
    public struct LogOrigin
    {
        #region Properties

        /// <summary>
        /// Source of the origin (usually a filename).
        /// </summary>
        public string Source
        {
            get;
            set;
        }

        /// <summary>
        /// Line number in the <see cref="Source"/>.
        /// </summary>
        public int Line
        {
            get;
            set;
        }

        /// <summary>
        /// Column number in the <see cref="Source"/>.
        /// </summary>
        public int Column
        {
            get;
            set;
        }

        /// <summary>
        /// End Line number in the <see cref="Source"/>.
        /// </summary>
        public int EndLine
        {
            get;
            set;
        }

        /// <summary>
        /// End Column number in the <see cref="Source"/>.
        /// </summary>
        public int EndColumn
        {
            get;
            set;
        }

        /// <summary>
        /// Empty origin.
        /// </summary>
        public static LogOrigin Empty
        {
            get { return new LogOrigin(); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Transforms the origin into its canonical string format.
        /// </summary>
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(Source);
            if (builder.Length > 0)
            {
                if (Line != 0)
                {
                    if (Column == 0)
                    {
                        if (EndLine == 0)
                        {
                            builder.AppendFormat("({0})", Line);
                        }
                        else
                        {
                            builder.AppendFormat("({0}-{1})", Line, EndLine);
                        }
                    }
                    else if (EndLine == 0)
                    {
                        if (EndColumn == 0)
                        {
                            builder.AppendFormat("({0},{1})", Line, Column);
                        }
                        else
                        {
                            builder.AppendFormat("({0},{1}-{2})", Line, Column, EndColumn);
                        }
                    }
                    else if (EndColumn == 0)
                    {
                        builder.AppendFormat("({0}-{1},{2})", Line, EndLine, Column);
                    }
                    else
                    {
                        builder.AppendFormat("({0},{1},{2},{3})", Line, Column, EndLine, EndColumn);
                    }
                }
            }

            return builder.ToString();
        }

        #endregion
    }
}
