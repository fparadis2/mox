using System;
using System.Collections.Generic;
using Mox.Database;

namespace Mox.UI.Browser
{
    internal class DesignTimeCardViewModel : CardViewModel
    {
        public DesignTimeCardViewModel()
            : base(CreateCardInfo())
        {
        }

        private static CardInfo CreateCardInfo()
        {
            var database = new CardDatabase();

            List<SetInfo> sets = new List<SetInfo>
            {
                database.AddSet("TMP", "Forces of nature", "Joys of life", DateTime.Now), 
                database.AddSet("M11", "Creatures of the house", "Joys of life", DateTime.Now.Subtract(TimeSpan.FromDays(2)))
            };

            return CreateCardInfo(database, sets);
        }

        internal static CardInfo CreateCardInfo(CardDatabase database, IEnumerable<SetInfo> sets)
        {
            CardInfo yogurt = database.AddCard("Turned yogurt", "BB", SuperType.None, Type.Artifact | Type.Creature, new SubType[0], "1", "1", new[] { "{2}{R}: Will you eat it?", "{W/B}: Will you dare?" });

            foreach (SetInfo set in sets)
            {
                database.AddCardInstance(yogurt, set, Rarity.Common, 1, "Frank");
            }

            return yogurt;
        }
    }
}