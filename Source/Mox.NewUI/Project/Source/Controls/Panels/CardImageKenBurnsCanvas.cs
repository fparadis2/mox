using System.Linq;
using System.Windows.Controls;
using Mox.Database;

namespace Mox.UI
{
    public class CardImageKenBurnsCanvas : KenBurnsCanvas
    {
        private readonly IRandom m_random = Random.New();

        protected override object GetNextContent(out object readyObject)
        {
            // Random card, then random instance
            var database = MasterCardDatabase.Instance;
            CardInstanceInfo randomInstance = null;
            while (randomInstance == null)
            {
                CardInfo randomCard = m_random.Choose(database.Cards);
                var instances = randomCard.Instances.ToList();
                if (instances.Count > 0)
                {
                    randomInstance = m_random.Choose(instances);
                }
            }

            CardFrame cardFrame = new CardFrame
            {
                Card = randomInstance,
                Width = 480,
                Height = 680,
            };

            readyObject = cardFrame;
            return cardFrame;
        }

        protected override bool IsReady(object readyObject)
        {
            /*Image image = (Image)readyObject;
            return image.Source != null;*/
            return true;
        }
    }
}