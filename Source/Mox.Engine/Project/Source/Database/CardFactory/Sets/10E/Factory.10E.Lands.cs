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

using Mox.Database.Library;

namespace Mox.Database.Sets
{
    #region Lands

    [CardFactory("Adarkar Wastes")]
    [CardFactory("Battlefield Forge")]
    [CardFactory("Brushland")]
    [CardFactory("Caves of Koilos")]
    [CardFactory("Karplusan Forest")]
    [CardFactory("Llanowar Wastes")]
    [CardFactory("Shivan Reef")]
    [CardFactory("Sulfurous Springs")]
    [CardFactory("Underground River")]
    [CardFactory("Yavimaya Coast")]
    public class PainLandCardFactory : CardFactory
    {
        // T Add 1 to your mana pool.
        // T Add X or Y to your mana pool. Z deals 1 damage to you.

        #region Abilities

        private class TapPainLandAbility : TapForManaAbility
        {
            #region Overrides

            protected override void OnResolve(Spell spell)
            {
                base.OnResolve(spell);
                spell.Controller.DealDamage(1);
            }

            #endregion
        }

        #endregion

        #region Overrides of CardFactory

        protected override void Initialize(Card card)
        {
            base.Initialize(card);

            TapForManaAbility tapLand = CreateAbility<TapForManaAbility>(card);
            tapLand.Color = Color.None;

            Color color1;
            Color color2;
            GetColors(card, out color1, out color2);

            TapPainLandAbility tapLandForDamage1 = CreateAbility<TapPainLandAbility>(card);
            tapLandForDamage1.Color = color1;

            TapPainLandAbility tapLandForDamage2 = CreateAbility<TapPainLandAbility>(card);
            tapLandForDamage2.Color = color2;
        }

        private static void GetColors(Card card, out Color color1, out Color color2)
        {
            switch (card.Name)
            {
                case "Adarkar Wastes":
                    color1 = Color.White;
                    color2 = Color.Blue;
                    break;

                case "Battlefield Forge": 
                    color1 = Color.Red;
                    color2 = Color.White;
                    break;

                case "Brushland": 
                    color1 = Color.Green; 
                    color2 = Color.White;
                    break;

                case "Caves of Koilos": 
                    color1 = Color.White;
                    color2 = Color.Black;
                    break;

                case "Karplusan Forest": 
                    color1 = Color.Red;
                    color2 = Color.Green;
                    break;

                case "Llanowar Wastes": 
                    color1 = Color.Black;
                    color2 = Color.Green;
                    break;

                case "Shivan Reef": 
                    color1 = Color.Blue;
                    color2 = Color.Red;
                    break;

                case "Sulfurous Springs": 
                    color1 = Color.Black;
                    color2 = Color.Red;
                    break;

                case "Underground River": 
                    color1 = Color.Blue;
                    color2 = Color.Black;
                    break;

                case "Yavimaya Coast": 
                    color1 = Color.Green;
                    color2 = Color.Blue;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion
    }

    [CardFactory("Quicksand")]
    public class QuicksandCardFactory : CardFactory
    {
        // T, Sacrifice Quicksand: Target attacking creature without flying gets -1/-2 until end of turn.
        private class SacrificeAbility : InPlayAbility
        {
            public override void Play(Spell spell)
            {
                spell.AddCost(Tap(spell.Source));

                var target = Target.Creature().Attacking().Without<FlyingAbility>();
                spell.AddCost(target);

                spell.Effect = s =>
                {
                    AddEffect.On(s.Resolve(target)).ModifyPowerAndToughness(-1, -2).UntilEndOfTurn();
                };
            }
        }

        protected override void Initialize(Card card)
        {
            base.Initialize(card);
            CreateAbility<TapForManaAbility>(card);
            CreateAbility<SacrificeAbility>(card);
        }
    }

    #endregion
}
