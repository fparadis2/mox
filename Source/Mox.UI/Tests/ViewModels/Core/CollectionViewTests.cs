using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.UI
{
    [TestFixture]
    public class CollectionViewTests
    {
        #region Variables

        private CollectionView<int> m_view;

        #endregion

        #region Setup/Teardown

        [SetUp]
        public void Setup()
        {
            m_view = new CollectionView<int>();
            m_view.Reset(Enumerable.Range(0, 100));
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Items_are_the_items_of_the_current_page()
        {
            m_view.PageSize = 45;

            m_view.CurrentPage = 0;
            Assert.Collections.AreEqual(Enumerable.Range(0, 45), m_view.Items);

            m_view.CurrentPage = 1;
            Assert.Collections.AreEqual(Enumerable.Range(45, 45), m_view.Items);

            m_view.CurrentPage = 2;
            Assert.Collections.AreEqual(Enumerable.Range(90, 10), m_view.Items);
        }

        [Test]
        public void Test_PageCount_is_the_page_count()
        {
            m_view.PageSize = 45;
            Assert.AreEqual(3, m_view.PageCount);

            m_view.PageSize = 25;
            Assert.AreEqual(4, m_view.PageCount);

            m_view.PageSize = 26;
            Assert.AreEqual(4, m_view.PageCount);

            m_view.PageSize = 100;
            Assert.AreEqual(1, m_view.PageCount);

            m_view.PageSize = int.MaxValue;
            Assert.AreEqual(1, m_view.PageCount);

            m_view.PageSize = 1;
            Assert.AreEqual(100, m_view.PageCount);
        }

        [Test]
        public void Test_CurrentPage_is_clamped_to_valid_pages()
        {
            m_view.PageSize = 25;

            m_view.CurrentPage = -1;
            Assert.AreEqual(0, m_view.CurrentPage);

            m_view.CurrentPage = 100;
            Assert.AreEqual(3, m_view.CurrentPage);
        }

        [Test]
        public void Test_PageSize_cannot_be_zero_or_negative()
        {
            m_view.PageSize = 0;
            Assert.AreEqual(1, m_view.PageSize);
            Assert.AreEqual(100, m_view.PageCount);

            m_view.PageSize = -1;
            Assert.AreEqual(1, m_view.PageSize);
            Assert.AreEqual(100, m_view.PageCount);
        }

        [Test]
        public void Test_CurrentPage_gets_adjusted_if_not_enough_pages()
        {
            m_view.PageSize = 10;
            m_view.CurrentPage = 9;
            Assert.AreEqual(10, m_view.PageCount);
            Assert.AreEqual(9, m_view.CurrentPage);

            m_view.PageSize = 25;
            Assert.AreEqual(4, m_view.PageCount);
            Assert.AreEqual(3, m_view.CurrentPage);
        }

        [Test]
        public void Test_Items_are_sorted_by_default()
        {
            m_view.Reset(new []{ 5, 1, 4, 3, 6, 2 });
            Assert.Collections.AreEqual(new [] { 1, 2, 3, 4, 5, 6 }, m_view.Items);

            m_view.SortComparer = (a, b) => b - a;
            Assert.Collections.AreEqual(new[] { 6, 5, 4, 3, 2, 1 }, m_view.Items);

            m_view.SortComparer = null;
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, m_view.Items);
        }

        [Test]
        public void Test_Items_can_be_filtered()
        {
            m_view.Reset(new[] { 5, 1, 4, 3, 6, 2 });
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, m_view.Items);

            m_view.Filter = a => a % 2 == 0;
            Assert.Collections.AreEqual(new[] { 2, 4, 6 }, m_view.Items);

            m_view.Filter = null;
            Assert.Collections.AreEqual(new[] { 1, 2, 3, 4, 5, 6 }, m_view.Items);
        }

        #endregion
    }
}
