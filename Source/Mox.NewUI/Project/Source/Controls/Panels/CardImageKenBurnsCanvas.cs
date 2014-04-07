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


            ImageKey key = ImageKey.ForCardImage(randomInstance, false);
            
            var image = new Image
            {
                Width = 480,
                Height = 680
            };

            ImageService.SetKey(image, key);

            readyObject = image;
            return image;
        }

        protected override bool IsReady(object readyObject)
        {
            Image image = (Image)readyObject;
            return image.Source != null;
        }
    }
}