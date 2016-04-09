using System.Collections.Generic;

namespace Mox.UI.Game
{
    public class BattlefieldGroup : List<CardViewModel>
    {
        public readonly BattlefieldViewModel.PermanentType Type;

        public BattlefieldGroup(BattlefieldViewModel.PermanentType type)
        {
            Type = type;
        }
    }
}