﻿using System;
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

namespace Mox.UI.Library
{
    /// <summary>
    /// Interaction logic for DeckView.xaml
    /// </summary>
    public partial class DeckView : UserControl
    {
        public DeckView()
        {
            InitializeComponent();
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            DeckLibraryView libraryView = this.FindVisualParent<DeckLibraryView>();

            DeckLibraryViewModel library = (DeckLibraryViewModel)libraryView.DataContext;
            DeckViewModel deck = (DeckViewModel)DataContext;

            library.DeleteDeck(deck);
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            DeckLibraryView libraryView = this.FindVisualParent<DeckLibraryView>();

            DeckLibraryViewModel library = (DeckLibraryViewModel)libraryView.DataContext;
            DeckViewModel deck = (DeckViewModel)DataContext;

            library.EditDeck(deck);
        }
    }
}
