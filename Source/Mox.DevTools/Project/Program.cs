using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Database;
using Mox.Database.Internal;
using System.Diagnostics;

namespace Mox
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                args = new [] { "report" };
            }

            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "report":
                        new CardSupportReport().Write();
                        return;
                }
            }

            //CardDatabaseFileUpdater.UpdateDatabase();
            //SetSymbolsGenerator.GenerateSymbols();
        }        
    }
}
