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
using System.Reflection;
using System.Text;
using Mox.Flow;
using NUnit.Framework;

namespace Mox.AI
{
    [TestFixture]
    public class AttributedChoiceResolverProviderTests
    {
        #region Inner Types

        private class MyResolver : ChoiceResolver
        {
            #region Implementation of ChoiceResolver

            /// <summary>
            /// Returns the part context for the given choice context.
            /// </summary>
            /// <param name="method"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public override Part<TController>.Context GetContext<TController>(MethodBase method, object[] args)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Replaces the context argument with the given one.
            /// </summary>
            /// <param name="method"></param>
            /// <param name="args"></param>
            /// <param name="context"></param>
            public override void SetContext<TController>(MethodBase method, object[] args, Part<TController>.Context context)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Returns the player associated with the given method call.
            /// </summary>
            /// <param name="choiceMethod"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public override Player GetPlayer(MethodBase choiceMethod, object[] args)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Returns the possible choices for choice context.
            /// </summary>
            /// <param name="choiceMethod"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public override IEnumerable<object> ResolveChoices(MethodBase choiceMethod, object[] args)
            {
                throw new System.NotImplementedException();
            }

            /// <summary>
            /// Returns the default choice for the choice context.
            /// </summary>
            /// <remarks>
            /// The actual value is not so important, only that it returns a valid value.
            /// </remarks>
            /// <param name="choiceMethod"></param>
            /// <param name="args"></param>
            /// <returns></returns>
            public override object GetDefaultChoice(MethodBase choiceMethod, object[] args)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }

        #endregion

        #region Variables

        private AttributedChoiceResolverProvider m_provider;

        #endregion

        #region Setup / Teardown

        [SetUp]
        public void Setup()
        {
            m_provider = new AttributedChoiceResolverProvider(new AIParameters());
        }

        #endregion

        #region Utilities

        private ChoiceResolver GetResolver(string methodName)
        {
            MethodBase method = GetType().GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method);
            return m_provider.GetResolver(method);
        }

        #endregion

        #region Test Methods

        [ChoiceResolver(typeof(MyResolver))]
        private static void ValidMethod()
        {
        }

        private static void NonAttributedMethod()
        {
        }

        [ChoiceResolver(typeof(AttributedChoiceResolverProviderTests))]
        private static void InvalidTypeMethod()
        {
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Construction_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => new AttributedChoiceResolverProvider(null));
        }

        [Test]
        public void Test_GetResolver_Invalid_arguments()
        {
            Assert.Throws<ArgumentNullException>(() => m_provider.GetResolver(null));
            Assert.Throws<ArgumentException>(() => GetResolver("NonAttributedMethod"));
            Assert.Throws<ArgumentException>(() => GetResolver("InvalidTypeMethod"));
        }

        [Test]
        public void Test_GetResolver_uses_attribute_to_find_type()
        {
            Assert.IsInstanceOf<MyResolver>(GetResolver("ValidMethod"));
            Assert.AreSame(GetResolver("ValidMethod"), GetResolver("ValidMethod")); // Always returns the same instance?
        }

        #endregion
    }
}
