using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
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
                var instances = randomCard.Instances.Where(instance => instance.Index > 0).ToList();
                if (instances.Count > 0)
                {
                    randomInstance = m_random.Choose(instances);
                }
            }
            var asyncImage = ImageService.GetCardImage(randomInstance);

            var image = new Image
            {
                Width = 480,
                Height = 680
            };

            Binding sourceBinding = new Binding("ImageSource") { Source = asyncImage };
            image.SetBinding(Image.SourceProperty, sourceBinding);

            readyObject = asyncImage;
            return image;
        }

        protected override bool IsReady(object readyObject)
        {
            AsyncImage asyncImage = (AsyncImage)readyObject;
            return !asyncImage.IsLoading;
        }
    }
}