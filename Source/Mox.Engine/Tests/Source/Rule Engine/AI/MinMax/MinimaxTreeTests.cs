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

        [TearDown]
        public void TearDown()
        {
            Debug.WriteLine(m_tree.DebugInfo);
            Debug.Flush();
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
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(true, false, false);
        }

        [Test]
        public void Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor_swapped()
        {
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(true, true, false);
            Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(false, false, true);
        }

        private void Test_BeginNode_pushes_a_new_node_on_the_stack_with_the_alpha_and_beta_of_its_ancestor(bool expectSwapped, bool firstNodeMax, bool secondNodeMax)
        {
            m_tree = new MinimaxTree();
            MinimaxTree.Node rootNode = m_tree.CurrentNode;
            rootNode.Alpha = 10;
            rootNode.Beta = 20;

            m_tree.BeginNode("Something");
            m_tree.InitializeNode(firstNodeMax);

            MinimaxTree.Node node1 = m_tree.CurrentNode;
            Assert.AreNotEqual(rootNode, node1);
            Assert.AreEqual("Something", node1.Result);

            m_tree.BeginNode("Something Else");
            m_tree.InitializeNode(secondNodeMax);

            MinimaxTree.Node node2 = m_tree.CurrentNode;
            Assert.AreNotEqual(node1, node2);
            Assert.AreEqual("Something Else", node2.Result);

            if (expectSwapped)
            {
                Assert.AreEqual(20, node2.Alpha);
                Assert.AreEqual(10, node2.Beta);
            }
            else
            {
                Assert.AreEqual(10, node2.Alpha);
                Assert.AreEqual(20, node2.Beta);
            }
        }

        [Test]
        public void Test_Evaluate_assigns_to_Alpha()
        {
            m_tree.BeginNode(null);
            m_tree.Evaluate(10);

            Assert.AreEqual(10, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Discard_assigns_very_small_alpha_if_maximizing()
        {
            m_tree.BeginNode(null);
            m_tree.InitializeNode(true);

            m_tree.BeginNode(null);
            m_tree.Discard();
            Assert.AreEqual(MinimaxTree.MinValue, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Discard_assigns_very_large_alpha_if_minimizing()
        {
            m_tree.BeginNode(null);
            m_tree.InitializeNode(false);

            m_tree.BeginNode(null);
            m_tree.Discard();
            Assert.AreEqual(MinimaxTree.MaxValue, m_tree.CurrentNode.Alpha);
        }

        [Test]
        public void Test_Depth_returns_the_current_depth_of_the_tree()
        {
            Assert.AreEqual(1, m_tree.Depth);
            m_tree.BeginNode(null);
            {
                m_tree.InitializeNode(true);

                Assert.AreEqual(2, m_tree.Depth);
                m_tree.BeginNode(null);
                {
                    m_tree.InitializeNode(false);

                    Assert.AreEqual(3, m_tree.Depth);
                    m_tree.BeginNode(null);
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

        private IDisposable BeginNode(object result = null, bool? cutoff = null)
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
            using (BeginNode(null, cutoff))
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

            Assert.AreEqual(-7, m_tree.CurrentNode.Alpha);
            Assert.AreEqual("Right", m_tree.CurrentNode.Result);
        }

        [Test]
        public void Test_Wikipedia_Alpha_Beta()
        {
            #region Left

            using (BeginNode("Left", false)) // 3
            {
                m_tree.InitializeNode(false);

                using (BeginNode(null, false)) // 5
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(null, false)) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(5, false);
                        Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);

                        Do_LeafNode(6, false);
                        Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);
                    }

                    Assert_CurrentNode_Is(5, MinimaxTree.StartingMaxValue);

                    using (BeginNode(null, false)) // 4
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(7, false);
                        Assert_CurrentNode_Is(7, 5);

                        Do_LeafNode(4, true);
                        Assert_CurrentNode_Is(4, 5);

                        // Skip 5
                    }

                    Assert_CurrentNode_Is(5, MinimaxTree.StartingMaxValue);
                }

                Assert_CurrentNode_Is(5, MinimaxTree.StartingMinValue);

                using (BeginNode(null, false)) // 3
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(null, false)) // 3
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(3, false);
                    }
                }
            }

            Assert.AreEqual(3, m_tree.CurrentNode.Alpha);

            #endregion

            #region Middle

            using (BeginNode("Middle", false)) // 6
            {
                m_tree.InitializeNode(false);

                using (BeginNode(null, false)) // 6
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(null, false)) // 6
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(6, false);
                    }

                    using (BeginNode(null, false)) // 6
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(6, true);
                        // Skip 9
                    }
                }

                using (BeginNode(null, false)) // 7
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(null, true)) // 7
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(7, false);
                    }
                }
            }

            Assert.AreEqual(6, m_tree.CurrentNode.Alpha);

            #endregion

            #region Right

            using (BeginNode("Right", false)) // 5
            {
                m_tree.InitializeNode(false);

                using (BeginNode(null, true)) // 5
                {
                    m_tree.InitializeNode(true);

                    using (BeginNode(null, false)) // 5
                    {
                        m_tree.InitializeNode(false);

                        Do_LeafNode(5, true);
                    }
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
            using (BeginNode("ExpectedResult"))
            {
                m_tree.InitializeNode(true);

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

                //using (BeginNode(true, null))
                {
                    Do_LeafNode(0);
                }
            }

            using (BeginNode("+1000"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);

                    Do_LeafNode(+1000);
                }
            }

            using (BeginNode("-1000"))
            {
                m_tree.InitializeNode(true);

                using (BeginNode())
                {
                    m_tree.InitializeNode(true);

                    Do_LeafNode(-1000);
                }
            }

            Assert.AreEqual("+1000", m_tree.CurrentNode.Result);
        }

        #endregion

        #endregion
    }
}
