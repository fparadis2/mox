using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Mox.UI
{
    public static class CaliburnExtensions
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

        public static T FindParent<T>(this IChild child)
        {
            while (child != null)
            {
                if (child.Parent is T)
                    return (T)child.Parent;

                child = child.Parent as IChild;
            }

            return default(T);
        }
    }
}
