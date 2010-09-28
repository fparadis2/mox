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
using System.Windows;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;

namespace Mox.UI
{
    public class MockMessageService : IMessageService, IDisposable
    {
        #region Variables

        private readonly IMessageService m_subInstance;
        private IDisposable m_singletonHandle;

        #endregion

        #region Constructor

        private MockMessageService(MockRepository mockery)
        {
            m_subInstance = mockery.StrictMock<IMessageService>();
        }

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_singletonHandle);
        }

        #endregion

        #region Methods

        #region Expectations

        public void Expect_Show(string text, string caption, MessageBoxButton button, MessageBoxResult result)
        {
            Expect
                .Call(m_subInstance.Show(text, caption, button, MessageBoxImage.None, result))
                .IgnoreArguments()
                .Constraints(Is.Equal(text), Is.Equal(caption), Is.Equal(button), Is.Anything(), Is.Anything())
                .Return(result);
        }

        #endregion

        #region Implementation

        public MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult)
        {
            return m_subInstance.Show(text, caption, button, image, defaultResult);
        }

        #endregion

        public static MockMessageService Use(MockRepository mockery)
        {
            var instance = new MockMessageService(mockery);
            instance.m_singletonHandle = MessageService.Use(instance);
            return instance;
        }

        #endregion
    }
}
