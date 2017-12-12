using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Database;
using Mox.Database.Internal;

namespace Mox
{
    partial class Program
    {
        static void Main(string[] args)
        {
            //CardDatabaseFileUpdater.UpdateDatabase();
            SetSymbolsGenerator.GenerateSymbols();
        }        
    }
}
