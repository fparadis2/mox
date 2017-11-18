using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public interface IGameLog
    {
        void Log(FormattableString message);
    }
}
