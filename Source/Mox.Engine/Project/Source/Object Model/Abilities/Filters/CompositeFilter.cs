using System;
using System.Collections;
using System.Collections.Generic;

namespace Mox.Abilities
{
#warning todo spell_v2 test filters
    public abstract class CompositeFilter : Filter, IEnumerable<Filter>
    {
        public List<Filter> Filters { get; } = new List<Filter>();

        public void Add(Filter f)
        {
            Filters.Add(f);
        }

        public IEnumerator<Filter> GetEnumerator()
        {
            return Filters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class AndFilter : CompositeFilter
    {
        public override FilterType FilterType
        {
            get
            {
                FilterType type = FilterType.All;

                foreach (var filter in Filters)
                {
                    type &= filter.FilterType;
                }

                return type;
            }
        }

        public override bool Accept(GameObject o, Player controller)
        {
            foreach (var filter in Filters)
            {
                if (!filter.Accept(o, controller))
                    return false;
            }

            return true;
        }
    }
}
