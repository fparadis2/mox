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

namespace Mox.UI
{
    public class Assert : Mox.Assert
    {
        #region INotifyPropertyChanged

        public static void TriggersPropertyChanged(INotifyPropertyChanged @object, string expectedProperty, System.Action operation)
        {
            EventSink<System.ComponentModel.PropertyChangedEventArgs> sink = new EventSink<System.ComponentModel.PropertyChangedEventArgs>(@object);

            int timesCalled = 0;

            sink.Callback += (o, e) =>
            {
                if (object.Equals(expectedProperty, e.PropertyName))
                {
                    timesCalled++;
                }
            };

            try
            {
                @object.PropertyChanged += sink.Handler;

                operation();

                Assert.Greater(timesCalled, 0, "Expected PropertyChanged event for property {0} to be triggered at least once.", expectedProperty);
            }
            finally
            {
                @object.PropertyChanged -= sink.Handler;
            }
        }

        public static void DoesntTriggerPropertyChanged(INotifyPropertyChanged @object, string expectedProperty, System.Action operation)
        {
            EventSink<System.ComponentModel.PropertyChangedEventArgs> sink = new EventSink<System.ComponentModel.PropertyChangedEventArgs>(@object);

            sink.Callback += (o, e) =>
            {
                if (object.Equals(expectedProperty, e.PropertyName))
                {
                    Assert.Fail("Did not expect PropertyChanged for property {0} to be triggered", expectedProperty);
                }
            };

            try
            {
                @object.PropertyChanged += sink.Handler;

                operation();
            }
            finally
            {
                @object.PropertyChanged -= sink.Handler;
            }
        }

        #endregion
    }
}
