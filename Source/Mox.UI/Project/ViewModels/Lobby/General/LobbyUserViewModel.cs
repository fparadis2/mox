using System;
using Caliburn.Micro;
using Mox.Lobby;
using System.Windows.Media;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Media.Imaging;

namespace Mox.UI.Lobby
{
    public class LobbyUserViewModel : PropertyChangedBase
    {
        #region Variables
        
        private string m_name;
        private ImageSource m_image;
        private bool m_isBot;

        #endregion

        #region Constructor

        public LobbyUserViewModel(Guid id)
        {
            Id = id;
        }

        #endregion

        #region Properties

        public Guid Id { get; }

        public string Name
        {
            get { return m_name; }
            set
            {
                if (m_name != value)
                {
                    m_name = value;
                    NotifyOfPropertyChange(() => Name);
                }
            }
        }

        public ImageSource Image
        {
            get { return m_image; }
            set
            {
                m_image = value;
                NotifyOfPropertyChange();
            }
        }

        public bool IsBot
        {
            get { return m_isBot; }
            set
            {
                if (m_isBot != value)
                {
                    m_isBot = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        #endregion

        #region Methods

        internal void Update(ILobbyUser user)
        {
            Debug.Assert(user.Id == Id);

            var data = user.Data;

            Name = data.Name;
            IsBot = data.IsBot;
        }

        internal async Task UpdateIdentity(IUserIdentity identity)
        {
            var image = await Task.Run(() => ToImageSource(identity.Image));

            if (image == null)
            {
                var generatedBytes = await AvatarGenerator.GetAvatar(identity.Name);
                if (generatedBytes != null)
                {
                    image = await Task.Run(() => ToImageSource(generatedBytes));
                }
            }

            Image = image;
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