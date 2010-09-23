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

namespace Mox.Effects
{
    [Serializable]
    public class ChangeControllerEffect : MTGEffect<Player>
    {
        #region Variables

        private readonly Resolvable<Player> m_controllerIdentifier;

        #endregion

        #region Constructor

        public ChangeControllerEffect(Player controller)
            : base(Card.ControllerProperty)
        {
            Throw.IfNull(controller, "controller");
            m_controllerIdentifier = new Resolvable<Player>(controller);
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.ControlChanging; }
        }

        public Resolvable<Player> Controller
        {
            get { return m_controllerIdentifier; }
        }

        #endregion

        #region Methods

        public override Player Modify(Object owner, Player value)
        {
            return Controller.Resolve(owner.Manager);
        }

        #endregion
    }
}
