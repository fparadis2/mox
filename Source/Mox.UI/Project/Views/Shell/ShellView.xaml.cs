using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;

namespace Mox.UI.Shell
{
    /// <summary>
    /// Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView : MetroWindow
    {
        public ShellView()
        {
            InitializeComponent();

            DialogViewModel.ShowImplementation = ShowDialog;
            DialogViewModel.CloseImplementation = CloseDialog;
        }

        private object ShowDialog(IDialogViewModel dialog)
        {
            DialogView view = new DialogView();
            ViewModelBinder.Bind(dialog, view, null);
            _ModalContentPresenter.PushModalContent(view);
            return view;
        }

        private void CloseDialog(IDialogViewModel dialog, object view)
        {
            _ModalContentPresenter.PopModalContent(view);
        }
    }
}
