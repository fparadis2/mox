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
using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class NegamaxTreeTests
    {
        #region Variables

        private NegamaxTree m_tree;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_tree = new NegamaxTree();
        }

        [TearDown]
        public void Teardown()
        {
            Debug.WriteLine(m_tree.DebugInfo);
        }

        #endregion

        #region Tests

        private IDisposable BeginNode(bool? cutoff = null)
        {
            return BeginNode(null, cutoff);
        }

        private IDisposable BeginNode(object result, bool? cutoff = null)
        {
            m_tree.BeginNode(result);

            return new DisposableHelper(delegate
            {
                bool actualCutoff = !m_tree.EndNode();

                if (cutoff.HasValue)
                {
                    Assert.That(cutoff == actualCutoff);
                }
            });
        }

        private void Do_LeafNode(float value, bool? cutoff = null)
        {
            using (BeginNode(cutoff))
            {
                m_tree.Evaluate(value);
            }
        }

        private void Assert_CurrentNode_Is(float alpha, float beta)
        {
            Assert.AreEqual(alpha, m_tree.CurrentNode.Alpha);
            Assert.AreEqual(beta, m_tree.CurrentNode.Beta);
        }

        [Test]
        public void Test_Wikipedia_Minimax()
        {
            #region Left

            using (BeginNode("Left")) // -10
            {
                m_tree.InitializeNode(false);

                using (BeginNode()) // 10
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode()) // 10
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(10);
                        Do_LeafNode(MinimaxTree.MaxValue);
                    }

                    using (BeginNode()) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(5);
                    }
                }

                using (BeginNode()) // -10
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode()) // -10
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(-10);
                    }
                }
            }

            #endregion

            #region Right

            using (BeginNode("Right")) // -7
            {
                m_tree.InitializeNode(false);

                using (BeginNode()) // 5
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode()) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(7);
                        Do_LeafNode(5);
                    }

                    using (BeginNode()) // -inf
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(MinimaxTree.MinValue);
                    }
                }

                using (BeginNode()) // -7
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode()) // -7
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(-7);
                        Do_LeafNode(-5);
                    }
                }
            }

            #endregion

            Assert.AreEqual(-7, m_tree.CurrentNode.BestValue);
            Assert.AreEqual("Right", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Wikipedia_Alpha_Beta()
        {
            #region Left

            using (BeginNode("Left", false)) // 3
            {
                m_tree.InitializeNode(false);

                using (BeginNode(false)) // 5
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(false)) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(5, false);
                        Assert_CurrentNode_Is(-5, NegamaxTree.MaxValue);

                        Do_LeafNode(6, false);
                        Assert_CurrentNode_Is(-5, NegamaxTree.MaxValue);
                    }

                    Assert_CurrentNode_Is(5, NegamaxTree.MaxValue);

                    using (BeginNode(false)) // 4
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(7, false);
                        Assert_CurrentNode_Is(-7, -5);

                        Do_LeafNode(4, true);
                        Assert_CurrentNode_Is(-4, -5);

                        // Skip 5
                    }

                    Assert_CurrentNode_Is(5, NegamaxTree.MaxValue);
                }

                Assert_CurrentNode_Is(-5, NegamaxTree.MaxValue);

                using (BeginNode(false)) // 3
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(false)) // 3
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(3, false);
                    }
                }
            }

            Assert.AreEqual(3, m_tree.CurrentNode.BestValue);

            #endregion

            #region Middle

            using (BeginNode("Middle", false)) // 6
            {
                m_tree.InitializeNode(false);

                using (BeginNode(false)) // 6
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(false)) // 6
                    {
                        m_tree.InitializeNode(false);
                        Do_LeafNode(6, false);
                    }

                    using (BeginNode(false)) // 6
                    {
                        m_tree.InitializeNode(false);
                        Do_LeafNode(6, true);
                        // Skip 9
                    }
                }

                using (BeginNode(false)) // 7
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(true)) // 7
                    {
                        m_tree.InitializeNode(false);
                        Do_LeafNode(7, false);
                    }
                }
            }

            Assert.AreEqual(6, m_tree.CurrentNode.BestValue);

            #endregion

            #region Right

            using (BeginNode("Right", false)) // 5
            {
                m_tree.InitializeNode(false);

                using (BeginNode(true)) // 5
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(false)) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(5, true);
                    }
                }

                // Skip the other subtree
            }

            #endregion

            Assert.AreEqual(6, m_tree.CurrentNode.BestValue);
            Assert.AreEqual("Middle", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Even_when_there_is_no_clear_winning_move_there_is_still_a_result()
        {
            using (BeginNode("ExpectedResult"))
            {
                m_tree.InitializeNode(false);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);
                    Do_LeafNode(0);
                }
            }

            Assert.AreEqual("ExpectedResult", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_a_discarded_choice_cannot_win_1()
        {
            using (BeginNode("DiscardedResult"))
            {
                m_tree.Discard();
            }

            using (BeginNode("ExpectedResult"))
            {
                m_tree.Evaluate(0f);
            }

            Assert.AreEqual("ExpectedResult", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_a_discarded_choice_cannot_win_2()
        {
            using (BeginNode("DiscardedResult"))
            {
                m_tree.Discard();
            }

            using (BeginNode("ExpectedResult"))
            {
                m_tree.Discard();
            }

            Assert.IsFalse(m_tree.CurrentNode.HasResult);
        }

        [Test]
        public void Test_When_only_maximizing_it_will_return_the_largest_result()
        {
            using (BeginNode("0"))
            {
                m_tree.InitializeNode(true);
                Do_LeafNode(0);
            }

            using (BeginNode("+1000"))
            {
                m_tree.InitializeNode(true);
                Do_LeafNode(+1000);
            }

            using (BeginNode("-1000"))
            {
                m_tree.InitializeNode(true);
                Do_LeafNode(-1000);
            }

            using (BeginNode("-10000"))
            {
                m_tree.InitializeNode(true);
                using (BeginNode())
                {
                    m_tree.InitializeNode(true);
                    Do_LeafNode(-10000);
                }
            }

            Assert.AreEqual("+1000", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Can_have_two_maximizing_or_minimizing_nodes_in_a_row()
        {
            //                M8             
            //        A   /       \    B
            //           M8        M1
            //          / \       / \
            //         M5   8    3  m1
            //        / \           / \
            //       m5  9         1  m4
            //      / \               / \
            //     5   7             4  15

            // Two maximizing levels in a row
            using (BeginNode("A"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    using (BeginNode())
                    {
                        m_tree.InitializeNode(false);
                        Do_LeafNode(5);
                        Do_LeafNode(7);
                    }

                    Do_LeafNode(9);
                }

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);
                    Do_LeafNode(8);
                }
            }

            // Two minimizing levels in a row
            using (BeginNode("B"))
            {
                m_tree.InitializeNode(true);

                Do_LeafNode(3);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    Do_LeafNode(1);

                    using (BeginNode())
                    {
                        m_tree.InitializeNode(false);
                        Do_LeafNode(4);
                        Do_LeafNode(15);
                    }
                }
            }

            Assert.AreEqual("A", m_tree.CurrentNode.Result);
            Assert.AreEqual(8, m_tree.CurrentNode.BestValue);
        }

        // Currently not supported (throws)
        /*[Test]
        public void Test_TranspositionTable_Seeing_the_same_node_at_the_same_level_will_use_the_table_on_different_color_nodes()
        {
            //               M10             
            //        A   /       \    B
            //           m8       M10
            //          / \       / \
            //        10   8    10X  1

            using (BeginNode("A"))
            {
                m_tree.InitializeNode(false);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);
                    Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                    m_tree.Evaluate(10);
                }

                Do_LeafNode(8);
            }

            using (BeginNode("B"))
            {
                m_tree.InitializeNode(true);

                Assert_CurrentNode_Is(8, NegamaxTree.MaxValue);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);
                    Assert.IsFalse(m_tree.ConsiderTranspositionTable(0xabcd));
                }

                Assert_CurrentNode_Is(10, NegamaxTree.MaxValue);

                Do_LeafNode(1);
            }

            Assert.AreEqual(10, m_tree.CurrentNode.BestValue);
        }*/

        [Test]
        public void Test_TranspositionTable_Exact()
        {
            //               M5             
            //          /    |    \
            //         m4    5     m4
            //        /           /|<- cut off
            //       4           7 4

            using (BeginNode("Setup"))
            {
                m_tree.InitializeNode(false);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                    m_tree.Evaluate(4);
                }
            }

            Do_LeafNode(5);

            using (BeginNode("Test"))
            {
                m_tree.InitializeNode(false);

                Do_LeafNode(7);

                Assert_CurrentNode_Is(-7, -5);

                using (BeginNode(true))
                {
                    m_tree.InitializeNode(false);
                    Assert.IsFalse(m_tree.ConsiderTranspositionTable(0xabcd));
                }

                Assert_CurrentNode_Is(-4, -5);
            }

            Assert.AreEqual(5, m_tree.CurrentNode.BestValue);
        }

        [Test]
        public void Test_TranspositionTable_Exact_all_maximizing()
        {
            //               M7
            //          /    |    \
            //         M4    5     M7
            //        /           /|
            //       4           7 4

            using (BeginNode("Setup"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);

                    Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                    m_tree.Evaluate(4);
                }
            }

            Do_LeafNode(5);

            using (BeginNode("Test"))
            {
                m_tree.InitializeNode(true);

                Do_LeafNode(7);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);
                    Assert.IsFalse(m_tree.ConsiderTranspositionTable(0xabcd));
                }
            }

            Assert.AreEqual(7, m_tree.CurrentNode.BestValue);
        }

        [Test]
        public void Test_TranspositionTable_LowerBound_cutoff()
        {
            //               M5             
            //          /    |    \
            //         m3    5     m3
            //        /  \        /|<- cut off
            //       3    4      7 3x

            using (BeginNode("Setup"))
            {
                m_tree.InitializeNode(false);

                Do_LeafNode(3);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                    m_tree.Evaluate(4);
                }
            }

            Do_LeafNode(5);

            using (BeginNode("Test"))
            {
                m_tree.InitializeNode(false);

                Do_LeafNode(7);

                Assert_CurrentNode_Is(-7, -5);

                using (BeginNode(true))
                {
                    m_tree.InitializeNode(false);
                    Assert.IsFalse(m_tree.ConsiderTranspositionTable(0xabcd));
                }

                Assert_CurrentNode_Is(-4, -5);
            }

            Assert.AreEqual(5, m_tree.CurrentNode.BestValue);
        }

        [Test]
        public void Test_TranspositionTable_LowerBound_no_cutoff()
        {
            //          M-inf     M5             
            //          /       /   \
            //         m-inf   5     m3
            //        /  \          /|
            //       3  -inf       7 3

            using (BeginNode("Setup"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    Do_LeafNode(3);

                    using (BeginNode())
                    {
                        m_tree.InitializeNode(false);

                        Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                        m_tree.Evaluate(4);
                    }

                    Do_LeafNode(-10000);
                }
            }

            using (BeginNode("Test"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(false);

                    Do_LeafNode(7);

                    Assert_CurrentNode_Is(-7, +10000);

                    using (BeginNode())
                    {
                        m_tree.InitializeNode(false);
                        Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));

                        Assert_CurrentNode_Is(-4, +10000);

                        m_tree.Evaluate(6);
                    }
                }
            }
        }

        [Test]
        public void Test_TranspositionTable_will_not_be_used_if_seen_in_earlier_depth()
        {
            //               M10             
            //        A   /       \    B
            //          M10       NOPE
            //          / \       / \
            //        10   8     12  1

            using (BeginNode("A"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);
                    Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));
                    m_tree.Evaluate(10);
                }

                Do_LeafNode(8);
            }

            using (BeginNode("B"))
            {
                m_tree.InitializeNode(true);

                Assert.IsTrue(m_tree.ConsiderTranspositionTable(0xabcd));

                using (BeginNode())
                {
                    m_tree.Evaluate(12);
                }

                Do_LeafNode(1);
            }

            Assert.AreEqual(12, m_tree.CurrentNode.BestValue);
        }

        #endregion
    }
}
