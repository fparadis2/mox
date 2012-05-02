// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Mox.UI
{
    public class ButtonAction
    {
        #region Variables

        public static readonly DependencyProperty BindToProperty = DependencyProperty.RegisterAttached("BindTo", typeof(string), typeof(ButtonAction), new PropertyMetadata(OnBindToChanged));

        #endregion

        #region Methods

        private static void OnBindToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SetBindTo(d, e.NewValue);
        }

        public static void SetBindTo(DependencyObject d, object value)
        {
            var buttonBase = d as ButtonBase;
            if (buttonBase == null) return;

            buttonBase.SetValue(BindToProperty, value);

            var handler = new Handler((string)value);
            handler.Initialize(buttonBase);
        }

        public static object GetBindTo(DependencyObject d)
        {
            var element = d as FrameworkElement;
            if (element == null) return null;

            return element.GetValue(BindToProperty);
        }

        private static ICommand GenerateCommand(DependencyObject element, string message)
        {
            foreach (var dataContext in EnumerateDataContexts(element))
            {
                foreach (var method in dataContext.GetType().GetMethods())
                {
                    if (method.Name == message)
                    {
                        var command = GenerateCommand(dataContext, method, element);
                        if (command != null)
                        {
                            return command;
                        }
                    }
                }
            }

            throw new InvalidProgramException(string.Format("Could not find handler corresponding to '{0}' in '{1}'", message, element));
        }

        private static ICommand GenerateCommand(object methodHost, MethodInfo method, DependencyObject element)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                return new Command(methodHost, method, new object[0]);
            }

            if (parameters.Length == 1)
            {
                foreach (var dataContext in EnumerateDataContexts(element))
                {
                    if (parameters[0].ParameterType.IsAssignableFrom(dataContext.GetType()))
                    {
                        return new Command(methodHost, method, new [] { dataContext });
                    }
                }
            }

            return null;
        }

        private static IEnumerable<object> EnumerateDataContexts(DependencyObject element)
        {
            HashSet<object> visitedElements = new HashSet<object>();

            for (DependencyObject parent = element; parent != null; parent = VisualTreeHelper.GetParent(parent))
            {
                FrameworkElement frameworkElement = parent as FrameworkElement;
                if (frameworkElement != null && frameworkElement.DataContext != null && visitedElements.Add(frameworkElement.DataContext))
                {
                    yield return frameworkElement.DataContext;
                }
            }
        }

        #endregion

        #region Inner Types

        private class Handler
        {
            private readonly string m_bindMessage;

            public Handler(string bindMessage)
            {
                m_bindMessage = bindMessage;
            }

            public void Initialize(ButtonBase button)
            {
                button.Loaded += button_Loaded;
            }

            private void Attach(ButtonBase button)
            {
                if ((string)GetBindTo(button) == m_bindMessage)
                {
                    button.Command = GenerateCommand(button, m_bindMessage);
                }
            }

            void button_Loaded(object sender, RoutedEventArgs e)
            {
                var button = (ButtonBase)sender;
                button.Loaded -= button_Loaded;

                Attach(button);
            }
        }

        private class Command : ICommand
        {
            #region Variables

            private readonly object m_methodHost;
            private readonly MethodInfo m_executeMethod;
            private readonly object[] m_executeParameters;

            public Command(object methodHost, MethodInfo executeMethod, object[] executeParameters)
            {
                m_methodHost = methodHost;
                m_executeParameters = executeParameters;
                m_executeMethod = executeMethod;
            }

            #endregion

            #region Implementation of ICommand

            public void Execute(object parameter)
            {
                m_executeMethod.Invoke(m_methodHost, m_executeParameters);
            }

            public bool CanExecute(object parameter)
            {
#warning [MEDIUM] TODO: Support CanExecute
                return true;
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            #endregion
        }

        #endregion
    }
}
