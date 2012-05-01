using System;
using Caliburn.Micro;

namespace Mox.UI
{
    internal static class ViewModelServices
    {
        #region Instance

        private static IViewModelServices m_instance = new DefaultViewModelServices();

        internal static IDisposable Use(IViewModelServices newInstance)
        {
            var oldInstance = m_instance;
            m_instance = newInstance;
            return new DisposableHelper(() => m_instance = oldInstance);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the first parent of the given type.
        /// </summary>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="child"></param>
        /// <returns></returns>
        public static TParent FindParent<TParent>(this IChild child)
        {
            return m_instance.FindParent<TParent>(child);
        }

        public static IPageHandle PushNavigationViewModel(this IChild child, MoxNavigationViewModel viewModel)
        {
            var parent = child.FindParent<INavigationConductor<INavigationViewModel<MoxWorkspace>>>();

            if (parent != null)
            {
                return parent.Push(viewModel);
            }

            return null;
        }

        #endregion

        #region Inner Types

        internal class DefaultViewModelServices : IViewModelServices
        {
            #region Methods

            TParent IViewModelServices.FindParent<TParent>(IChild child)
            {
                object parent = child.Parent;

                while (parent != null)
                {
                    if (parent is TParent)
                    {
                        return (TParent)parent;
                    }

                    parent = parent is IChild ? ((IChild)parent).Parent : null;
                }

                return default(TParent);
            }

            #endregion
        }

        #endregion
    }

    internal interface IViewModelServices
    {
        #region Methods

        TParent FindParent<TParent>(IChild child);

        #endregion
    }
}
