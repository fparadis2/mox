using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mox.UI
{
    /// <summary>
    /// A textblock that can be edited inline.
    /// </summary>
    public partial class EditableTextBlock : UserControl
    {
        #region Variables

        private string m_oldText;

        #endregion

        #region Constructor

        public EditableTextBlock()
        {
            InitializeComponent();
            Focusable = true;
            FocusVisualStyle = null;
        }

        #endregion

        #region Properties

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(""));
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
                    if (value)
                    {
                        m_oldText = Text;
                    }
                    SetValue(IsInEditModeProperty, value);
                }
            }
        }

        #endregion

        #region Event Handlers

        // Invoked when we enter edit mode.
        void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            if (textBox != null)
            {
                textBox.Focus();

                textBox.SelectAll();
            }
        }

        // Invoked when we exit edit mode.
        void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsInEditMode = false;
        }

        // Invoked when the user edits the annotation.
        void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsInEditMode = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                IsInEditMode = false;
                Text = m_oldText;
                e.Handled = true;
            }
        }

        #endregion
    }
}
