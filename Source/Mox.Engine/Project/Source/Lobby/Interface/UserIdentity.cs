using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Mox
{
    public interface IUserIdentity
    {
        string Name { get; }
        byte[] Image { get; }
    }

    [Serializable]
    public class UserIdentity : IUserIdentity
    {
        public string Name { get; set; }
        public byte[] Image { get; set; }
    }
}
