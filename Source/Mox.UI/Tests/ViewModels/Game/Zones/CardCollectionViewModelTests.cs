using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI.Game
{
    [TestFixture]
    public class OrderedCardCollectionViewModelTests
    {
        #region Setup

        private GameViewModel m_game;
        private OrderedCardCollectionViewModel m_collection;

        [SetUp]
        public void Setup()
        {
            m_collection = new OrderedCardCollectionViewModel();

            m_game = new GameViewModel();
            m_collection.Add(CreateCard());
            m_collection.Add(CreateCard());
            m_collection.Add(CreateCard());
        }

        private CardViewModel CreateCard()
        {
            return new CardViewModel(m_game);
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Top_is_the_top_card()
        {
            Assert.AreEqual(m_collection[2], m_collection.Top);

            m_collection.RemoveAt(2);
            Assert.AreEqual(m_collection[1], m_collection.Top);

            m_collection.Clear();
            Assert.IsNull(m_collection.Top);

            var card = CreateCard();
            m_collection.Add(card);
            Assert.AreEqual(card, m_collection.Top);
        }

        [Test]
        public void Test_Top_triggers_change_notification_correctly()
        {
            Assert.ThatProperty(m_collection, c => c.Top).DoesntRaiseChangeNotificationWhen(() => m_collection.Insert(0, CreateCard()));
            Assert.ThatProperty(m_collection, c => c.Top).RaisesChangeNotificationWhen(() => m_collection.Add(CreateCard()));

            Assert.ThatProperty(m_collection, c => c.Top).DoesntRaiseChangeNotificationWhen(() => m_collection.RemoveAt(0));
            Assert.ThatProperty(m_collection, c => c.Top).RaisesChangeNotificationWhen(() => m_collection.Remove(m_collection.Top));

            m_collection.Add(CreateCard());
            m_collection.Add(CreateCard());
            m_collection.Add(CreateCard());
            Assert.ThatProperty(m_collection, c => c.Top).DoesntRaiseChangeNotificationWhen(() => m_collection.Move(0, 1));
            Assert.ThatProperty(m_collection, c => c.Top).RaisesChangeNotificationWhen(() => m_collection.Move(0, m_collection.Count - 1));

            Assert.ThatProperty(m_collection, c => c.Top).RaisesChangeNotificationWhen(() => m_collection.Clear());
        }

        #endregion
    }
}
