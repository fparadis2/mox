using System;
using System.ComponentModel;
using Caliburn.Micro;

namespace Mox.UI
{
    public abstract class EditableViewModel : PropertyChangedBase, IEditableObject, IDataErrorInfo
    {
        #region Variables

        private readonly DataErrorInfo m_errorInfo = new DataErrorInfo();

        private bool m_isEditing;
        private bool m_isDirty;

        #endregion

        #region Properties

        public bool IsEditing
        {
            get { return m_isEditing; }
            private set
            {
                if (m_isEditing != value)
                {
                    m_isEditing = value;
                    NotifyOfPropertyChange(() => IsEditing);
                }
            }
        }

        public bool IsDirty
        {
            get { return m_isDirty; }
            set
            {
                if (m_isDirty != value)
                {
                    m_isDirty = value;
                    NotifyOfPropertyChange(() => IsDirty);
                }
            }
        }

        protected DataErrorInfo ErrorInfo
        {
            get { return m_errorInfo; }
        }

        #endregion

        #region Methods

        public virtual void BeginEdit()
        {
            Throw.InvalidOperationIf(IsEditing, "Already editing this object");
            IsEditing = true;
            IsDirty = false;
        }

        public virtual void EndEdit()
        {
            Throw.InvalidOperationIf(!IsEditing, "This object is not being edited");
            IsEditing = false;
        }

        public virtual void CancelEdit()
        {
            Throw.InvalidOperationIf(!IsEditing, "This object is not being edited");
            IsEditing = false;
        }

        protected void Modify(System.Action action)
        {
            Throw.InvalidOperationIf(!IsEditing, "Must call BeginEdit before editing a view model");
            action();
            IsDirty = true;
        }

        #endregion

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get { return m_errorInfo[columnName]; }
        }

        public string Error
        {
            get { return m_errorInfo.Error; }
        }

        #endregion
    }
}
