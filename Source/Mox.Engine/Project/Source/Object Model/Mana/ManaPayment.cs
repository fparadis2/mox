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

    [Serializable]
    public struct ManaPayment
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

        public ManaPayment Clone()
        {
            return new ManaPayment
            {
                Atoms = (Atoms != null ? (ManaPaymentAmount[])Atoms.Clone() : null),
                Generic = Generic
            };
        }

        public static ManaPayment Prepare(ManaCost cost)
        {
            var payment = new ManaPayment();

            if (cost.Symbols.Count > 0)
                payment.Atoms = new ManaPaymentAmount[cost.Symbols.Count];

            return payment;
        }

        public static ManaPayment CreateAnyFromCost(ManaCost cost)
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
            public ManaPayment Payment;

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

        public static bool TryGetTrivialPayment(ManaCost cost, ManaAmount manaAmount, out ManaPayment payment)
        {
            TrivialPaymentSolver solver = new TrivialPaymentSolver(cost, manaAmount);
            solver.Solve();
            payment = solver.Payment;
            return solver.HasTrivialPayment;
        }

        #endregion
    }
}
