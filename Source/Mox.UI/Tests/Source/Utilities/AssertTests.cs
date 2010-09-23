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
using System.ComponentModel;
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class AssertTests
    {
        #region INotifyPropertyChanged

        private class NotifyPropertyChangedObject : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;

            public void TriggerPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
                }
            }
        }

        [Test]
        public void TriggersPropertyChanged_fails_if_event_is_not_triggered()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.Throws<AssertionException>(() => Assert.TriggersPropertyChanged(obj, "Bla", () => { }));
        }

        [Test]
        public void TriggersPropertyChanged_fails_if_event_is_not_triggered_on_the_correct_property()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.Throws<AssertionException>(() => Assert.TriggersPropertyChanged(obj, "Red", () => obj.TriggerPropertyChanged("Blue")));
        }

        [Test]
        public void TriggersPropertyChanged_succeeds_if_event_is_triggered_at_least_once()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.DoesntThrow(() => Assert.TriggersPropertyChanged(obj, "Red", () =>
            {
                obj.TriggerPropertyChanged("Blue");
                obj.TriggerPropertyChanged("Red");
                obj.TriggerPropertyChanged("Red");
            }));
        }

        [Test]
        public void DoesntTriggerPropertyChanged_succeeds_if_event_is_not_triggered()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.DoesntThrow(() => Assert.DoesntTriggerPropertyChanged(obj, "Bla", () => { }));
        }

        [Test]
        public void DoesntTriggerPropertyChanged_succeeds_if_event_is_not_triggered_on_the_expected_property()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.DoesntThrow(() => Assert.DoesntTriggerPropertyChanged(obj, "Red", () => obj.TriggerPropertyChanged("Blue")));
        }

        [Test]
        public void DoesntTriggerPropertyChanged_fails_if_event_is_triggered_at_least_once()
        {
            NotifyPropertyChangedObject obj = new NotifyPropertyChangedObject();
            Assert.Throws<AssertionException>(() => Assert.DoesntTriggerPropertyChanged(obj, "Red", () => obj.TriggerPropertyChanged("Red")));
        }

        #endregion
    }
}
