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
using NUnit.Framework;

namespace Mox.Effects
{
    /// <summary>
    /// Uses examples from comp. rules ("Interaction of continous effects") as tests.
    /// </summary>
    [TestFixture]
    public class ContinousEffectInteractionTests : BaseGameTests
    {
        #region Variables

        #endregion

        #region Utilities

        private Card CreateCreature(int power, int toughness)
        {
            Card card = CreateCard(m_playerA);
            card.Type = Type.Creature;
            card.Power = power;
            card.Toughness = toughness;
            card.Zone = m_game.Zones.Battlefield;
            return card;
        }

        private static void Assert_PT_is(Card creature, int power, int toughness)
        {
            Assert.AreEqual(power, creature.Power, "Power");
            Assert.AreEqual(toughness, creature.Toughness, "Toughness");
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Switch_PT_1()
        {
            // Example: A 1/3 creature is given +0/+1 by an effect. 
            // Then another effect switches the creature's power and toughness. Its new power and toughness is 4/1. 
            // A new effect gives the creature +5/+0. Its "unswitched" power and toughness would be 6/4, so its actual power and toughness is 4/6.

            Card creature = CreateCreature(1, 3);
            AddEffect.On(creature).ModifyPowerAndToughness(+0, +1).Forever();
            Assert_PT_is(creature, 1, 4);

            AddEffect.On(creature).SwitchPowerAndToughness().Forever();
            Assert_PT_is(creature, 4, 1);

            AddEffect.On(creature).ModifyPowerAndToughness(+5, +0).Forever();
            Assert_PT_is(creature, 4, 6);
        }

        [Test]
        public void Test_Switch_PT_2()
        {
            // Example: A 1/3 creature is given +0/+1 by an effect. 
            // Then another effect switches the creature's power and toughness. Its new power and toughness is 4/1. 
            // If the +0/+1 effect ends before the switch effect ends, the creature becomes 3/1.

            Card creature = CreateCreature(1, 3);
            using (AddEffect.On(creature).ModifyPowerAndToughness(+0, +1).DuringScope())
            {
                Assert_PT_is(creature, 1, 4);

                AddEffect.On(creature).SwitchPowerAndToughness().Forever();
                Assert_PT_is(creature, 4, 1);
            }
            Assert_PT_is(creature, 3, 1);
        }

        private class CrusadeAbility : ContinuousAbility
        {
            private static readonly Property<Color> ColorProperty = Property<Color>.RegisterProperty("Color", typeof (CrusadeAbility), PropertyFlags.Modifiable, Color.White);

            protected override IEnumerable<IEffectCreator> AddEffects()
            {
                yield return AddEffect.OnCreatures(Manager, Condition.IsSameColorThan(this, ColorProperty)).ModifyPowerAndToughness(+1, +1);
            }
        }

        [Test]
        public void Test_Changes_are_instantaneous_1()
        {
            // Example: Crusade is an enchantment that reads "White creatures get +1/+1." 
            // Crusade and a 2/2 black creature are on the battlefield. 
            // If an effect then turns the creature white (layer 5), it gets +1/+1 from Crusade (layer 7c), becoming 3/3. 
            // If the creature's color is later changed to red (layer 5), Crusade's effect stops applying to it, and it will return to being 2/2.

            Card crusade = CreateCard(m_playerA);
            m_game.CreateAbility<CrusadeAbility>(crusade);
            crusade.Type = Type.Enchantment;
            crusade.Zone = m_game.Zones.Battlefield;

            Card creature = CreateCreature(2, 2);
            creature.Color = Color.Black;

            Assert_PT_is(creature, 2, 2);

            AddEffect.On(creature).ChangeColor(Color.White).Forever();
            Assert_PT_is(creature, 3, 3);

            AddEffect.On(creature).ChangeColor(Color.Red).Forever();
            Assert_PT_is(creature, 2, 2);
        }

        private class CreaturesYouControlGetPlus0Plus2 : ContinuousAbility
        {
            protected override IEnumerable<IEffectCreator> AddEffects()
            {
                yield return AddEffect.OnCreatures(Manager, Condition.ControlledBySameController(Source)).ModifyPowerAndToughness(+0, +2);
            }
        }

        [Test]
        public void Test_Changes_are_instantaneous_2()
        {
            // Example: Gray Ogre, a 2/2 creature, is on the battlefield. 
            // An effect puts a +1/+1 counter on it (layer 7d), making it 3/3. 
            // A spell targeting it that says "Target creature gets +4/+4 until end of turn" resolves (layer 7c), making it 7/7. 
            // An enchantment that says "Creatures you control get +0/+2" enters the battlefield (layer 7c), making it 7/9. 
            // An effect that says "Target creature becomes 0/1 until end of turn" is applied to it (layer 7b), making it 5/8 (0/1, plus +4/+4 from the resolved spell, plus +0/+2 from the enchantment, plus +1/+1 from the counter).

            Card grayOgre = CreateCreature(2, 2);
            Assert_PT_is(grayOgre, 2, 2);

#warning [LOW] make counter when counters are implemented
            AddEffect.On(grayOgre).ModifyPowerAndToughness(+1, +1).Forever();
            Assert_PT_is(grayOgre, 3, 3);

            AddEffect.On(grayOgre).ModifyPowerAndToughness(+4, +4).UntilEndOfTurn();
            Assert_PT_is(grayOgre, 7, 7);

            Card enchantment = CreateCard(m_playerA);
            m_game.CreateAbility<CreaturesYouControlGetPlus0Plus2>(enchantment);
            enchantment.Zone = m_game.Zones.Battlefield;

            Assert_PT_is(grayOgre, 7, 9);

            AddEffect.On(grayOgre).SetPowerAndToughness(0, 1).UntilEndOfTurn();
            Assert_PT_is(grayOgre, 5, 8);
        }

        #endregion
    }
}
