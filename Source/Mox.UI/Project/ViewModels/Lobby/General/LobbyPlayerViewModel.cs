using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Mox.Lobby;

namespace Mox.UI.Lobby
{
    public class LobbyPlayerViewModel : LobbyUserViewModel
    {
        #region Variables

        private readonly bool m_isBot;

        #endregion

        #region Constructor

        public LobbyPlayerViewModel(PlayerData player, bool isBot = false)
            : base(player.Id)
        {
            SyncFromPlayer(player);
            m_isBot = isBot;
        }

        #endregion

        #region Properties

        public bool IsBot
        {
            get { return m_isBot; }
        }

        private ImageSource m_image;

        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                NotifyOfPropertyChange();
            }
        }

        #endregion

        #region Methods

        internal void SyncFromPlayer(PlayerData player)
        {
            Debug.Assert(player.Id == Id);
            Name = player.Name;
        }

        public static async Task<ImageSource> GetImageSource(IPlayerIdentity identity)
        {
            var image = await Task.Run(() => ToImageSource(identity.Image));

            if (image == null)
            {
                var generatedBytes = await AvatarGenerator.GetAvatar(identity.Name);
                if (generatedBytes == null)
                    return null;

                image = await Task.Run(() => ToImageSource(generatedBytes));
            }

            return image;
        }

        private static ImageSource ToImageSource(byte[] imageBytes)
        {
            if (imageBytes == null)
                return null;

            try
            {
                MemoryStream stream = new MemoryStream(imageBytes);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch
            {
                return null;
            } 
        }

        #endregion
    }
}
