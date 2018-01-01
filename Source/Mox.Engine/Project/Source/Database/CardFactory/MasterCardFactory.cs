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
using System.Reflection;

namespace Mox.Database
{
    /// <summary>
    /// The card factory to rule them all.
    /// </summary>
    public class MasterCardFactory : AssemblyCardFactory
    {
        #region Singleton

        public static readonly MasterCardFactory Instance = new MasterCardFactory();

        #endregion

        #region Constructor

        private MasterCardFactory()
            : base(typeof(MasterCardFactory).Assembly)
        {
        }

        #endregion

        #region Methods

        public static CardFactoryResult Initialize(Card card)
        {
            return Initialize(card, Instance, MasterCardDatabase.Instance);
        }

        public static CardFactoryResult Initialize(Card card, ICardFactory factory, ICardDatabase cardDatabase)
        {
            CardInfo cardInfo = cardDatabase.GetCard(card.Name);
            Throw.InvalidArgumentIf(cardInfo == null, "Unknown card: " + card.Name, "card");

            return factory.InitializeCard(card, cardInfo);
        }

        #endregion
    }
}
