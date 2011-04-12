using System;

namespace Mox.UI.Shell
{
    public interface IShellViewModel : INavigationConductor
    {
        IPageHandle Push(object viewModel);
    }
}
