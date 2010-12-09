using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mox.UI
{
    [TemplatePart(Name = TextBoxPartName, Type = typeof(TextBox))]
    public class EditableTextBlock : Control
    {
        #region Constants

        private const string TextBoxPartName = "EditTextBox";

        #endregion

        #region Variables

        private TextBox m_textBox;

        #endregion

        #region Constructor

        static EditableTextBlock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EditableTextBlock), new FrameworkPropertyMetadata(typeof(EditableTextBlock)));
        }

        public EditableTextBlock()
        {
            Focusable = true;
            FocusVisualStyle = null;
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("") { BindsTwoWayByDefault = true });
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));
        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
        }

        public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));
        public bool IsInEditMode
        {
            get
            {
                if (IsEditable)
                {
                    return (bool)GetValue(IsInEditModeProperty);
                }
                return false;
            }
            set
            {
                if (IsEditable && value != IsInEditMode)
                {
                    if (!value && TextBox != null)
                    {
                        Text = TextBox.Text;
                    }
                    SetValue(IsInEditModeProperty, value);
                }
            }
        }

        public static readonly DependencyProperty AcceptsReturnProperty = DependencyProperty.Register("AcceptsReturn", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));
        public bool AcceptsReturn
        {
            get { return (bool)GetValue(AcceptsReturnProperty); }
            set { SetValue(AcceptsReturnProperty, value); }
        }

        public static readonly DependencyProperty AcceptsTabProperty = DependencyProperty.Register("AcceptsTab", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));
        public bool AcceptsTab
        {
            get { return (bool)GetValue(AcceptsTabProperty); }
            set { SetValue(AcceptsTabProperty, value); }
        }

        private TextBox TextBox
        {
            get { return m_textBox; }
            set
            {
                if (m_textBox != value)
                {
                    if (m_textBox != null)
                    {
                        m_textBox.KeyDown -= m_textBox_KeyDown;
                        m_textBox.LostFocus -= m_textBox_LostFocus;
                    }

                    m_textBox = value;

                    if (m_textBox != null)
                    {
                        m_textBox.KeyDown += m_textBox_KeyDown;
                        m_textBox.LostFocus += m_textBox_LostFocus;

                        m_textBox.Focus();
                        m_textBox.SelectAll();
                    }
                }
            }
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextBox = GetTemplateChild(TextBoxPartName) as TextBox;
        }

        #endregion

        #region Event Handlers

        void m_textBox_KeyDown(object sender, KeyEventArgs e)
        {
            bool enterCommitsChanges = !AcceptsReturn || Keyboard.Modifiers != ModifierKeys.None;
            if (e.Key == Key.Enter && enterCommitsChanges)
            {
                IsInEditMode = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                TextBox = null;
                IsInEditMode = false;
                e.Handled = true;
            }
        }

        void m_textBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsInEditMode = false;
        }

        #endregion
    }
}
