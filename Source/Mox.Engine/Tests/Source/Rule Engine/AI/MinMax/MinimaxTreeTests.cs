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
using System.Linq;
using System.Text;

using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class MinimaxTreeTests
    {
        #region Variables

        private MinimaxTree m_tree;

        #endregion

        #region Setup

        [SetUp]
        public void Setup()
        {
            m_tree = new MinimaxTree();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Constant_Consistency()
        {
            Assert.GreaterOrEqual(MinimaxTree.MinValue, MinimaxTree.StartingMinValue);
            Assert.LessOrEqual(MinimaxTree.MaxValue, MinimaxTree.StartingMaxValue);
        }

        [Test]
        public void Test_Construction_values()
        {
            Assert.IsNotNull(m_tree.CurrentNode);
            Assert.AreEqual(MinimaxTree.StartingMinValue, m_tree.CurrentNode.Alpha);
            Assert.AreEqual(MinimaxTree.StartingMaxValue, m_tree.CurrentNode.Beta);
            Assert.IsNull(m_tree.CurrentNode.Result);
            Assert.That(m_tree.CurrentNode.IsMaximizing);
        }

        [Test]
        public void Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor_if_both_maximizing_or_minimizing()
        {
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(false, true, true);
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(false, false, false);
        }

        [Test]
        public void Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor_swapped()
        {
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(true, true, false);
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(true, false, true);
        }

        private void Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(bool expectSwapped, bool firstNodeMax, bool secondNodeMax)
        {
            m_tree = new MinimaxTree();
            MinimaxTree.Node rootNode = m_tree.CurrentNode;
            rootNode.Alpha = 10;
            rootNode.Beta = 20;

            m_tree.BeginNode(firstNodeMax, "Something");
            MinimaxTree.Node node = m_tree.CurrentNode;
            m_tree.BeginNode(secondNodeMax, "Something Else");

            Assert.AreNotEqual(rootNode, node);
            Assert.AreEqual("Something", node.Result);

            if (expectSwapped)
            {
                Assert.AreEqual(20, node.Alpha);
                Assert.AreEqual(10, node.Beta);
            }
            else
            {
                Assert.AreEqual(10, node.Alpha);
                Assert.AreEqual(20, node.Beta);
            }
        }

        [Test]
        public void Test_Evaluate_assigns_to_Alpha()
        {
            m_tree.BeginNode(false, null);
            m_tree.Evaluate(10);

            Assert.AreEqual(10, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Discard_assigns_very_small_alpha_if_maximizing()
        {
            m_tree.BeginNode(true, null);
            m_tree.BeginNode(true, null);
            m_tree.Discard();
            Assert.AreEqual(MinimaxTree.MinValue, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Discard_assigns_very_large_alpha_if_minimizing()
        {
            m_tree.BeginNode(false, null);
            m_tree.BeginNode(true, null);
            m_tree.Discard();
            Assert.AreEqual(MinimaxTree.MaxValue, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Depth_returns_the_current_depth_of_the_tree()
        {
            Assert.AreEqual(1, m_tree.Depth);
            m_tree.BeginNode(true, null);
            {
                Assert.AreEqual(2, m_tree.Depth);
                m_tree.BeginNode(false, null);
                {
                    Assert.AreEqual(3, m_tree.Depth);
                    m_tree.BeginNode(true, null);
                    {
                        Assert.AreEqual(4, m_tree.Depth);
                        m_tree.Evaluate(10);
                    }
                    m_tree.EndNode();
                    Assert.AreEqual(3, m_tree.Depth);
                }
                m_tree.EndNode();
                Assert.AreEqual(2, m_tree.Depth);
            }
            m_tree.EndNode();
            Assert.AreEqual(1, m_tree.Depth);
        }

        #region Functional Tests

        private IDisposable BeginNode(bool maximizing, bool? cutoff)
        {
            return BeginNode(maximizing, cutoff, null);
        }

        private IDisposable BeginNode(bool maximizing, bool? cutoff, object result)
        {
            m_tree.BeginNode(maximizing, result);

            return new DisposableHelper(delegate
            {
                bool actualCutoff = !m_tree.EndNode();

                if (cutoff.HasValue)
                {
                    Assert.That(cutoff == actualCutoff);
                }
            });
        }

        private void Do_LeafNode(bool maximizing, bool? cutoff, float value)
        {
            using (BeginNode(maximizing, cutoff))
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

            using (BeginNode(true, null, "Left")) // -10
            {
                using (BeginNode(false, null)) // 10
                {
                    using (BeginNode(true, null)) // 10
                    {
                        Do_LeafNode(false, null, 10);
                        Do_LeafNode(false, null, MinimaxTree.MaxValue);
                    }

                    using (BeginNode(true, null)) // 5
                    {
                        Do_LeafNode(false, null, 5);
                    }
                }

                using (BeginNode(false, null)) // -10
                using (BeginNode(true, null)) // -10
                {
                    Do_LeafNode(false, null, -10);
                }
            }

            #endregion

            #region Right

            using (BeginNode(true, null, "Right")) // -7
            {
                using (BeginNode(false, null)) // 5
                {
                    using (BeginNode(true, null)) // 5
                    {
                        Do_LeafNode(false, null, 7);
                        Do_LeafNode(false, null, 5);
                    }

                    using (BeginNode(true, null)) // -inf
                    {
                        Do_LeafNode(false, null, MinimaxTree.MinValue);
                    }
                }

                using (BeginNode(false, null)) // -7
                using (BeginNode(true, null)) // -7
                {
                    Do_LeafNode(false, null, -7);
                    Do_LeafNode(false, null, -5);
                }
            }

            #endregion

            Assert.AreEqual(-7, m_tree.CurrentNode.Alpha);
            Assert.AreEqual("Right", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Wikipedia_Alpha_Beta()
        {
            #region Left

            using (BeginNode(true, false, "Left")) // 3
            {
                using (BeginNode(false, false)) // 5
                {
                    using (BeginNode(true, false)) // 5
                    {
                        Do_LeafNode(false, false, 5);
                        Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);

                        Do_LeafNode(false, false, 6);
                        Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);
                    }

                    Assert_CurrentNode_Is(5, MinimaxTree.StartingMaxValue);

                    using (BeginNode(true, false)) // 4
                    {
                        Do_LeafNode(false, false, 7);
                        Assert_CurrentNode_Is(7, 5);

                        Do_LeafNode(false, true, 4);
                        Assert_CurrentNode_Is(4, 5);

                        // Skip 5
                    }

                    Assert_CurrentNode_Is(5, MinimaxTree.StartingMaxValue);
                }

                Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);

                using (BeginNode(false, false)) // 3
                using (BeginNode(true, false)) // 3
                {
                    Do_LeafNode(false, false, 3);
                }
            }

            Assert.AreEqual(3, m_tree.CurrentNode.Alpha);

            #endregion

            #region Middle

            using (BeginNode(true, false, "Middle")) // 6
            {
                using (BeginNode(false, false)) // 6
                {
                    using (BeginNode(true, false)) // 6
                    {
                        Do_LeafNode(false, false, 6);
                    }

                    using (BeginNode(true, false)) // 6
                    {
                        Do_LeafNode(false, true, 6);
                        // Skip 9
                    }
                }

                using (BeginNode(false, false)) // 7
                using (BeginNode(true, true)) // 7
                {
                    Do_LeafNode(false, false, 7);
                }
            }

            Assert.AreEqual(6, m_tree.CurrentNode.Alpha);

            #endregion

            #region Right

            using (BeginNode(true, false, "Right")) // 5
            {
                using (BeginNode(false, true)) // 5
                using (BeginNode(true, false)) // 5
                {
                    Do_LeafNode(false, true, 5);
                }

                // Skip the other subtree
            }

            #endregion

            Assert.AreEqual(6, m_tree.CurrentNode.Alpha);
            Assert.AreEqual("Middle", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Even_when_there_is_no_clear_winning_move_there_is_still_a_result()
        {
            using (BeginNode(true, null, "ExpectedResult"))
            {
                using (BeginNode(false, null))
                {
                    Do_LeafNode(false, null, 0);
                }
            }

            Assert.AreEqual("ExpectedResult", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_a_discarded_choice_cannot_win_1()
        {
            using (BeginNode(true, null, "DiscardedResult"))
            {
                m_tree.Discard();
            }

            using (BeginNode(true, null, "ExpectedResult"))
            {
                m_tree.Evaluate(0f);
            }

            Assert.AreEqual("ExpectedResult", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_a_discarded_choice_cannot_win_2()
        {
            using (BeginNode(true, null, "DiscardedResult"))
            {
                m_tree.Discard();
            }

            using (BeginNode(true, null, "ExpectedResult"))
            {
                m_tree.Discard();
            }

            Assert.IsFalse(m_tree.CurrentNode.HasResult);
        }

        [Test]
        public void Test_When_only_maximizing_it_will_return_the_largest_result()
        {
            using (BeginNode(true, null, "0"))
            {
                //using (BeginNode(true, null))
                {
                    Do_LeafNode(true, null, 0);
                }
            }

            using (BeginNode(true, null, "+1000"))
            {
                //using (BeginNode(true, null))
                {
                    Do_LeafNode(true, null, +1000);
                }
            }

            using (BeginNode(true, null, "-1000"))
            {
                //using (BeginNode(true, null))
                {
                    Do_LeafNode(true, null, -1000);
                }
            }

            Assert.AreEqual("+1000", m_tree.CurrentNode.Result);
        }

        #endregion

        #endregion
    }
}
