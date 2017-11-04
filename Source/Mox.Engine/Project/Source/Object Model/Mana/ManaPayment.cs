﻿// Copyright (c) François Paradis
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
    public struct ManaPaymentAmount
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

        public static implicit operator ManaPaymentAmount(ManaAmount amount)
        {
            return new ManaPaymentAmount
            {
                Colorless = amount.Colorless,
                White = amount.White,
                Blue = amount.Blue,
                Black = amount.Black,
                Red = amount.Red,
                Green = amount.Green,
            };
        }

        public static ManaPaymentAmount operator+(ManaPaymentAmount a, ManaPaymentAmount b)
        {
            return new ManaPaymentAmount
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

#warning [Mana] Rename
    [Serializable]
    public struct ManaPaymentNew
    {
        #region Fields

        public ManaPaymentAmount[] Atoms;
        public ManaPaymentAmount Generic;

        #endregion

        #region Methods

        public ManaPaymentAmount GetTotalAmount()
        {
            ManaPaymentAmount amount = Generic;

            if (Atoms != null)
            {
                foreach (var atom in Atoms)
                {
                    amount += atom;
                }
            }

            return amount;
        }

        public bool TryPay(ManaCost cost, out ManaCost remaining)
        {
            Debug.Assert(!cost.IsEmpty);
            remaining = cost;

            // Validation
            int atomCount = Atoms != null ? Atoms.Length : 0;
            if (atomCount != cost.SortedSymbols.Count)
                return false;

            byte generic = cost.Generic;
            if (!TryPayGeneric(ref generic))
                return false;

            List<ManaSymbol> remainingSymbols = new List<ManaSymbol>(cost.SortedSymbols.Count);
            for (int i = 0; i < cost.SortedSymbols.Count; i++)
            {
                int atomTotal = GetTotal(Atoms[i]);
                if (atomTotal == 0)
                {
                    remainingSymbols.Add(cost.SortedSymbols[i]);
                    continue;
                }

                if (!TryPaySymbol(cost.SortedSymbols[i], Atoms[i], atomTotal))
                    return false;
            }

#warning [Mana] Remove ToArray()
            remaining = new ManaCost(generic, remainingSymbols.ToArray());
            return true;
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();

            AddToString(builder, Generic);

            if (Atoms != null)
            {
                foreach (var atom in Atoms)
                {
                    AddToString(builder, atom);
                }
            }

            return builder.ToString();
        }

        private static void AddToString(System.Text.StringBuilder builder, ManaPaymentAmount atom)
        {
            int atomTotal = GetTotal(atom);
            if (atomTotal == 0)
            {
                builder.Append("0");
            }

            if (atomTotal > 1)
                builder.Append('(');

            for (byte i = 0; i < atom.White; i++)
                builder.Append('W');

            for (byte i = 0; i < atom.Blue; i++)
                builder.Append('U');

            for (byte i = 0; i < atom.Black; i++)
                builder.Append('B');

            for (byte i = 0; i < atom.Red; i++)
                builder.Append('R');

            for (byte i = 0; i < atom.Green; i++)
                builder.Append('G');

            for (byte i = 0; i < atom.Colorless; i++)
                builder.Append('C');

            for (byte i = 0; i < atom.Phyrexian; i++)
                builder.Append('P');

            if (atomTotal > 1)
                builder.Append(')');
        }

        private static int GetTotal(ManaPaymentAmount atom)
        {
            return
                atom.Phyrexian +
                atom.Colorless +
                atom.White +
                atom.Blue +
                atom.Black +
                atom.Red +
                atom.Green;
        }

        private bool TryPayGeneric(ref byte generic)
        {
            if (Generic.Phyrexian > 0)
                return false;

            int total = 
                Generic.Colorless + 
                Generic.White + 
                Generic.Blue + 
                Generic.Black + 
                Generic.Red + 
                Generic.Green;

            if (total > generic)
                return false;

            generic -= (byte)total;
            return true;
        }

        private bool TryPaySymbol(ManaSymbol manaSymbol, ManaPaymentAmount atom, int atomTotal)
        {
            switch (manaSymbol)
            {
                case ManaSymbol.W: return TryPaySingleSymbol(atom.White, atomTotal);
                case ManaSymbol.U: return TryPaySingleSymbol(atom.Blue, atomTotal);
                case ManaSymbol.B: return TryPaySingleSymbol(atom.Black, atomTotal);
                case ManaSymbol.R: return TryPaySingleSymbol(atom.Red, atomTotal);
                case ManaSymbol.G: return TryPaySingleSymbol(atom.Green, atomTotal);
                case ManaSymbol.C: return TryPaySingleSymbol(atom.Colorless, atomTotal);

                case ManaSymbol.W2: return TryPayHybrid2Symbol(atom.White, atom, atomTotal);
                case ManaSymbol.U2: return TryPayHybrid2Symbol(atom.Blue, atom, atomTotal);
                case ManaSymbol.B2: return TryPayHybrid2Symbol(atom.Black, atom, atomTotal);
                case ManaSymbol.R2: return TryPayHybrid2Symbol(atom.Red, atom, atomTotal);
                case ManaSymbol.G2: return TryPayHybrid2Symbol(atom.Green, atom, atomTotal);

                case ManaSymbol.WP: return TryPayPhyrexianSymbol(atom.White, atom, atomTotal);
                case ManaSymbol.UP: return TryPayPhyrexianSymbol(atom.Blue, atom, atomTotal);
                case ManaSymbol.BP: return TryPayPhyrexianSymbol(atom.Black, atom, atomTotal);
                case ManaSymbol.RP: return TryPayPhyrexianSymbol(atom.Red, atom, atomTotal);
                case ManaSymbol.GP: return TryPayPhyrexianSymbol(atom.Green, atom, atomTotal);

                case ManaSymbol.WU: return TryPayHybridSymbol(atom.White, atom.Blue, atomTotal);
                case ManaSymbol.WB: return TryPayHybridSymbol(atom.White, atom.Black, atomTotal);
                case ManaSymbol.UB: return TryPayHybridSymbol(atom.Blue, atom.Black, atomTotal);
                case ManaSymbol.UR: return TryPayHybridSymbol(atom.Blue, atom.Red, atomTotal);
                case ManaSymbol.BR: return TryPayHybridSymbol(atom.Black, atom.Red, atomTotal);
                case ManaSymbol.BG: return TryPayHybridSymbol(atom.Black, atom.Green, atomTotal);
                case ManaSymbol.RG: return TryPayHybridSymbol(atom.Red, atom.Green, atomTotal);
                case ManaSymbol.RW: return TryPayHybridSymbol(atom.Red, atom.White, atomTotal);
                case ManaSymbol.GW: return TryPayHybridSymbol(atom.Green, atom.White, atomTotal);
                case ManaSymbol.GU: return TryPayHybridSymbol(atom.Green, atom.Blue, atomTotal);

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                case ManaSymbol.S:
                    throw new InvalidOperationException();

                default:
                    throw new NotSupportedException();
            }
        }

        private bool TryPaySingleSymbol(byte color, int atomTotal)
        {
            if (atomTotal != 1)
                return false;

            return color > 0;
        }

        private bool TryPayHybridSymbol(byte colorA, byte colorB, int atomTotal)
        {
            if (atomTotal != 1)
                return false;

            return colorA > 0 || colorB > 0;
        }

        private bool TryPayHybrid2Symbol(byte color, ManaPaymentAmount atom, int atomTotal)
        {
            return (color == 1 && atomTotal == 1) || (atom.Phyrexian == 0 && atomTotal == 2);
        }

        private bool TryPayPhyrexianSymbol(byte color, ManaPaymentAmount atom, int atomTotal)
        {
            if (atomTotal != 1)
                return false;

            return color > 0 || atom.Phyrexian > 0;
        }

        #endregion

        #region Creation

        public ManaPaymentNew Clone()
        {
            return new ManaPaymentNew
            {
                Atoms = (Atoms != null ? (ManaPaymentAmount[])Atoms.Clone() : null),
                Generic = Generic
            };
        }

        public static ManaPaymentNew Prepare(ManaCost cost)
        {
            var payment = new ManaPaymentNew();

            if (cost.Symbols.Count > 0)
                payment.Atoms = new ManaPaymentAmount[cost.Symbols.Count];

            return payment;
        }

        public static ManaPaymentNew CreateAnyFromCost(ManaCost cost)
        {
            var payment = Prepare(cost);

            for(int i = 0; i < cost.SortedSymbols.Count; i++)
            {
                payment.Atoms[i] = CreateAnyAtomFromSymbol(cost.SortedSymbols[i]);
            }

            payment.Generic = new ManaPaymentAmount { Colorless = cost.Generic };

            return payment;
        }

        private static ManaPaymentAmount CreateAnyAtomFromSymbol(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.W: return new ManaPaymentAmount { White = 1 };
                case ManaSymbol.U: return new ManaPaymentAmount { Blue = 1 };
                case ManaSymbol.B: return new ManaPaymentAmount { Black = 1 };
                case ManaSymbol.R: return new ManaPaymentAmount { Red = 1 };
                case ManaSymbol.G: return new ManaPaymentAmount { Green = 1 };
                case ManaSymbol.C: return new ManaPaymentAmount { Colorless = 1 };

                case ManaSymbol.W2: return new ManaPaymentAmount { White = 1 };
                case ManaSymbol.U2: return new ManaPaymentAmount { Blue = 1 };
                case ManaSymbol.B2: return new ManaPaymentAmount { Black = 1 };
                case ManaSymbol.R2: return new ManaPaymentAmount { Red = 1 };
                case ManaSymbol.G2: return new ManaPaymentAmount { Green = 1 };

                case ManaSymbol.WP: return new ManaPaymentAmount { White = 1 };
                case ManaSymbol.UP: return new ManaPaymentAmount { Blue = 1 };
                case ManaSymbol.BP: return new ManaPaymentAmount { Black = 1 };
                case ManaSymbol.RP: return new ManaPaymentAmount { Red = 1 };
                case ManaSymbol.GP: return new ManaPaymentAmount { Green = 1 };

                case ManaSymbol.WU: return new ManaPaymentAmount { White = 1 };
                case ManaSymbol.WB: return new ManaPaymentAmount { White = 1 };
                case ManaSymbol.UB: return new ManaPaymentAmount { Blue = 1 };
                case ManaSymbol.UR: return new ManaPaymentAmount { Blue = 1 };
                case ManaSymbol.BR: return new ManaPaymentAmount { Black = 1 };
                case ManaSymbol.BG: return new ManaPaymentAmount { Black = 1 };
                case ManaSymbol.RG: return new ManaPaymentAmount { Red = 1 };
                case ManaSymbol.RW: return new ManaPaymentAmount { Red = 1 };
                case ManaSymbol.GW: return new ManaPaymentAmount { Green = 1 };
                case ManaSymbol.GU: return new ManaPaymentAmount { Green = 1 };

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                case ManaSymbol.S:
                    throw new InvalidOperationException();

                default:
                    throw new NotSupportedException();
            }
        }

        private class TrivialPaymentSolver
        {
            public bool HasTrivialPayment;
            public ManaPaymentNew Payment;

            private ManaAmount m_amount;
            private byte m_genericCost;
            private readonly IList<ManaSymbol> m_symbols;
            private readonly List<int> m_symbolIndicesToPay = new List<int>();

            private struct TrivialPotential
            {
                public int PotentialWhite;
                public int PotentialBlue;
                public int PotentialBlack;
                public int PotentialRed;
                public int PotentialGreen;

                public void Reset()
                {
                    PotentialWhite = -1;
                    PotentialBlue = -1;
                    PotentialBlack = -1;
                    PotentialRed = -1;
                    PotentialGreen = -1;
                }

                public void ConsiderWhite(int i, ref ManaAmount amount)
                {
                    if (amount.White == 0)
                        return;

                    Consider(i, ref PotentialWhite);
                }

                public void ConsiderBlue(int i, ref ManaAmount amount)
                {
                    if (amount.Blue == 0)
                        return;

                    Consider(i, ref PotentialBlue);
                }

                public void ConsiderBlack(int i, ref ManaAmount amount)
                {
                    if (amount.Black == 0)
                        return;

                    Consider(i, ref PotentialBlack);
                }

                public void ConsiderRed(int i, ref ManaAmount amount)
                {
                    if (amount.Red == 0)
                        return;

                    Consider(i, ref PotentialRed);
                }

                public void ConsiderGreen(int i, ref ManaAmount amount)
                {
                    if (amount.Green == 0)
                        return;

                    Consider(i, ref PotentialGreen);
                }

                private static void Consider(int i, ref int potential)
                {
                    if (potential == -1)
                        potential = i;
                    else
                        potential = int.MinValue;
                }
            }

            public TrivialPaymentSolver(ManaCost cost, ManaAmount amount)
            {
                Payment = Prepare(cost);

                m_amount = amount;

                m_genericCost = cost.Generic;
                m_symbols = cost.SortedSymbols;
                m_symbolIndicesToPay = new List<int>(m_symbols.Count);

                for (int i = 0; i < m_symbols.Count; i++)
                {
                    switch (m_symbols[i])
                    {
                        case ManaSymbol.W:
                        case ManaSymbol.U:
                        case ManaSymbol.B:
                        case ManaSymbol.R:
                        case ManaSymbol.G:
                        case ManaSymbol.C:

                        case ManaSymbol.W2:
                        case ManaSymbol.U2:
                        case ManaSymbol.B2:
                        case ManaSymbol.R2:
                        case ManaSymbol.G2: 

                        case ManaSymbol.WP:
                        case ManaSymbol.UP:
                        case ManaSymbol.BP:
                        case ManaSymbol.RP:
                        case ManaSymbol.GP:

                        case ManaSymbol.WU:
                        case ManaSymbol.WB:
                        case ManaSymbol.UB:
                        case ManaSymbol.UR:
                        case ManaSymbol.BR:
                        case ManaSymbol.BG:
                        case ManaSymbol.RG:
                        case ManaSymbol.RW:
                        case ManaSymbol.GW:
                        case ManaSymbol.GU:
                            m_symbolIndicesToPay.Add(i);
                            break;

                        case ManaSymbol.X:
                        case ManaSymbol.Y:
                        case ManaSymbol.Z:
                        case ManaSymbol.S:
                            throw new NotSupportedException();

                        default:
                            throw new NotSupportedException();
                    }
                }
            }

            public void Solve()
            {
                TrivialPotential potential = new TrivialPotential();

                while (m_symbolIndicesToPay.Count > 0)
                {
                    if (!FindTrivialPayment(ref potential))
                        break;
                }

                if (m_symbolIndicesToPay.Count == 0)
                    potential.Reset();

                PayGenericMana(ref potential);
            }

            private bool FindTrivialPayment(ref TrivialPotential potential)
            {
                potential.Reset();

                for(int i = 0; i < m_symbolIndicesToPay.Count; i++)
                {
                    var symbolIndex = m_symbolIndicesToPay[i];

                    switch (m_symbols[symbolIndex])
                    {
                        case ManaSymbol.W: if (m_amount.White > 0) return AddPayment(i, new ManaPaymentAmount { White = 1 }, ref m_amount.White); break;
                        case ManaSymbol.U: if (m_amount.Blue > 0) return AddPayment(i, new ManaPaymentAmount { Blue = 1 }, ref m_amount.Blue); break;
                        case ManaSymbol.B: if (m_amount.Black > 0) return AddPayment(i, new ManaPaymentAmount { Black = 1 }, ref m_amount.Black); break;
                        case ManaSymbol.R: if (m_amount.Red > 0) return AddPayment(i, new ManaPaymentAmount { Red = 1 }, ref m_amount.Red); break;
                        case ManaSymbol.G: if (m_amount.Green > 0) return AddPayment(i, new ManaPaymentAmount { Green = 1 }, ref m_amount.Green); break;
                        case ManaSymbol.C: if (m_amount.Colorless > 0) return AddPayment(i, new ManaPaymentAmount { Colorless = 1 }, ref m_amount.Colorless); break;

                        case ManaSymbol.W2: potential.ConsiderWhite(i, ref m_amount); break;
                        case ManaSymbol.U2: potential.ConsiderBlue(i, ref m_amount); break;
                        case ManaSymbol.B2: potential.ConsiderBlack(i, ref m_amount); break;
                        case ManaSymbol.R2: potential.ConsiderRed(i, ref m_amount); break;
                        case ManaSymbol.G2: potential.ConsiderGreen(i, ref m_amount); break;

                        case ManaSymbol.WP: potential.ConsiderWhite(i, ref m_amount); break;
                        case ManaSymbol.UP: potential.ConsiderBlue(i, ref m_amount); break;
                        case ManaSymbol.BP: potential.ConsiderBlack(i, ref m_amount); break;
                        case ManaSymbol.RP: potential.ConsiderRed(i, ref m_amount); break;
                        case ManaSymbol.GP: potential.ConsiderGreen(i, ref m_amount); break;

                        case ManaSymbol.WU: potential.ConsiderWhite(i, ref m_amount); potential.ConsiderBlue(i, ref m_amount); break;
                        case ManaSymbol.WB: potential.ConsiderWhite(i, ref m_amount); potential.ConsiderBlack(i, ref m_amount); break;
                        case ManaSymbol.UB: potential.ConsiderBlue(i, ref m_amount); potential.ConsiderBlack(i, ref m_amount); break;
                        case ManaSymbol.UR: potential.ConsiderBlue(i, ref m_amount); potential.ConsiderRed(i, ref m_amount); break;
                        case ManaSymbol.BR: potential.ConsiderBlack(i, ref m_amount); potential.ConsiderRed(i, ref m_amount); break;
                        case ManaSymbol.BG: potential.ConsiderBlack(i, ref m_amount); potential.ConsiderGreen(i, ref m_amount); break;
                        case ManaSymbol.RG: potential.ConsiderRed(i, ref m_amount); potential.ConsiderGreen(i, ref m_amount); break;
                        case ManaSymbol.RW: potential.ConsiderRed(i, ref m_amount); potential.ConsiderWhite(i, ref m_amount); break;
                        case ManaSymbol.GW: potential.ConsiderGreen(i, ref m_amount); potential.ConsiderWhite(i, ref m_amount); break;
                        case ManaSymbol.GU: potential.ConsiderGreen(i, ref m_amount); potential.ConsiderBlue(i, ref m_amount); break;

                        case ManaSymbol.X:
                        case ManaSymbol.Y:
                        case ManaSymbol.Z:
                        case ManaSymbol.S:
                            throw new NotSupportedException();

                        default:
                            throw new NotSupportedException();
                    }
                }

                return ResolveTrivialPayment(ref potential);
            }

            private bool ResolveTrivialPayment(ref TrivialPotential potential)
            {
                if (potential.PotentialWhite >= 0)
                {
                    return AddPayment(potential.PotentialWhite, new ManaPaymentAmount { White = 1 }, ref m_amount.White);
                }

                if (potential.PotentialBlue >= 0)
                {
                    return AddPayment(potential.PotentialBlue, new ManaPaymentAmount { Blue = 1 }, ref m_amount.Blue);
                }

                if (potential.PotentialBlack >= 0)
                {
                    return AddPayment(potential.PotentialBlack, new ManaPaymentAmount { Black = 1 }, ref m_amount.Black);
                }

                if (potential.PotentialRed >= 0)
                {
                    return AddPayment(potential.PotentialRed, new ManaPaymentAmount { Red = 1 }, ref m_amount.Red);
                }

                if (potential.PotentialGreen >= 0)
                {
                    return AddPayment(potential.PotentialGreen, new ManaPaymentAmount { Green = 1 }, ref m_amount.Green);
                }

                return false;
            }

            private void PayGenericMana(ref TrivialPotential potential)
            {
                PayGenericMana(potential.PotentialWhite, m_amount.White, ref Payment.Generic.White);
                PayGenericMana(potential.PotentialBlue, m_amount.Blue, ref Payment.Generic.Blue);
                PayGenericMana(potential.PotentialBlack, m_amount.Black, ref Payment.Generic.Black);
                PayGenericMana(potential.PotentialRed, m_amount.Red, ref Payment.Generic.Red);
                PayGenericMana(potential.PotentialGreen, m_amount.Green, ref Payment.Generic.Green);
                PayGenericMana(-1, m_amount.Colorless, ref Payment.Generic.Colorless);
            }

            private void PayGenericMana(int potentialIndex, byte available, ref byte payment)
            {
                if (potentialIndex >= 0)
                    return; // Could use mana for remaining ambiguous symbols - not trivial

                available = Math.Min(available, m_genericCost);
                if (available > 0)
                {
                    HasTrivialPayment = true;
                    payment += available;
                }
            }

            private bool AddPayment(int i, ManaPaymentAmount atom, ref byte amount)
            {
                HasTrivialPayment = true;

                Payment.Atoms[m_symbolIndicesToPay[i]] = atom;
                m_symbolIndicesToPay.RemoveAtFast(i);

                Debug.Assert(amount > 0);
                amount--;

                return true;
            }
        }

        public static bool TryGetTrivialPayment(ManaCost cost, ManaAmount manaAmount, out ManaPaymentNew payment)
        {
            TrivialPaymentSolver solver = new TrivialPaymentSolver(cost, manaAmount);
            solver.Solve();
            payment = solver.Payment;
            return solver.HasTrivialPayment;
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
            byte numColorlessPay = Math.Min(workingPool.Colorless, cost.Generic);
            if (numColorlessPay > 0)
            {
                workingPool.Colorless -= numColorlessPay;
                cost = cost.RemoveGeneric(numColorlessPay);

                for (int i = 0; i < numColorlessPay; i++)
                {
                    basePayment.Add(Color.None);
                }
            }

            // Pay generic with remaining mana
            foreach (var colorlessPayment in EnumerateColorlessPayments(cost.Generic, workingPool))
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
            int numColorlessPay = Math.Max(0, cost.Generic - workingPool.TotalManaAmount);
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
            if (paidAllColored && cost.Generic > 0)
            {
                PayColorlessTrivial(cost.Generic, workingPool, payment);
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

        private static void PayColorlessTrivial(byte colorless, ManaPool pool, List<Color> payment)
        {
            Color color;
            if (!pool.TryGetSingleColor(out color))
                return;

            byte count = pool[color];
            byte payable = Math.Min(colorless, count);

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
