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

namespace Mox.Abilities
{
    [Serializable]
    public class ChangeColorEffect : MTGEffect<Color>
    {
        #region Variables

        private readonly Color m_color;

        #endregion

        #region Constructor

        public ChangeColorEffect(Color color)
            : base(Card.ColorProperty)
        {
            m_color = color;
        }

        #endregion

        #region Properties

        public override EffectDependencyLayer DependendencyLayer
        {
            get { return EffectDependencyLayer.ColorChanging; }
        }

        public Color Color
        {
            get { return m_color; }
        }

        #endregion

        #region Methods

        public override Color Modify(Object owner, Color value)
        {
            return m_color;
        }

        #endregion
    }
}
