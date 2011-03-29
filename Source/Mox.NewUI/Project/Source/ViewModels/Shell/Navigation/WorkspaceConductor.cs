using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mox.UI
{
    public class WorkspaceConductor<TWorkspace> : NavigationConductor<INavigationViewModel<TWorkspace>>
        where TWorkspace : new()
    {
        #region Variables

        private static readonly List<PropertyInfo> ms_workspaceProperties = new List<PropertyInfo>();

        private readonly TWorkspace m_workspace = new TWorkspace();
        private readonly Stack<TWorkspace> m_stack = new Stack<TWorkspace>();

        #endregion

        #region Constructor

        static WorkspaceConductor()
        {
            foreach (PropertyInfo property in typeof(TWorkspace).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (property.CanRead && property.CanWrite)
                {
                    ms_workspaceProperties.Add(property);
                }
            }
        }

        #endregion

        #region Properties

        public TWorkspace Workspace
        {
            get { return m_workspace; }
        }

        #endregion

        #region Methods

        protected override void OnPush(INavigationViewModel<TWorkspace> viewModel)
        {
            base.OnPush(viewModel);

            m_stack.Push(Clone(m_workspace));

            var workCopy = Clone(m_workspace);
            viewModel.Fill(workCopy);
            Assign(workCopy, m_workspace, TransformWorkspaceValue);
        }

        protected virtual object TransformWorkspaceValue(object oldValue, object newValue)
        {
            return newValue;
        }

        protected override void OnPop()
        {
            var old = m_stack.Pop();
            Assign(old, m_workspace);

            base.OnPop();
        }

        private static TWorkspace Clone(TWorkspace original)
        {
            var copy = new TWorkspace();
            Assign(original, copy);
            return copy;
        }

        private static void Assign(TWorkspace source, TWorkspace target, Func<object, object, object> converter = null)
        {
            foreach (var property in ms_workspaceProperties)
            {
                object value = property.GetValue(source, null);
                object oldValue = property.GetValue(target, null);

                object newValue = value;
                if (converter != null)
                {
                    newValue = converter(oldValue, value);
                }

                if (!Equals(oldValue, newValue))
                {
                    property.SetValue(target, newValue, null);
                }
            }
        }

        #endregion
    }
}
