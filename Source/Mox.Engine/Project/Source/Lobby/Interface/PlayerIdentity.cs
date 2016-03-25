using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mox
{
    public interface IPlayerIdentity
    {
        string Name { get; }
        byte[] Image { get; }
    }

    [Serializable]
    public class PlayerIdentity : IPlayerIdentity
    {
        public string Name { get; set; }
        public byte[] Image { get; set; }
    }
}
