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
using System.Collections.ObjectModel;
using Caliburn.Micro;

namespace Mox.UI.Game
{
    public class UserChoiceInteractionModel : PropertyChangedBase
    {
        #region Variables

        private string m_text;

        private readonly ObservableCollection<UserChoiceModel> m_choices = new ObservableCollection<UserChoiceModel>();

        #endregion

        #region Constructor

        public UserChoiceInteractionModel()
        {
        }

        public UserChoiceInteractionModel(params UserChoiceModel[] choices)
            : this()
        {
            choices.ForEach(Choices.Add);
        }

        #endregion

        #region Properties

        public string Text
        {
            get { return m_text; }
            set
            {
                if (m_text != value)
                {
                    m_text = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public ObservableCollection<UserChoiceModel> Choices
        {
            get { return m_choices; }
        }

        #endregion

        #region Static constructors

        #region Yes/No

        /// <summary>
        /// Constructs a simple yes/no user choice.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static UserChoiceInteractionModel YesNo(string text)
        {
            UserChoiceInteractionModel model = new UserChoiceInteractionModel { Text = text };

            model.Choices.Add(new UserChoiceModel { Text = "Yes", Type = UserChoiceType.Yes });
            model.Choices.Add(new UserChoiceModel { Text = "No", Type = UserChoiceType.No });

            return model;
        }

        #endregion

        #region Cancel

        /// <summary>
        /// Constructs a cancel user choice.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static UserChoiceInteractionModel Cancel(string text)
        {
            return Cancel(text, "Cancel");
        }

        /// <summary>
        /// Constructs a cancel user choice.
        /// </summary>
        /// <returns></returns>
        public static UserChoiceInteractionModel Cancel(string text, string choiceText)
        {
            UserChoiceInteractionModel model = new UserChoiceInteractionModel { Text = text };

            model.Choices.Add(new UserChoiceModel { Text = choiceText, Type = UserChoiceType.Cancel });

            return model;
        }

        #endregion

        #endregion
    }

    public class UserChoiceInteractionModel_DesignTime : UserChoiceInteractionModel
    {
        #region Constructor

        public UserChoiceInteractionModel_DesignTime()
        {
            Text = "Hey, this is some message about what's happening in the game. What do you want do do?";

            Choices.Add(new UserChoiceModel { Text = "Continue" });
            Choices.Add(new UserChoiceModel { Text = "Pass" });
            Choices.Add(new UserChoiceModel { Text = "No" });
        }

        #endregion
    }
}
