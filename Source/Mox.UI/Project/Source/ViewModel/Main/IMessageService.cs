using System;
using System.Windows;

namespace Mox.UI
{
    public interface IMessageService
    {
        #region Methods

        MessageBoxResult Show(string text, string caption, MessageBoxButton button, MessageBoxImage image, MessageBoxResult defaultResult);

        #endregion
    }
}