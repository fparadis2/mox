using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    public class ManaPaymentEvaluator
    {
        [Flags]
        private enum EvaluationFlags
        {
            None = 0,
            EnumeratePayments = 1,
            EnumerateMissingMana = 2,
        }

        private readonly ManaCost m_cost;
        private readonly List<ManaSymbol> m_costSymbols = new List<ManaSymbol>();
        private byte m_genericCost;

        private readonly int m_minimumAmount;

        private readonly List<ManaPaymentNew> m_completePayments = new List<ManaPaymentNew>();
        private ManaColors m_missingMana;

        public ManaPaymentEvaluator(ManaCost cost)
        {
            Throw.IfNull(cost, "cost");
            Throw.InvalidArgumentIf(cost.IsEmpty, "Cost is empty", "cost");
            Throw.InvalidArgumentIf(!cost.IsConcrete, "Cost is not concrete", "cost");

            m_cost = cost;
            m_genericCost = cost.Generic;

            foreach (var symbol in cost.SortedSymbols)
            {
                m_minimumAmount += GetMinimumAmount(symbol);
                m_costSymbols.Add(symbol);
            }
        }

        public bool CanPay(ManaAmount amount)
        {
            // Early bail-out
            if (m_minimumAmount > amount.TotalAmount)
            {
                return false;
            }

            return Evaluate(EvaluationFlags.None, amount);
        }

        public bool EnumerateCompletePayments(ManaAmount amount)
        {
            m_completePayments.Clear();
            m_missingMana = ManaColors.None;

            return Evaluate(EvaluationFlags.EnumeratePayments | EvaluationFlags.EnumerateMissingMana, amount);
        }

        public IEnumerable<ManaPaymentNew> CompletePayments
        {
            get { return m_completePayments; }
        }

        public ManaColors MissingMana
        {
            get { return m_missingMana; }
        }

        private bool Evaluate(EvaluationFlags flags, ManaAmount remaining)
        {
            ManaPaymentNew payment = ManaPaymentNew.Prepare(m_cost);
            return EvaluateImpl(flags, 0, remaining, payment);
        }

        private bool EvaluateImpl(EvaluationFlags flags, int i, ManaAmount remaining, ManaPaymentNew payment)
        {
            if (i == m_costSymbols.Count)
            {
                if (m_genericCost > remaining.TotalAmount)
                {
                    m_missingMana |= ManaColors.All;
                    return false;
                }

                if (!flags.HasFlag(EvaluationFlags.EnumeratePayments))
                    return true;

                payment = payment.Clone(); // Clones the atoms
                return EnumerateGenericPaymentsImpl(m_genericCost, 0, remaining, payment, m_completePayments);
            }

            return EvaluateSymbol(flags, i, remaining, payment);
        }

        private bool EvaluateSymbol(EvaluationFlags flags, int i, ManaAmount remaining, ManaPaymentNew payment)
        {
            switch (m_costSymbols[i])
            {
                case ManaSymbol.C: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.Colorless, ref payment.Atoms[i].Colorless, ManaColors.Colorless);
                case ManaSymbol.W: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White);
                case ManaSymbol.U: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue);
                case ManaSymbol.B: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black);
                case ManaSymbol.R: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red);
                case ManaSymbol.G: return EvaluateSingle(flags, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green);

                case ManaSymbol.BG: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green);
                case ManaSymbol.BR: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red);
                case ManaSymbol.GU: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue);
                case ManaSymbol.GW: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White);
                case ManaSymbol.RG: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green);
                case ManaSymbol.RW: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White);
                case ManaSymbol.UB: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black);
                case ManaSymbol.UR: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red);
                case ManaSymbol.WB: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black);
                case ManaSymbol.WU: return EvaluateHybrid(flags, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue);

                case ManaSymbol.WP: return EvaluatePhyrexian(flags, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.White, ref payment.Atoms[i].White, ManaColors.White);
                case ManaSymbol.UP: return EvaluatePhyrexian(flags, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue);
                case ManaSymbol.BP: return EvaluatePhyrexian(flags, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black);
                case ManaSymbol.RP: return EvaluatePhyrexian(flags, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red);
                case ManaSymbol.GP: return EvaluatePhyrexian(flags, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green);

                case ManaSymbol.W2: return EvaluateHybrid2(flags, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ManaColors.White);
                case ManaSymbol.U2: return EvaluateHybrid2(flags, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ManaColors.Blue);
                case ManaSymbol.B2: return EvaluateHybrid2(flags, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ManaColors.Black);
                case ManaSymbol.R2: return EvaluateHybrid2(flags, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ManaColors.Red);
                case ManaSymbol.G2: return EvaluateHybrid2(flags, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ManaColors.Green);

                case ManaSymbol.S:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException("Symbol: " + m_costSymbols[i]);
            }
        }

        private bool EvaluateSingle(EvaluationFlags flags, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColor, ref byte paymentColor, ManaColors color)
        {
            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                return EvaluateImpl(flags, i + 1, remaining, payment);
            }
            else if (flags.HasFlag(EvaluationFlags.EnumerateMissingMana))
            {
                m_missingMana |= color;
                EvaluateImpl(EvaluationFlags.EnumerateMissingMana, i + 1, remaining, payment);
            }

            return false;
        }

        private bool EvaluateHybrid(EvaluationFlags flags, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColorA, ref byte paymentColorA, ManaColors colorA, ref byte remainingColorB, ref byte paymentColorB, ManaColors colorB)
        {
            bool canPay = false;

            if (remainingColorA > 0)
            {
                remainingColorA--;
                paymentColorA++;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);

                if (flags == EvaluationFlags.None && canPay)
                    return canPay;

                remainingColorA++;
                paymentColorA--;
            }
            else if (flags.HasFlag(EvaluationFlags.EnumerateMissingMana))
            {
                m_missingMana |= colorA;
                EvaluateImpl(EvaluationFlags.EnumerateMissingMana, i + 1, remaining, payment);
            }

            if (remainingColorB > 0)
            {
                remainingColorB--;
                paymentColorB++;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);
            }
            else if (flags.HasFlag(EvaluationFlags.EnumerateMissingMana))
            {
                m_missingMana |= colorB;

                if (remainingColorA > 0) // Only recurse once if both A and B are missing
                    EvaluateImpl(EvaluationFlags.EnumerateMissingMana, i + 1, remaining, payment);
            }

            return canPay;
        }

        private bool EvaluateHybrid2(EvaluationFlags flags, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColor, ref byte paymentColor, ManaColors color)
        {
            bool canPay = false;

            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);

                if (flags == EvaluationFlags.None && canPay)
                    return canPay;

                remainingColor++;
                paymentColor--;
            }
            else
            {
                // No need to recurse because it's always done with generic
                m_missingMana |= color;
            }

            {
                // Try with more generic at the end
                m_genericCost += 2;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);
                m_genericCost -= 2;
            }

            return canPay;
        }

        private bool EvaluatePhyrexian(EvaluationFlags flags, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref ManaPaymentAmount paymentAtom, ref byte remainingColor, ref byte paymentColor, ManaColors color)
        {
            bool canPay = false;

            {
                paymentAtom.Phyrexian++;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);

                if (flags == EvaluationFlags.None && canPay)
                    return canPay;

                paymentAtom.Phyrexian--;
            }

            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                canPay |= EvaluateImpl(flags, i + 1, remaining, payment);
            }
            else
            {
                // No need to recurse because it's always done with phyrexian
                m_missingMana |= color;
            }

            return canPay;
        }

        private static bool EnumerateGenericPaymentsImpl(byte numGeneric, int colorIndex, ManaPaymentAmount remaining, ManaPaymentNew payment, List<ManaPaymentNew> payments)
        {
            if (numGeneric == 0)
            {
                payments.Add(payment);
                return true;
            }

            byte remainingCount = remaining[colorIndex];

            if (colorIndex == ManaPaymentAmount.NumColors - 1)
            {
                // Slight optim: skip last round if it's not possible to pay
                if (numGeneric > remainingCount)
                    return false;

                payment.Generic[colorIndex] += numGeneric;
                payments.Add(payment);
                return true;
            }

            byte maxPayment = Math.Min(numGeneric, remainingCount);
            byte basePayment = payment.Generic[colorIndex];

            bool canPay = false;
            for (byte i = 0; i <= maxPayment; i++)
            {
                payment.Generic[colorIndex] = (byte)(basePayment + i);
                canPay |= EnumerateGenericPaymentsImpl((byte)(numGeneric - i), colorIndex + 1, remaining, payment, payments);
            }
            return canPay;
        }

        private static int GetMinimumAmount(ManaSymbol symbol)
        {
            switch (symbol)
            {
                case ManaSymbol.WP:
                case ManaSymbol.UP:
                case ManaSymbol.BP:
                case ManaSymbol.RP:
                case ManaSymbol.GP:
                    return 0; // Phyrexian can always be paid without mana

                case ManaSymbol.W:
                case ManaSymbol.U:
                case ManaSymbol.B:
                case ManaSymbol.R:
                case ManaSymbol.G:

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

                case ManaSymbol.S:
                case ManaSymbol.C:

                case ManaSymbol.W2:
                case ManaSymbol.U2:
                case ManaSymbol.B2:
                case ManaSymbol.R2:
                case ManaSymbol.G2:
                    return 1;

                case ManaSymbol.X:
                case ManaSymbol.Y:
                case ManaSymbol.Z:
                    return 0;

                default:
                    throw new ArgumentException("Invalid mana symbol");
            }
        }
    }
}
