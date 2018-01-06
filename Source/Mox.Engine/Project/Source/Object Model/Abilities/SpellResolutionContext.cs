using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class SpellResolutionContext
    {
        public SpellResolutionContext(Game game)
        {
            Game = game;
        }

        public Game Game
        {
            get;
            private set;
        }
    }
}
