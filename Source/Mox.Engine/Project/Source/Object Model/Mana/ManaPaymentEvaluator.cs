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
        private readonly ManaCost m_cost;
        private readonly List<ManaSymbol> m_costSymbols = new List<ManaSymbol>();
        private int m_genericCost;

        private readonly int m_minimumAmount;

        private readonly List<ManaPaymentNew> m_completePayments = new List<ManaPaymentNew>();

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
            return Evaluate(true, amount);
        }

        public bool EnumerateCompletePayments(ManaAmount amount)
        {
            m_completePayments.Clear();
            return Evaluate(false, amount);
        }

        public IEnumerable<ManaPaymentNew> CompletePayments
        {
            get { return m_completePayments; }
        }

        private bool Evaluate(bool single, ManaAmount remaining)
        {
            // Early bail-out
            if (m_minimumAmount > remaining.TotalAmount)
            {
                return false;
            }

            ManaPaymentNew payment = ManaPaymentNew.Prepare(m_cost);
            return EvaluateImpl(single, 0, remaining, payment);
        }

        private bool EvaluateImpl(bool single, int i, ManaAmount remaining, ManaPaymentNew payment)
        {
            if (i == m_costSymbols.Count)
            {
                if (single)
                {
                    // Shortcut: if we only want to know if it's payable, we can compare total amount to remaining generic cost
                    return (m_genericCost <= remaining.TotalAmount);
                }

                payment = payment.Clone(); // Clones the atoms

                Debug.Assert(m_genericCost <= byte.MaxValue);
                return EnumerateGenericPaymentsImpl((byte)m_genericCost, 0, remaining, payment, m_completePayments);
            }

            return EvaluateSymbol(single, i, remaining, payment);
        }

        private bool EvaluateSymbol(bool single, int i, ManaAmount remaining, ManaPaymentNew payment)
        {
            switch (m_costSymbols[i])
            {
                case ManaSymbol.C: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.Colorless, ref payment.Atoms[i].Colorless);
                case ManaSymbol.W: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White);
                case ManaSymbol.U: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue);
                case ManaSymbol.B: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black);
                case ManaSymbol.R: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red);
                case ManaSymbol.G: return EvaluateSingle(single, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green);

                case ManaSymbol.BG: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ref remaining.Green, ref payment.Atoms[i].Green);
                case ManaSymbol.BR: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black, ref remaining.Red, ref payment.Atoms[i].Red);
                case ManaSymbol.GU: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ref remaining.Blue, ref payment.Atoms[i].Blue);
                case ManaSymbol.GW: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green, ref remaining.White, ref payment.Atoms[i].White);
                case ManaSymbol.RG: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ref remaining.Green, ref payment.Atoms[i].Green);
                case ManaSymbol.RW: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red, ref remaining.White, ref payment.Atoms[i].White);
                case ManaSymbol.UB: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ref remaining.Black, ref payment.Atoms[i].Black);
                case ManaSymbol.UR: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue, ref remaining.Red, ref payment.Atoms[i].Red);
                case ManaSymbol.WB: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ref remaining.Black, ref payment.Atoms[i].Black);
                case ManaSymbol.WU: return EvaluateHybrid(single, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White, ref remaining.Blue, ref payment.Atoms[i].Blue);

                case ManaSymbol.WP: return EvaluatePhyrexian(single, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.White, ref payment.Atoms[i].White);
                case ManaSymbol.UP: return EvaluatePhyrexian(single, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Blue, ref payment.Atoms[i].Blue);
                case ManaSymbol.BP: return EvaluatePhyrexian(single, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Black, ref payment.Atoms[i].Black);
                case ManaSymbol.RP: return EvaluatePhyrexian(single, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Red, ref payment.Atoms[i].Red);
                case ManaSymbol.GP: return EvaluatePhyrexian(single, i, ref remaining, ref payment, ref payment.Atoms[i], ref remaining.Green, ref payment.Atoms[i].Green);

                case ManaSymbol.W2: return EvaluateHybrid2(single, i, ref remaining, ref payment, ref remaining.White, ref payment.Atoms[i].White);
                case ManaSymbol.U2: return EvaluateHybrid2(single, i, ref remaining, ref payment, ref remaining.Blue, ref payment.Atoms[i].Blue);
                case ManaSymbol.B2: return EvaluateHybrid2(single, i, ref remaining, ref payment, ref remaining.Black, ref payment.Atoms[i].Black);
                case ManaSymbol.R2: return EvaluateHybrid2(single, i, ref remaining, ref payment, ref remaining.Red, ref payment.Atoms[i].Red);
                case ManaSymbol.G2: return EvaluateHybrid2(single, i, ref remaining, ref payment, ref remaining.Green, ref payment.Atoms[i].Green);

                case ManaSymbol.S:
                    throw new NotImplementedException();

                default:
                    throw new NotImplementedException("Symbol: " + m_costSymbols[i]);
            }
        }

        private bool EvaluateSingle(bool single, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColor, ref byte paymentColor)
        {
            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                return EvaluateImpl(single, i + 1, remaining, payment);
            }

            return false;
        }

        private bool EvaluateHybrid(bool single, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColorA, ref byte paymentColorA, ref byte remainingColorB, ref byte paymentColorB)
        {
            bool canPay = false;

            if (remainingColorA > 0)
            {
                remainingColorA--;
                paymentColorA++;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);

                if (single && canPay)
                    return canPay;

                remainingColorA++;
                paymentColorA--;
            }

            if (remainingColorB > 0)
            {
                remainingColorB--;
                paymentColorB++;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);
            }

            return canPay;
        }

        private bool EvaluateHybrid2(bool single, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref byte remainingColor, ref byte paymentColor)
        {
            bool canPay = false;

            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);

                if (single && canPay)
                    return canPay;

                remainingColor++;
                paymentColor--;
            }

            {
                // Try with more generic at the end
                m_genericCost += 2;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);
                m_genericCost -= 2;
            }

            return canPay;
        }

        private bool EvaluatePhyrexian(bool single, int i, ref ManaAmount remaining, ref ManaPaymentNew payment, ref ManaPaymentAmount paymentAtom, ref byte remainingColor, ref byte paymentColor)
        {
            bool canPay = false;

            {
                paymentAtom.Phyrexian++;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);

                if (single && canPay)
                    return canPay;

                paymentAtom.Phyrexian--;
            }

            if (remainingColor > 0)
            {
                remainingColor--;
                paymentColor++;
                canPay |= EvaluateImpl(single, i + 1, remaining, payment);
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
