using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mox.UI.Shell
{
    public interface IShellViewModel
    {
        void Push(object viewModel);
    }
}
