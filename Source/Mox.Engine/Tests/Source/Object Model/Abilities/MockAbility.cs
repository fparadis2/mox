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
using Rhino.Mocks;
using Rhino.Mocks.Interfaces;

namespace Mox.Abilities
{
    public interface IMockAbility
    {
        MockAbility.Impl BaseImplementation { get; }
    }

    public interface IMockAbility<out TImplementation> : IMockAbility
        where TImplementation : MockAbility.Impl
    {
        TImplementation Implementation { get; }
    }

    public class MockAbility : Ability, IMockAbility<MockAbility.Impl>
    {
        #region Inner Types

        public abstract class Impl
        {
            public abstract void Play(Spell spell);
        }

        #endregion

        #region Variables

        private int m_mockProperty;
        public static readonly Property<int> MockPropertyProperty = Property<int>.RegisterProperty<MockAbility>("MockProperty", a => a.m_mockProperty);

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
            get { return m_mockProperty; }
            set { SetValue(MockPropertyProperty, value, ref m_mockProperty); }
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

        public bool MockedIsManaAbility { get; set; }

        public override bool IsManaAbility => MockedIsManaAbility;

        public override void FillManaOutcome(IManaAbilityOutcome outcome)
        {
            outcome.AddAny();
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
        public override void Play(Spell spell)
        {
            Implementation.Play(spell);
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

        public static IMethodOptions<object> Expect_CanPlay(this IMockAbility ability)
        {
            return Expect_Play(ability);
        }

        public static IMethodOptions<object> Expect_CannotPlay(this IMockAbility ability)
        {
            return Expect_Play(ability, new[] { Cost.CannotPlay }, null);
        }

        #endregion

        #region Expect_Play

        public static IMethodOptions<object> Expect_Play(this IMockAbility ability)
        {
            return Expect_Play(ability, Enumerable.Empty<Cost>());
        }

        public static IMethodOptions<object> Expect_Play(this IMockAbility ability, IEnumerable<Cost> costs)
        {
            return Expect_Play(ability, costs, null);
        }

        public static IMethodOptions<object> Expect_Play(this IMockAbility ability, Action<Spell> playCallback)
        {
            return Expect_Play(ability, null, playCallback);
        }

        public static IMethodOptions<object> Expect_Play(this IMockAbility ability, IEnumerable<Cost> costs, Action<Spell> playCallback)
        {
            ability.BaseImplementation.Play(null);

            return LastCall
                .IgnoreArguments()
                .Callback<Spell>(spell =>
                {
                    if (costs != null)
                    {
                        costs.ForEach(spell.AddCost);
                    }
                    if (playCallback != null)
                    {
                        playCallback(spell);
                    }
                    return true;
                });
        }

        #endregion

        #region Expect_Play_and_execute_costs

        public static void Expect_Play_and_execute_costs(this IMockAbility ability, Player player, IEnumerable<Cost> costs)
        {
            Expect_Play_and_execute_costs(ability, player, costs, null);
        }

        public static void Expect_Play_and_execute_costs(this IMockAbility ability, Player player, IEnumerable<Cost> costs, Action<Spell> spellCallback)
        {
            Expect_Play(ability, costs, spellCallback);

            if (costs != null)
            {
                costs.ForEach(cost => cost.Expect_Execute(player, true));
            }
        }

        #endregion

        #endregion
    }
}
