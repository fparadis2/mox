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
using System.Diagnostics;
using Mox.Database;
using NUnit.Framework;

namespace Mox.AI.Arena
{
    [TestFixture, Ignore]
    public class ArenaTests
    {
        #region Decks

        private static Deck Create_ChoMannosResolve()
        {
            Deck deck = new Deck();

            deck.Cards.Add("Ghost Warden", 1);
            deck.Cards.Add("Youthful Knight", 2);
            deck.Cards.Add("Benalish Knight", 2);
            deck.Cards.Add("Venerable Monk", 1);
            deck.Cards.Add("Wild Griffin", 2);
            deck.Cards.Add("Cho-Manno, Revolutionary", 1);    // TODO
            deck.Cards.Add("Skyhunter Patrol", 2);
            deck.Cards.Add("Angel of Mercy", 2);
            deck.Cards.Add("Loxodon Mystic", 2);
            deck.Cards.Add("Ancestor's Chosen", 1);
            deck.Cards.Add("Condemn", 1);
            deck.Cards.Add("Pacifism", 2);                    // TODO
            deck.Cards.Add("Pariah", 1);                      // TODO
            deck.Cards.Add("Serra's Embrace", 1);             // TODO
            deck.Cards.Add("Angel's Feather", 1);
            deck.Cards.Add("Icy Manipulator", 1);

            deck.Cards.Add("Plains", 17);

            return deck;
        }

        private static ArenaResult Run()
        {
            int seed = new System.Random().Next();
            return Run(seed);
        }

        private static ArenaResult Run(int seed)
        {
            Arena test = new Arena(seed)
            {
                DeckA = Create_ChoMannosResolve(),
                DeckB = Create_ChoMannosResolve()
            };

            return test.Run();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_DryRun_1()
        {
            var result = Run(1);
            Console.Out.WriteLine(result);
        }

        [Test]
        public void Test_DryRun_2()
        {
            var result = Run(2);
            Console.Out.WriteLine(result);
        }

        [Test]
        public void Test_DryRun_3()
        {
            var result = Run(3);
            Console.Out.WriteLine(result);
        }

        [Test]
        public void Test_DryRun_9()
        {
            var result = Run(9);
            Console.Out.WriteLine(result);
        }

        [Test]
        public void Test_DryRun_12()
        {
            var result = Run(12);
            Console.Out.WriteLine(result);
        }

        [Test]
        public void Test_Consistency()
        {
            const int TestRuns = 50;

            int numAVictories = 0;

            Stopwatch stopwatch = Stopwatch.StartNew();

            for (int i = 1; i <= TestRuns; i++)
            {
                Console.Out.WriteLine("## Game {0} ##", i);
                var result = Run(i);
                if (result.Winner.Name.EndsWith("A"))
                {
                    numAVictories++;
                }
            }

            Console.Out.WriteLine("Test took {0} s.", stopwatch.Elapsed.TotalSeconds);
            Console.Out.WriteLine("Player A won {0} of the {1} games ({2}%).", numAVictories, TestRuns, (float)numAVictories / TestRuns * 100f);
        }

        #endregion
    }
}