using System;
using NUnit.Framework;

namespace Mox.UI
{
    [TestFixture]
    public class EditableViewModelTests
    {
        #region Variables

        private MyEditableViewModel m_editableViewModel;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_editableViewModel = new MyEditableViewModel();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_arguments()
        {
            Assert.IsFalse(m_editableViewModel.IsEditing);
            Assert.IsFalse(m_editableViewModel.IsDirty);
        }

        [Test]
        public void Test_Can_set_IsDirty()
        {
            m_editableViewModel.IsDirty = true;
            Assert.IsTrue(m_editableViewModel.IsDirty);

            Assert.ThatProperty(m_editableViewModel, e => e.IsDirty).RaisesChangeNotification();
        }

        [Test]
        public void Test_BeginEdit_sets_IsEditing_and_resets_IsDirty()
        {
            m_editableViewModel.IsDirty = true;

            Assert.ThatProperty(m_editableViewModel, e => e.IsEditing).RaisesChangeNotificationWhen(() => m_editableViewModel.BeginEdit());
            Assert.IsTrue(m_editableViewModel.IsEditing);
            Assert.IsFalse(m_editableViewModel.IsDirty);
        }

        [Test]
        public void Test_Cannot_BeginEdit_twice()
        {
            m_editableViewModel.BeginEdit();
            Assert.Throws<InvalidOperationException>(() => m_editableViewModel.BeginEdit());
        }

        [Test]
        public void Test_EndEdit_resets_IsEditing()
        {
            m_editableViewModel.BeginEdit();
            Assert.ThatProperty(m_editableViewModel, e => e.IsEditing).RaisesChangeNotificationWhen(() => m_editableViewModel.EndEdit());
            Assert.IsFalse(m_editableViewModel.IsEditing);
        }

        [Test]
        public void Test_Cannot_EndEdit_when_not_editing()
        {
            Assert.Throws<InvalidOperationException>(() => m_editableViewModel.EndEdit());
        }

        [Test]
        public void Test_CancelEdit_resets_IsEditing()
        {
            m_editableViewModel.BeginEdit();
            Assert.ThatProperty(m_editableViewModel, e => e.IsEditing).RaisesChangeNotificationWhen(() => m_editableViewModel.CancelEdit());
            Assert.IsFalse(m_editableViewModel.IsEditing);
        }

        [Test]
        public void Test_Cannot_CancelEdit_when_not_editing()
        {
            Assert.Throws<InvalidOperationException>(() => m_editableViewModel.CancelEdit());
        }

        [Test]
        public void Test_Cannot_Modify_when_not_editing()
        {
            Assert.Throws<InvalidOperationException>(() => m_editableViewModel.Modify(() => { }));
        }

        [Test]
        public void Test_Modify_calls_the_action_and_sets_IsDirty()
        {
            bool called = false;
            System.Action action = () => called = true;

            m_editableViewModel.BeginEdit();
            m_editableViewModel.Modify(action);
            Assert.IsTrue(called);
            Assert.IsTrue(m_editableViewModel.IsDirty);
        }

        #endregion

        #region Inner Types

        private class MyEditableViewModel : EditableViewModel
        {
            public new void Modify(System.Action action)
            {
                base.Modify(action);
            }
        }

        #endregion
    }
}
