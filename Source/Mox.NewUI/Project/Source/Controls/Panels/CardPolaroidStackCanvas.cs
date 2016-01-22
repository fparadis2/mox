using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Mox.Database;

namespace Mox.UI
{
    public class CardPolaroidStackCanvas : PolaroidStackCanvas
    {
        private readonly IRandom m_random = Random.New();

        protected override object PrepareNextContent()
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
            CardFrameRenderer.PreloadResourcesForRendering(randomInstance);
            return randomInstance;
        }

        protected override FrameworkElement CreateContainer(object content)
        {
            CardInstanceInfo card = (CardInstanceInfo) content;
            var frame = new CardFrame
            {
                Card = card,
                Width = ActualWidth / 5
            };
            return new ContentPresenter { Content = frame };
        }
    }
}