using System;

namespace Mox.UI
{
    public interface INavigationConductor
    {
        void Pop();
    }

    public interface INavigationConductor<in TViewModel> : INavigationConductor
    {
        void Push(TViewModel viewModel);
    }
}
