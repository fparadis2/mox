using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Mox.UI
{
    public interface IMoxScreen : IScreen
    {
        void Goto();
    }

    public class MoxScreen : Screen, IMoxScreen
    {
        public void Goto()
        {
            this.ActivateScreen();
        }
    }
}
