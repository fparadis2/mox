using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Abilities
{
    public class SpellContext
    {
        public SpellContext(Resolvable<SpellAbility> ability, Resolvable<Player> controller)
        {
            Ability = ability;
            Controller = controller;
        }

        public Resolvable<SpellAbility> Ability { get; }
        public Resolvable<Player> Controller { get; }
    }

#warning todo spell_v2 remove
    public class SpellResolutionContext
    {
        #region Constructor

        public SpellResolutionContext(Ability ability, Player controller)
        {
            Debug.Assert(ability != null);
            Debug.Assert(controller != null);

            Ability = ability;
            Controller = controller;
        }

        public SpellResolutionContext(Game game, Spell spell)
        {
            spell = spell.Resolve(game, false);

            Ability = spell.Ability;
            Controller = spell.Controller;
        }

        #endregion

        #region Properties

        public Game Game
        {
            get { return Ability.Manager; }
        }

        /// <summary>
        /// Source of the spell.
        /// </summary>
        public Card Source
        {
            get { return Ability.Source; }
        }

        /// <summary>
        /// Ability represented by the spell.
        /// </summary>
        public Ability Ability
        {
            get;
            private set;
        }

        /// <summary>
        /// Controller of the spell.
        /// </summary>
        public Player Controller
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        public TObject Resolve<TObject>(Resolvable<TObject> resolvable)
            where TObject : class, IObject
        {
            return resolvable.Resolve(Game);
        }

        public GameObject Resolve(TargetCost target)
        {
            return target.Resolve(Game);
        }

        public TTargetable Resolve<TTargetable>(TargetCost<TTargetable> target)
            where TTargetable : GameObject
        {
            return (TTargetable)Resolve((TargetCost)target);
        }

        #endregion
    }
}
