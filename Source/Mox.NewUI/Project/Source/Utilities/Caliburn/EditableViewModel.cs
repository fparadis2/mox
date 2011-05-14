using System;
using System.ComponentModel;
using Caliburn.Micro;

namespace Mox.UI
{
    public abstract class EditableViewModel : PropertyChangedBase, IEditableObject
    {
        #region Variables

        private bool m_isEditing;

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

        #endregion

        #region Methods

        public virtual void BeginEdit()
        {
            Throw.InvalidOperationIf(IsEditing, "Already editing this object");
            IsEditing = true;
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

        #endregion
    }
}
