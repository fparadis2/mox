using Mox.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox.Database
{
    public static class AmountParser
    {
        public static bool Parse(string text, out AmountResolver amount)
        {
            if (TryParseConstant(text, out int value))
            {
                amount = new ConstantAmountResolver(value);
                return true;
            }

            amount = null;
            return false;
        }

        private static bool TryParseConstant(string number, out int value)
        {
            value = 0;

            if (string.IsNullOrEmpty(number))
                return false;

            if (int.TryParse(number, out value))
                return true;

            switch (number)
            {
                case "no": value = 0; return true;
                case "a": value = 1; return true;
                case "an": value = 1; return true;
                case "one": value = 1; return true;
                case "two": value = 2; return true;
                case "three": value = 3; return true;
                case "four": value = 4; return true;
                case "five": value = 5; return true;
                case "six": value = 6; return true;
                case "seven": value = 7; return true;
                case "eight": value = 8; return true;
                case "nine": value = 9; return true;
                case "ten": value = 10; return true;
                case "eleven": value = 11; return true;
                case "twelve": value = 12; return true;
                case "thirteen": value = 13; return true;
                case "fourteen": value = 14; return true;
                case "fifteen": value = 15; return true;
                case "twenty": value = 20; return true;
                case "ninety-nine": value = 99; return true;
                default: return false;
            }
        }
    }
}
