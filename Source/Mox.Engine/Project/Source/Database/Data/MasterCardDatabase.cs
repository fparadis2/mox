// Copyright (c) François Paradis
// This file is part of Mox, a card game simulator.
// 
// Mox is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// Mox is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Mox.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mox.Database.Internal;
using Mox.Properties;

namespace Mox.Database
{
    /// <summary>
    /// The master card database
    /// </summary>
    public class MasterCardDatabase : CardDatabase
    {
        #region Singleton

        private static Task m_loadingTask;
        private static MasterCardDatabase m_instance;

        public static CardDatabase Instance
        {
            get
            {
                if (m_instance == null)
                {
                    BeginLoading();
                    m_loadingTask.Wait();
                }
                return m_instance;
            }
        }

        public static void BeginLoading()
        {
            if (Interlocked.CompareExchange(ref m_loadingTask, new Task(Load), null) == null)
            {
                m_loadingTask.Start();
            }
        }

        private static void Load()
        {
            Debug.Assert(m_instance == null);
            m_instance = new MasterCardDatabase();
        }

        #endregion

        #region Constructor

        private MasterCardDatabase()
        {
            var parser = new JsonParser(this);

            using (Profile("Total"))
            {
                using (Stream stream = new MemoryStream(Resources.AllSets))
                {
                    parser.Parse(stream);
                }
            }
        }

        private static IDisposable Profile(string scope)
        {
            return null;

            //Stopwatch watch = Stopwatch.StartNew();
            //return new DisposableHelper(() =>
            //{
            //    watch.Stop();
            //    Trace.WriteLine(scope + ": " + watch.Elapsed.TotalSeconds + "s");
            //});
        }

        #endregion
    }
}
