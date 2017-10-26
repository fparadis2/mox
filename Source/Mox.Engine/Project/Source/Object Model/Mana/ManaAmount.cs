using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mox
{
    /// <summary>
    /// A concrete amount of mana (in a mana pool for instance)
    /// </summary>
    [Serializable]
    public struct ManaAmount
    {
        #region Variables

        public byte Colorless;
        public byte White;
        public byte Blue;
        public byte Black;
        public byte Red;
        public byte Green;

        public int TotalAmount
        {
            get
            {
                return Colorless + White + Blue + Black + Red + Green;
            }
        }

        #endregion

        #region Methods

        public void Add(Color color, byte amount)
        {
            Debug.Assert(!color.HasMoreThanOneColor());
            
            switch (color)
            {
                case Color.None:
                    Debug.Assert(Colorless <= byte.MaxValue - amount);
                    Colorless += amount;
                    break;

                case Color.White:
                    Debug.Assert(White <= byte.MaxValue - amount);
                    White += amount;
                    break;

                case Color.Blue:
                    Debug.Assert(Blue <= byte.MaxValue - amount);
                    Blue += amount;
                    break;

                case Color.Black:
                    Debug.Assert(Black <= byte.MaxValue - amount);
                    Black += amount;
                    break;

                case Color.Red:
                    Debug.Assert(Red <= byte.MaxValue - amount);
                    Red += amount;
                    break;

                case Color.Green:
                    Debug.Assert(Green <= byte.MaxValue - amount);
                    Green += amount;
                    break;
            }
        }

        public static ManaAmount operator+(ManaAmount a, ManaAmount b)
        {
            return new ManaAmount
            {
                Colorless = (byte)(a.Colorless + b.Colorless),
                White = (byte)(a.White + b.White),
                Blue = (byte)(a.Blue + b.Blue),
                Black = (byte)(a.Black + b.Black),
                Red = (byte)(a.Red + b.Red),
                Green = (byte)(a.Green + b.Green)
            };
        }

        #endregion
    }
}
