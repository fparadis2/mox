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
using System.Diagnostics;

namespace Mox.Flow
{
    [Serializable]
    public class ModalChoiceContext
    {
        #region Variables

        private readonly List<ModalChoiceResult> m_choices = new List<ModalChoiceResult>();

        #endregion

        #region Properties

        /// <summary>
        /// The choices available.
        /// </summary>
        public IList<ModalChoiceResult> Choices
        {
            get { return m_choices; }
        }

        /// <summary>
        /// The question to ask.
        /// </summary>
        public string Question
        {
            get;
            set;
        }

        /// <summary>
        /// The default choice to propose.
        /// </summary>
        public ModalChoiceResult DefaultChoice
        {
            get;
            set;
        }

        public ModalChoiceImportance Importance
        {
            get;
            set;
        }

        #endregion

        #region Static creation

        #region Yes/No

        /// <summary>
        /// Creates a context for a Yes/No question
        /// </summary>
        /// <param name="question"></param>
        /// <param name="defaultChoice"></param>
        /// <returns></returns>
        public static ModalChoiceContext YesNo(string question, ModalChoiceResult defaultChoice)
        {
            return YesNo(question, defaultChoice, ModalChoiceImportance.Important);
        }

        /// <summary>
        /// Creates a context for a Yes/No question
        /// </summary>
        /// <param name="question"></param>
        /// <param name="defaultChoice"></param>
        /// <param name="importance"></param>
        /// <returns></returns>
        public static ModalChoiceContext YesNo(string question, ModalChoiceResult defaultChoice, ModalChoiceImportance importance)
        {
            Debug.Assert(defaultChoice == ModalChoiceResult.Yes || defaultChoice == ModalChoiceResult.No, "Invalid default choice");

            ModalChoiceContext context = new ModalChoiceContext
            {
                Question = question,
                DefaultChoice = defaultChoice,
                Importance = importance,
            };

            // Always put the default choice before other choices because AI performance can be better if choices more likely to be good are first.
            switch (defaultChoice)
            {
                case ModalChoiceResult.No:
                    context.Choices.Add(ModalChoiceResult.No);
                    context.Choices.Add(ModalChoiceResult.Yes);
                    break;

                case ModalChoiceResult.Yes:
                    context.Choices.Add(ModalChoiceResult.Yes);
                    context.Choices.Add(ModalChoiceResult.No);
                    break;

                default:
                    throw new NotImplementedException();
            }

            return context;
        }

        #endregion

        #endregion
    }

    public enum ModalChoiceResult
    {
        Yes,
        No,
    }

    public enum ModalChoiceImportance
    {
        Critical,
        Important,
        Trivial,
    }
}
