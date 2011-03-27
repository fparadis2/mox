using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Mox.UI
{
    public sealed class MockViewModelServices : IDisposable, IViewModelServices
    {
        #region Static Usage

        public static MockViewModelServices Use()
        {
            MockViewModelServices services = new MockViewModelServices();
            services.m_handle = ViewModelServices.Use(services);
            return services;
        }

        #endregion

        #region Variables

        private IDisposable m_handle;

        private readonly List<FindParentExpectation> m_findParentExpectations = new List<FindParentExpectation>();

        #endregion

        #region Constructor

        private MockViewModelServices()
        {}

        public void Dispose()
        {
            DisposableHelper.SafeDispose(m_handle);
        }

        #endregion

        #region Implementation of IViewModelServices

        public TParent FindParent<TParent>(IChild child)
        {
            var expectation = m_findParentExpectations.FirstOrDefault(e => Equals(e.Child, child) && Equals(e.ParentType, typeof (TParent)));

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
