using System.ComponentModel;
using Mox.Database;

namespace Mox.UI.Browser
{
    public interface IDeckViewModelEditor : INotifyPropertyChanged
    {
        CardDatabase Database { get; }
        IMasterCardFactory CardFactory { get; }

        string UserName { get; }
    }
}