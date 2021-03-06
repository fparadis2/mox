﻿using System;

namespace Mox.Flow
{
    [Serializable]
    public abstract class Choice
    {
        #region Variables

        private readonly Resolvable<Player> m_player;

        #endregion

        #region Constructor

        protected Choice(Resolvable<Player> player)
        {
            m_player = player;
        }

        #endregion

        #region Properties

        public Resolvable<Player> Player
        {
            get { return m_player; }
        }

        public abstract object DefaultValue { get; }

        #endregion
    }

    public interface IChoiceResult
    {
        string ToString(Game game);
    }
}