using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Rhino.Mocks;

namespace Mox.UI
{
    public sealed class MockViewModelServices : IDisposable, IViewModelServices
    {
        #region Static Usage

        public static MockViewModelServices Use(MockRepository mockery)
        {
            MockViewModelServices services = new MockViewModelServices(mockery);
            services.m_handle = ViewModelServices.Use(services);
            return services;
        }

        #endregion

        #region Variables

        private IDisposable m_handle;

        private readonly MockRepository m_mockery;
        private readonly List<FindParentExpectation> m_findParentExpectations = new List<FindParentExpectation>();

        #endregion

        #region Constructor

        private MockViewModelServices(MockRepository mockery)
        {
            Throw.IfNull(mockery, "mockery");
            m_mockery = mockery;
        }

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_handle);
        }

        #endregion

        #region Implementation of IViewModelServices

        public TParent FindParent<TParent>(IChild child)
        {
            var expectation = m_findParentExpectations.FirstOrDefault(e => Equals(e.Child, child) && typeof(TParent).IsAssignableFrom(e.ParentType));

            if (expectation == null)
            {
                Assert.Fail("Unexpected FindParent<{0}> call on {1}", typeof (TParent).Name, child.GetType().FullName);
            }

            return (TParent)expectation.Parent;
        }

        public void Expect_FindParent<TParent>(IChild child, TParent result)
        {
            m_findParentExpectations.Add(new FindParentExpectation
            {
                Child = child,
                ParentType = typeof(TParent),
                Parent = result
            });
        }

        public MockPageHandle Expect_Push<TNavigationViewModel>(IChild child, Action<TNavigationViewModel> validationCallback)
            where TNavigationViewModel : class
        {
            INavigationConductor<TNavigationViewModel> conductor = m_mockery.StrictMock<INavigationConductor<TNavigationViewModel>>();
            Expect_FindParent(child, conductor);

            MockPageHandle pageHandle = new MockPageHandle();
            Expect.Call(conductor.Push(null)).Return(pageHandle).IgnoreArguments().Callback<TNavigationViewModel>(model =>
            {
                if (validationCallback != null)
                {
                    validationCallback(model);
                }
                return true;
            });
            return pageHandle;
        }

        public void Expect_PopParent(IChild child)
        {
            INavigationConductor conductor = m_mockery.StrictMock<INavigationConductor>();
            Expect_FindParent(child, conductor);
            conductor.Pop();
        }

        #endregion

        #region Inner Types

        private class FindParentExpectation
        {
            public IChild Child;
            public System.Type ParentType;
            public object Parent;
        }
		 
	    #endregion
    }
}
