using System.ComponentModel;
using Mox.Database;

namespace Mox.UI.Browser
{
    public interface IDeckViewModelEditor : INotifyPropertyChanged
    {
        CardDatabase Database { get; }
        IMasterCardFactory CardFactory { get; }

        bool IsDirty { get; set; }
        bool IsEnabled { get; set; }

        string UserName { get; }

        IDeckViewModelEditor Clone();
    }
}