using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mox.Flow;

namespace Mox.Abilities
{
    [Serializable]
    public abstract class ObjectResolver
    {
        public abstract IEnumerable<GameObject> Resolve();

        public IEnumerable<T> Resolve<T>()
        {
            return Resolve().OfType<T>();
        }
    }

    [Serializable]
    public class Spell2
    {
        public List<Cost> Costs { get; } = new List<Cost>();
        public List<Action> Actions { get; } = new List<Action>();
    }

    public class TapAction : Action
    {
        public TapAction(ObjectResolver cards)
        {
            Debug.Assert(cards != null);
            Cards = cards;
        }

        public ObjectResolver Cards { get; private set; }

        public void Execute()
        {
            foreach (var card in Cards.Resolve<Card>())
            {
                card.Tap();
            }
        }

        public override Part Resolve()
        {
            throw new NotImplementedException();
        }
    }
}
