﻿// Copyright (c) François Paradis
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mox.UI.Game
{
    /// <summary>
    /// Interaction logic for StackControl.xaml
    /// </summary>
    public partial class StackControl : UserControl
    {
        public StackControl()
        {
            InitializeComponent();
        }

        private void ListBox_MouseMove(object sender, MouseEventArgs e)
        {
            RefreshSelectedIndexFromMouse();
        }

        private void ListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            RefreshSelectedIndexFromMouse();
        }

        private void RefreshSelectedIndexFromMouse()
        {
            if (Mouse.Captured == null)
            {
                object element = _Stack.GetObjectDataFromPoint(Mouse.GetPosition(_Stack));

                if (element is SpellViewModel)
                {
                    _Stack.SelectedItem = element;
                    if (!_Stack.IsFocused)
                    {
                        _Stack.Focus();
                    }
                }
                else if (!IsMouseOver)
                {
                    _Stack.SelectedItem = null;
                }
            }
        }
    }
}
