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
using System.ComponentModel;
using System.Diagnostics;

namespace Mox.UI
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Can be used to notify bindings that all the object has possibly changed.
        /// </summary>
        protected void NotifyChangedCompletely()
        {
            OnPropertyChanged(null);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);
            
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                // Verify that the property name matches a real,
                // public, instance property on this object.
                Throw.IfNull(TypeDescriptor.GetProperties(this)[propertyName], string.Format("Invalid property: {0}", propertyName));
            }
        }

        #endregion
    }
}
