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
