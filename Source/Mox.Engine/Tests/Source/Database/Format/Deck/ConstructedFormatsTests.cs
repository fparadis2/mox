using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Database;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ConstructedDeckFormatTests
    {
        #region Constants

        private const string ValidDeckContents = @"
4 Plains
";

        #endregion

        #region Setup

        private readonly MockConstructedDeckFormat m_format = new MockConstructedDeckFormat();
        
        #endregion

        #region Tests

        [Test]
        public void Test_Validate_returns_false_when_passing_a_null_deck()
        {
            Assert.That(!m_format.Validate(null));
        }

        [Test]
        public void Test_Validate_returns_true_only_if_the_deck_has_the_correct_amount_of_cards()
        {
            m_format.MockMinimumCardCount = 2;

            IDeck invalidDeck = Deck.Read("My Deck", "1 Plains");
            Assert.That(!m_format.Validate(invalidDeck));

            IDeck validDeck = Deck.Read("My Deck", ValidDeckContents);
            Assert.That(m_format.Validate(validDeck));
        }

        #endregion

        #region Nested Types

        private class MockConstructedDeckFormat : ConstructedDeckFormat
        {
            public override string Name
            {
                get { return "Mock"; }
            }

            public override string Description
            {
                get { return "Mock"; }
            }

            public int MockMinimumCardCount
            {
                get; 
                set;
            }

            public override int MinimumCardCount
            {
                get { return MockMinimumCardCount; }
            }
        }

        #endregion
    }
}
