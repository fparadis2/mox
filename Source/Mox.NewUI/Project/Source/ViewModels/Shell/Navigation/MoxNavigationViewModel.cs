using Caliburn.Micro;

namespace Mox.UI
{
    public abstract class MoxNavigationViewModel : Child, INavigationViewModel<MoxWorkspace>
    {
        #region Methods

        protected TPart ActivatePart<TPart>(TPart part)
        {
            IChild child = part as IChild;
            if (child != null)
            {
                child.Parent = this;
            }
            return part;
        }

        #endregion

        #region Implementation of INavigationViewModel<in MoxWorkspace>

        public abstract void Fill(MoxWorkspace view);

        #endregion
    }
}