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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Mox.Replication;

namespace Mox.UI
{
	public partial class GamePage
	{
        #region Variables

        private Game m_game;
        private GameRunner m_gameRunner;
        private UIPlayerController m_playerController;
        private readonly GameViewModel m_gameViewModel = new GameViewModel();

        #endregion

        #region Constructor

        public GamePage()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            DataContext = m_gameViewModel;

            CreateGame();
            RunGame();
        }

        private void CreateGame()
        {
            m_gameRunner = new GameRunner();

            // Start listener
            GameListener listener = new GameListener();
            m_game = listener.Game;
            m_gameRunner.Register(listener);

            // Assign controller
            m_playerController = new UIPlayerController(m_gameViewModel, Dispatcher);
            m_gameRunner.AssignController(m_game.Players[0], m_playerController);

            new GameViewModelSynchronizer(m_gameViewModel, m_game, m_game.Players[0], Dispatcher);
        }

        private void RunGame()
        {
            Thread gameThread = new Thread(m_gameRunner.Run);
            gameThread.IsBackground = true;
            gameThread.Name = "Game Thread";
            gameThread.Start();
        }

        #endregion
	}
}