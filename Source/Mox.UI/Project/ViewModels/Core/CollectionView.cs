using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;

namespace Mox.UI
{
    public class CollectionView<T> : PropertyChangedBase
    {
        private readonly List<T> m_allItems = new List<T>();
        private readonly List<T> m_filteredItems = new List<T>();
        private readonly CurrentItems m_currentItems;

        public CollectionView()
        {
            m_currentItems = new CurrentItems(this);
        }

        public IReadOnlyCollection<T> Items
        {
            get { return m_currentItems; }
        }

        private IList<T> FilteredItems
        {
            get { return m_filteredItems; }
        }

        #region Paging

        private int m_currentPage;
        private int m_pageSize = int.MaxValue;

        public int CurrentPage
        {
            get { return m_currentPage; }
            set
            {
                if (m_currentPage != value)
                {
                    m_currentPage = value;

                    m_currentPage = Math.Max(m_currentPage, 0);
                    m_currentPage = Math.Min(m_currentPage, PageCount - 1);

                    NotifyOfPropertyChange();
                    RefreshItems();
                }
            }
        }

        public int PageSize
        {
            get { return m_pageSize; }
            set
            {
                if (m_pageSize != value)
                {
                    m_pageSize = Math.Max(1, value);
                    m_currentPage = Math.Min(m_currentPage, PageCount - 1);

                    NotifyOfPropertyChange();
                    NotifyOfPropertyChange(nameof(CurrentPage));
                    NotifyOfPropertyChange(nameof(PageCount));
                    RefreshItems();
                }
            }
        }

        public int PageCount
        {
            get
            {
                return Math.Max(1, (FilteredItems.Count + m_pageSize - 1) / m_pageSize);
            }
        }

        #endregion

        #region Sorting

        private Comparison<T> m_sortComparer;

        public Comparison<T> SortComparer
        {
            get { return m_sortComparer; }
            set
            {
                if (m_sortComparer != value)
                {
                    m_sortComparer = value;
                    SortItems();
                    RefreshItems();
                }
            }
        }

        #endregion

        #region Filtering

        private Predicate<T> m_filter;

        public Predicate<T> Filter
        {
            get { return m_filter; }
            set
            {
                if (m_filter != value)
                {
                    m_filter = value;
                    Refresh();
                }
            }
        }

        #endregion

        public void Reset(IEnumerable<T> items)
        {
            m_allItems.Clear();
            m_allItems.AddRange(items);

            Refresh();
        }

        public new void Refresh()
        {
            m_currentPage = 0;

            FilterItems();
            SortItems();
            RefreshItems();

            NotifyOfPropertyChange(nameof(CurrentPage));
            NotifyOfPropertyChange(nameof(PageCount));
        }

        private void FilterItems()
        {
            m_filteredItems.Clear();

            if (m_filter == null)
            {
                m_filteredItems.AddRange(m_allItems);
            }
            else
            {
                foreach (var item in m_allItems)
                {
                    if (m_filter(item))
                        m_filteredItems.Add(item);
                }
            }
        }

        private void SortItems()
        {
            if (m_sortComparer != null)
                m_filteredItems.Sort(m_sortComparer);
            else
                m_filteredItems.Sort();
        }

        private void RefreshItems()
        {
            m_currentItems.OnCollectionChanged();
        }

        private class CurrentItems : IReadOnlyCollection<T>, INotifyCollectionChanged
        {
            private readonly CollectionView<T> m_view;

            public CurrentItems(CollectionView<T> view)
            {
                m_view = view;
            }

            public int StartIndex
            {
                get { return m_view.PageSize * m_view.CurrentPage; }
            }

            public int Count
            {
                get
                {
                    var items = m_view.FilteredItems;
                    int startIndex = StartIndex;
                    int remaining = items.Count - startIndex;
                    return Math.Min(remaining, m_view.PageSize);
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                var items = m_view.FilteredItems;
                int start = StartIndex;
                int end = Math.Min(start + m_view.PageSize, items.Count);

                for (int i = start; i < end; i++)
                    yield return items[i];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            internal void OnCollectionChanged()
            {
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }
    }
}
