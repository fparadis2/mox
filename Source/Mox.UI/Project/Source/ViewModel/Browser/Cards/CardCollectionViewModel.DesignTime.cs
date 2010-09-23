using System;
using System.Collections.Generic;
using Mox.Database;

namespace Mox.UI.Browser
{
    internal class DesignTimeCardCollectionViewModel : CardCollectionViewModel
    {
        public DesignTimeCardCollectionViewModel()
            : base(DesignTimeCardDatabase.Instance.Cards)
        {
        }
    }
}