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
using System.Diagnostics;
using System.Linq;

namespace Mox
{
    /// <summary>
    /// A (possibly partial) mana payment
    /// </summary>
    [Serializable]
    public struct ManaPayment2
    {
        public byte Colorless;
        public byte White;
        public byte Blue;
        public byte Black;
        public byte Red;
        public byte Green;
        public byte Phyrexian;

        public const int NumColors = 6;
        public byte this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Colorless;
                    case 1: return White;
                    case 2: return Blue;
                    case 3: return Black;
                    case 4: return Red;
                    case 5: return Green;
                    default: throw new InvalidProgramException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: Colorless = value; return;
                    case 1: White = value; return;
                    case 2: Blue = value; return;
                    case 3: Black = value; return;
                    case 4: Red = value; return;
                    case 5: Green = value; return;
                    default: throw new InvalidProgramException();
                }
            }
        }

        #region Operators

        public static implicit operator ManaPayment2(ManaAmount amount)
        {
            return new ManaPayment2
            {
                Colorless = amount.Colorless,
                White = amount.White,
                Blue = amount.Blue,
                Black = amount.Black,
                Red = amount.Red,
                Green = amount.Green,
            };
        }

        public static ManaPayment2 operator+(ManaPayment2 a, ManaPayment2 b)
        {
            return new ManaPayment2
            {
                Colorless = (byte)(a.Colorless + b.Colorless),
                White = (byte)(a.White + b.White),
                Blue = (byte)(a.Blue + b.Blue),
                Black = (byte)(a.Black + b.Black),
                Red = (byte)(a.Red + b.Red),
                Green = (byte)(a.Green + b.Green),
                Phyrexian = (byte)(a.Phyrexian + b.Phyrexian)
            };
        }

        public override string ToString()
        {
            return $"{Colorless}C, {White}W, {Blue}U, {Black}B, {Red}R, {Green}G, {Phyrexian}P";
        }

        #endregion
    }

    /// <summary>
    /// A (possibly partial) mana payment
    /// </summary>
    [Serializable]
    public class ManaPayment
    {
        #region Variables

        private readonly List<Color> m_colors = new List<Color>();

        #endregion

        #region Constructor

        public ManaPayment()
        {
        }

        private ManaPayment(IEnumerable<Color> payment)
        {
            m_colors.AddRange(payment);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The mana atoms used to pay mana.
        /// </summary>
        public ICollection<Color> Payments
        {
            get { return m_colors.AsReadOnly(); }
        }

        /// <summary>
        /// Whether this is an empty payment.
        /// </summary>
        public bool IsEmpty
        {
            get { return m_colors.Count == 0; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Pays mana using one token of the given <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Token color</param>
        public void Pay(Color color)
        {
            Pay(color, 1);
        }

        /// <summary>
        /// Pays mana using <paramref name="amount"/> token of the given <paramref name="color"/>.
        /// </summary>
        /// <param name="color">Token color</param>
        /// <param name="amount">Number of tokens to pay with.</param>
        public void Pay(Color color, int amount)
        {
            Throw.ArgumentOutOfRangeIf(amount < 0, "amount must be positive", "amount");
            for (int i = 0; i < amount; i++)
            {
                m_colors.Add(color);
            }
        }

        #region EnumerateCompletePayments

        /// <summary>
        /// Enumerates the complete unique payments that it is possible to make for the given <paramref name="cost"/>, from the given <paramref name="pool"/>.
        /// </summary>
        /// <param name="cost"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static IEnumerable<ManaPayment> EnumerateCompletePayments(ManaCost cost, ManaPool pool)
        {
            Throw.IfNull(cost, "cost");
            Throw.IfNull(pool, "pool");
            Throw.InvalidArgumentIf(cost.IsEmpty, "Cost is empty", "cost");
            Throw.InvalidArgumentIf(!cost.IsConcrete, "Cost is not concrete", "cost");

            // Early bail-out
            if (!ValidatePaymentPossibility(cost, pool))
            {
                yield break;
            }

            // We first try to pay the colored symbols
            List<Color> basePayment = new List<Color>();
            ManaPool workingPool = new ManaPool(pool);
            if (!TryPayColoredSymbols(ref cost, workingPool, basePayment))
            {
                yield break;
            }

            // Always pay colorless with colorless in priority
            int numColorlessPay = Math.Min(workingPool[Color.None], cost.Colorless);
            if (numColorlessPay > 0)
            {
                workingPool[Color.None] -= numColorlessPay;
                cost = cost.RemoveColorless(numColorlessPay);

                for (int i = 0; i < numColorlessPay; i++)
                {
                    basePayment.Add(Color.None);
                }
            }

            // Pay colorless with remaining mana
            foreach (var colorlessPayment in EnumerateColorlessPayments(cost.Colorless, workingPool))
            {
                var combinedPayment = new[] { basePayment, colorlessPayment }.SelectMany(p => p);
                yield return new ManaPayment(combinedPayment);
            }
        }

        private static bool ValidatePaymentPossibility(ManaCost cost, ManaPool pool)
        {
            return pool.TotalManaAmount >= cost.ConvertedValue;
        }

        private static bool TryPayColoredSymbols(ref ManaCost cost, ManaPool workingPool, ICollection<Color> payment)
        {
            foreach (ManaSymbol symbol in cost.Symbols)
            {
                Color payColor;
                if (!TryPay(ref cost, symbol, workingPool, out payColor))
                {
                    return false;
                }
                payment.Add(payColor);
            }

            return true;
        }

        private static IEnumerable<IEnumerable<Color>> EnumerateColorlessPayments(int numColorless, ManaPool pool)
        {
            Debug.Assert(numColorless == 0 || pool[Color.None] == 0, "Should have used all the colorless mana by now");

            int[] poolList = new int[ms_colors.Length];
            for (int i = 0; i < ms_colors.Length; i++)
            {
                poolList[i] = pool[ms_colors[i]];
            }

            foreach (var groupCombination in poolList.EnumerateGroupCombinations(numColorless))
            {
                yield return groupCombination.Select(i => ms_colors[i]);
            }
        }

        private static readonly Color[] ms_colors = new[] { Color.Black, Color.Blue, Color.Green, Color.Red, Color.White };

        #endregion

        #region GetMaximalRemainingPayment

        public static ManaPayment GetMaximalRemainingPayment(ManaCost cost, ManaPool pool)
        {
            ManaPool workingPool = new ManaPool(pool);
            List<Color> payment = new List<Color>();

            // We first try to pay the colored symbols
            foreach (ManaSymbol symbol in cost.Symbols)
            {
                Color payColor;
                if (!TryPay(ref cost, symbol, workingPool, out payColor))
                {
                    payment.Add(payColor);
                }
            }

            // Pay colorless with the rest
            int numColorlessPay = Math.Max(0, cost.Colorless - workingPool.TotalManaAmount);
            if (numColorlessPay > 0)
            {
                payment.AddRange(Enumerable.Repeat(Color.None, numColorlessPay));
            }

            return new ManaPayment(payment);
        }

        #endregion

        #region GetMaximalTrivialPayment

        public static ManaPayment GetMaximalTrivialPayment(ManaCost cost, ManaPool pool)
        {
            ManaPool workingPool = new ManaPool(pool);
            List<Color> payment = new List<Color>();

            List<ManaSymbol> symbolsToPay = cost.Symbols.ToList();
            symbolsToPay.Sort();

            // We first try to pay the colored symbols
            bool paidAllColored = true;
            for (int i = symbolsToPay.Count; i-- > 0;)
            {
                ManaSymbol symbol = symbolsToPay[i];

                Color color;
                if (TryPayTrivial(workingPool, symbol, out color))
                {
                    payment.Add(color);
                }
                else
                {
                    paidAllColored = false;
                }
            }

            // We can pay the rest if possible
            if (paidAllColored && cost.Colorless > 0)
            {
                PayColorlessTrivial(cost.Colorless, workingPool, payment);
            }

            return new ManaPayment(payment);
        }

        private static bool TryPayTrivial(ManaPool pool, ManaSymbol symbol, out Color color)
        {
            color = Color.None;

            switch (symbol)
            {
                case ManaSymbol.S:
                    return false; // todo?

                case ManaSymbol.C:
                    return TryPaySingleTrivial(pool, Color.None, ref color);
                case ManaSymbol.W:
                    return TryPaySingleTrivial(pool, Color.White, ref color);
                case ManaSymbol.U:
                    return TryPaySingleTrivial(pool, Color.Blue, ref color);
                case ManaSymbol.B:
                    return TryPaySingleTrivial(pool, Color.Black, ref color);
                case ManaSymbol.R:
                    return TryPaySingleTrivial(pool, Color.Red, ref color);
                case ManaSymbol.G:
                    return TryPaySingleTrivial(pool, Color.Green, ref color);

                case ManaSymbol.BG:
                case ManaSymbol.BR:
                case ManaSymbol.GU:
                case ManaSymbol.GW:
                case ManaSymbol.RG:
                case ManaSymbol.RW:
                case ManaSymbol.UB:
                case ManaSymbol.UR:
                case ManaSymbol.WB:
                case ManaSymbol.WU:
                    return false; // Never trivial?

                case ManaSymbol.W2:
                case ManaSymbol.U2:
                case ManaSymbol.B2:
                case ManaSymbol.R2:
                case ManaSymbol.G2:
                    return false; // Never trivial?

                case ManaSymbol.WP:
                case ManaSymbol.UP:
                case ManaSymbol.BP:
                case ManaSymbol.RP:
                case ManaSymbol.GP:
                    return false; // Never trivial?

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return false; // Never trivial?

                default:
                    throw new ArgumentException("Invalid mana symbol");
            }
        }

        private static bool TryPaySingleTrivial(ManaPool pool, Color color, ref Color paidColor)
        {
            if (pool[color] > 0)
            {
                pool[color]--;
                paidColor = color;
                return true;
            }

            return false;
        }

        private static void PayColorlessTrivial(int colorless, ManaPool pool, List<Color> payment)
        {
            Color color = Color.None;
            if (!pool.TryGetSingleColor(ref color))
                return;

            int count = pool[color];
            int payable = Math.Min(colorless, count);

            pool[color] -= payable;
            for (int i = 0; i < payable; i++)
                payment.Add(color);
        }

        #endregion

        #region Utilities

        private static bool TryPay(ref ManaCost cost, ManaSymbol symbol, ManaPool workingPool, out Color color)
        {
            Throw.InvalidArgumentIf(ManaSymbolHelper.IsHybrid(symbol), "TODO: Implement hybrid costs", "cost");
            Debug.Assert(symbol != ManaSymbol.X);
            Debug.Assert(symbol != ManaSymbol.Y);
            Debug.Assert(symbol != ManaSymbol.Z);
            Debug.Assert(symbol != ManaSymbol.S, "TODO");

            color = ManaSymbolHelper.GetColor(symbol);
            if (workingPool[color] > 0)
            {
                // Ok, this can be paid.
                workingPool[color]--;
                cost = cost.Remove(symbol);
                return true;
            }

            // Cannot pay the cost
            return false;
        }

        public override string ToString()
        {
            // Debug only
            var symbols = m_colors.Select(ManaSymbolHelper.GetSymbol);
            return new ManaCost(0, symbols.ToArray()).ToString();
        }

        #endregion

        #endregion
    }
}
