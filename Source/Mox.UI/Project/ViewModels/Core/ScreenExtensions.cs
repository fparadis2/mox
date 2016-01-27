using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Mox.UI
{
    public static class ScreenExtensions
    {
        public static void ActivateScreen(this IChild screen)
        {
            IConductor conductor = (IConductor) screen.Parent;

            if (conductor == null)
                return;

            conductor.ActivateItem(screen);

            var parentScreen = conductor as IChild;
            if (parentScreen != null)
                ActivateScreen(parentScreen);
        }
    }
}
