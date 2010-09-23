using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mox.Database;

namespace Mox.UI.Browser
{
    public class MockDeckViewModelEditor : IDeckViewModelEditor
    {
        private readonly CardDatabase m_database;

        public MockDeckViewModelEditor(CardDatabase database)
        {
            m_database = database;
        }

        public CardDatabase Database
        {
            get { return m_database; }
        }

        public bool IsEnabled
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }
    }
}
