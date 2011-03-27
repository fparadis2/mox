﻿using System;
using Caliburn.Micro;
using NUnit.Framework;
using Rhino.Mocks;

namespace Mox.UI
{
    [TestFixture]
    public class WorkspaceConductorTests
    {
        #region Mock Types

        public class MyWorkspaceView : PropertyChangedBase, IWorkspaceView
        {
            private int m_property1;
            private object m_property2;

            public int Property1
            {
                get { return m_property1; }
                set
                {
                    if (m_property1 != value)
                    {
                        m_property1 = value;
                        NotifyOfPropertyChange(() => Property1);
                    }
                }
            }
            public object Property2
            {
                get { return m_property2; }
                set
                {
                    if (m_property2 != value)
                    {
                        m_property2 = value;
                        NotifyOfPropertyChange(() => Property2);
                    }
                }
            }

            #region Implementation of IWorkspaceView

            public void AssignTo(IWorkspaceView other)
            {
                MyWorkspaceView otherView = (MyWorkspaceView)other;
                otherView.Property1 = Property1;
                otherView.Property2 = Property2;
            }

            #endregion
        }

        #endregion

        #region Variables

        private MockRepository m_mockery;
        private INavigationViewModel<MyWorkspaceView> m_viewModel;
        private WorkspaceConductor<MyWorkspaceView> m_conductor;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_mockery = new MockRepository();
            m_viewModel = m_mockery.StrictMock<INavigationViewModel<MyWorkspaceView>>();
            m_conductor = new WorkspaceConductor<MyWorkspaceView>();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_conductor.View);
            Assert.AreEqual(0, m_conductor.View.Property1);
            Assert.AreEqual(null, m_conductor.View.Property2);
        }

        [Test]
        public void Test_Pushing_a_viewmodel_fills_the_view()
        {
            m_conductor.View.Property1 = 3;
            m_conductor.View.Property2 = new object();

            m_viewModel.Fill(null);

            LastCall.IgnoreArguments().Callback<MyWorkspaceView>(view =>
            {
                Assert.AreEqual(3, view.Property1);
                Assert.IsNotNull(view.Property2);

                view.Property1 = 10;
                view.Property2 = new object();
                return true;
            });

            using (m_mockery.Test())
            {
                Assert.ThatProperty(m_conductor.View, v => v.Property1)
                      .AndProperty(v => v.Property2)
                      .RaisesChangeNotificationWhen(() => m_conductor.Push(m_viewModel));
            }

            Assert.AreEqual(10, m_conductor.View.Property1);
            Assert.IsNotNull(m_conductor.View.Property2);
        }

        [Test]
        public void Test_Pop_restores_the_view()
        {
            var originalObject = new object();
            m_conductor.View.Property1 = 3;
            m_conductor.View.Property2 = originalObject;

            m_viewModel.Fill(null);

            LastCall.IgnoreArguments().Callback<MyWorkspaceView>(view =>
            {
                view.Property1 = 10;
                view.Property2 = new object();
                return true;
            });

            using (m_mockery.Test())
            {
                m_conductor.Push(m_viewModel);
            }

            Assert.ThatProperty(m_conductor.View, v => v.Property1)
                      .AndProperty(v => v.Property2)
                      .RaisesChangeNotificationWhen(() => m_conductor.Pop());

            Assert.AreEqual(3, m_conductor.View.Property1);
            Assert.AreSame(originalObject, m_conductor.View.Property2);
        }

        #endregion
    }
}