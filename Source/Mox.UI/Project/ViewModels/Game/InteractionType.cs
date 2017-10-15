using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI.Game
{
    public enum InteractionType
    {
        None,
        /// <summary>
        /// Play, activate
        /// </summary>
        Play,
        /// <summary>
        /// Target
        /// </summary>
        Target,
        /// <summary>
        /// Attack
        /// </summary>
        Attack,
    }
}
