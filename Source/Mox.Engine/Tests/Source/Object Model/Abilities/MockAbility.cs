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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mox.Flow;
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Mox
{
    public interface IMockAbility
    {
        MockAbility.Impl BaseImplementation { get; }
    }

    public interface IMockAbility<TImplementation> : IMockAbility
        where TImplementation : MockAbility.Impl
    {
        TImplementation Implementation { get; }
    }

    public class MockAbility : Ability, IMockAbility<MockAbility.Impl>
    {
        #region Inner Types

        public abstract class Impl
        {
            public abstract IEnumerable<ImmediateCost> Play(Spell spell);
        }

        #endregion

        #region Variables

        public static readonly Property<int> MockPropertyProperty = Property<int>.RegisterProperty("MockProperty", typeof (MockAbility));

        #endregion

        #region Properties

        public Impl Implementation
        {
            get;
            internal set;
        }

        Impl IMockAbility.BaseImplementation
        {
            get { return Implementation; }
        }

        public int MockProperty
        {
            get { return GetValue(MockPropertyProperty); }
            set { SetValue(MockPropertyProperty, value); }
        }

        #endregion

        #region Overrides of Ability

        public AbilityType MockedAbilityType
        {
            get;
            set;
        }

        public override AbilityType AbilityType
        {
            get
            {
                return MockedAbilityType;
            }
        }

        public bool MockedIsManaAbility
        {
            get
            {
                return IsManaAbility;
            }
            set 
            {
                MockedManaOutcome = value ? ManaAbilityOutcome.Any : ManaAbilityOutcome.None;
            }
        }

        public ManaAbilityOutcome MockedManaOutcome
        {
            get;
            set;
        }

        public override ManaAbilityOutcome ManaOutcome
        {
            get
            {
                return MockedManaOutcome ?? base.ManaOutcome;
            }
        }

        public AbilitySpeed MockedAbilitySpeed
        {
            get;
            set;
        }

        public override AbilitySpeed AbilitySpeed
        {
            get
            {
                return MockedAbilitySpeed;
            }
        }

        /// <summary>
        /// Initializes the given spell and returns the "pre payment" costs associated with the spell (asks players for modal choices, {X} choices, etc...)
        /// </summary>
        /// <param name="spell"></param>
        public override IEnumerable<ImmediateCost> Play(Spell spell)
        {
            return Implementation.Play(spell);
        }

        public override string ToString()
        {
            return string.Format("[MockAbility ({0}) on card {1} ({2})]", Identifier, Source, Source.Identifier);
        }

        #endregion
    }

    public static class MockAbilityExtensions
    {
        #region Expectations

        #region Can/Cannot Play

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_CanPlay(this IMockAbility ability)
        {
            return Expect_Play(ability);
        }

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_CannotPlay(this IMockAbility ability)
        {
            return Expect_Play(ability, new[] { Cost.CannotPlay }, null);
        }

        #endregion

        #region Expect_Play

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_Play(this IMockAbility ability)
        {
            return Expect_Play(ability, null);
        }

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_Play(this IMockAbility ability, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts)
        {
            return Expect_Play(ability, immediateCosts, delayedCosts, null);
        }

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_Play(this IMockAbility ability, Action<Spell> playCallback)
        {
            return Expect_Play(ability, null, null, playCallback);
        }

        public static IMethodOptions<IEnumerable<ImmediateCost>> Expect_Play(this IMockAbility ability, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts, Action<Spell> playCallback)
        {
            return Expect.Call(ability.BaseImplementation.Play(null))
                .IgnoreArguments()
                .Callback<Spell>(spell =>
                {
                    if (delayedCosts != null)
                    {
                        delayedCosts.ForEach(spell.DelayedCosts.Add);
                    }
                    if (playCallback != null)
                    {
                        playCallback(spell);
                    }
                    return true;
                })
                .Return(immediateCosts);
        }

        #endregion

        #region Expect_Play_and_execute_costs

        public static void Expect_Play_and_execute_costs(this IMockAbility ability, Player player, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts)
        {
            Expect_Play_and_execute_costs(ability, player, immediateCosts, delayedCosts, null);
        }

        public static void Expect_Play_and_execute_costs(this IMockAbility ability, Player player, IEnumerable<ImmediateCost> immediateCosts, IEnumerable<DelayedCost> delayedCosts, Action<Spell> spellCallback)
        {
            Expect_Play(ability, immediateCosts, delayedCosts, spellCallback);

            if (immediateCosts != null)
            {
                immediateCosts.ForEach(cost => cost.Expect_Execute(player, true));
            }

            if (delayedCosts != null)
            {
                delayedCosts.ForEach(cost => cost.Expect_Execute(player, true));
            }
        }

        #endregion

        #endregion
    }
}
