using System;

using Caliburn.Micro;

namespace Mox.UI
{
    public abstract class Child : PropertyChangedBase, IChild
    {
        #region Implementation of IChild

        public object Parent
        {
            get { return ((IChild)this).Parent; }
        }

        object IChild.Parent
        {
            get;
            set;
        }

        #endregion
    }
}
