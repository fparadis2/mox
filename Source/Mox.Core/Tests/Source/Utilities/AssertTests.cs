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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Mox.Transactions;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class AssertTests
    {
        #region Mock objects

        private class NonSerializableClass
        {
        }

        #endregion

        #region Tests

        #region Strings

        [Test]
        public void Test_Assert_IsNullOrEmpty_on_strings()
        {
            Assert.DoesntThrow(delegate { Assert.IsNullOrEmpty(null); });
            Assert.DoesntThrow(delegate { Assert.IsNullOrEmpty(string.Empty); });
            Assert.Throws<AssertionException>(delegate { Assert.IsNullOrEmpty("My String"); });
        }

        #endregion

        #region Exception Handling

        [Test]
        public void Test_cannot_call_DoesntThrow_with_a_null_action()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.DoesntThrow(null));
        }

        [Test]
        public void Test_DoesntThrow_succeeds_when_the_method_doesnt_throw()
        {
            Assert.DoesntThrow(delegate { });
        }

        [Test]
        public void Test_DoesntThrow_fails_when_the_method_throws()
        {
            Assert.Throws<AssertionException>(() => Assert.DoesntThrow(delegate { throw new Exception(); }));
        }

        [Test]
        public void Test_cannot_call_Throws_with_a_null_action()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Throws<Exception>(null));
        }

        [Test]
        public void Test_Throws_fails_when_no_exception_is_thrown()
        {
            Assert.Throws<AssertionException>(() => Assert.Throws<Exception>(delegate { }));
        }

        [Test]
        public void Test_Throws_fails_when_an_exception_is_thrown_but_not_of_the_expected_type()
        {
            Assert.Throws<AssertionException>(() => Assert.Throws<ArgumentException>(delegate { throw new Exception(); }));
        }

        [Test]
        public void Test_Throws_succeeds_when_an_exception_of_the_expected_type_is_thrown()
        {
            Assert.Throws<ArgumentException>(delegate { throw new ArgumentNullException(); });
        }

        #endregion

        #region Logging

        [Test]
        public void Test_Assert_DoesntLogErrorsOrWarnings()
        {
            LogMessage message = new LogMessage() { Importance = LogImportance.Normal };
            LogMessage error = new LogMessage() { Importance = LogImportance.Error };
            LogMessage warning = new LogMessage() { Importance = LogImportance.Warning };

            Assert.Throws<ArgumentNullException>(delegate { Assert.DoesntLogErrorsOrWarnings(null); });

            Assert.DoesntThrow(delegate { Assert.DoesntLogErrorsOrWarnings(context => { }); });
            Assert.DoesntThrow(delegate { Assert.DoesntLogErrorsOrWarnings(context => context.Log(message)); });

            Assert.Throws<AssertionException>(delegate { Assert.DoesntLogErrorsOrWarnings(context => context.Log(error)); });
            Assert.Throws<AssertionException>(delegate { Assert.DoesntLogErrorsOrWarnings(context => context.Log(warning)); });
        }

        [Test]
        public void Test_Assert_LogsErrorsOrWarnings()
        {
            LogMessage error = new LogMessage() { Code = "Error" };
            LogMessage warning = new LogMessage() { Code = "Warning" };

            Assert.Throws<ArgumentNullException>(delegate { Assert.Logs(null); });

            Assert.DoesntThrow(delegate { Assert.Logs(context => { }); });
            Assert.DoesntThrow(delegate { Assert.Logs(context => context.Log(error), "Error"); });
            Assert.DoesntThrow(delegate { Assert.Logs(context => context.Log(warning), "Warning"); });
            Assert.DoesntThrow(delegate { Assert.Logs(context => { context.Log(warning); context.Log(error); context.Log(warning); }, "Warning", "Error", "Warning"); });

            Assert.Throws<AssertionException>(delegate { Assert.Logs(context => context.Log(error), "Warning"); });
            Assert.Throws<AssertionException>(delegate { Assert.Logs(context => context.Log(error), "Invalid"); });
            Assert.Throws<AssertionException>(delegate { Assert.Logs(context => context.Log(error), "Error", "Warning"); });
            Assert.Throws<AssertionException>(delegate { Assert.Logs(context => context.Log(error), "Error", "Error"); });
        }

        #endregion

        #region Serialization

        [Test]
        public void Test_IsSerializable()
        {
            Assert.AreEqual(3, Assert.IsSerializable(3));
            Assert.Throws<AssertionException>(() => Assert.IsSerializable(new NonSerializableClass()));
        }

        #endregion

        #region Complete Equality

        #region Inner Types

        private class EqualityTestClass
        {
            #region Variables

            public bool ObjectEqualsResult = true;
            public int GetHashCodeResult = 0;

            #endregion

            #region Methods

            public override bool Equals(object obj)
            {
                return ObjectEqualsResult;
            }

            public override int GetHashCode()
            {
                return GetHashCodeResult;
            }

            public virtual void SetState(bool equal)
            {
                ObjectEqualsResult = equal;
            }

            #endregion
        }

        private class EqualityClassWithOperators : EqualityTestClass
        {
            #region Variables

            public bool OperatorEqualsResult = true;
            public bool OperatorInequalsResult = false;

            #endregion

            #region Methods

            public static bool operator ==(EqualityClassWithOperators a, EqualityClassWithOperators b)
            {
                return a.OperatorEqualsResult;
            }

            public static bool operator !=(EqualityClassWithOperators a, EqualityClassWithOperators b)
            {
                return a.OperatorInequalsResult;
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override void SetState(bool equal)
            {
                base.SetState(equal);
                OperatorEqualsResult = equal;
                OperatorInequalsResult = !equal;
            }

            #endregion
        }

        private class IsEquatableClass : EqualityClassWithOperators, IEquatable<EqualityTestClass>
        {
            #region Variables

            public bool EquatableResult = true;

            #endregion

            #region Methods

            public bool Equals(EqualityTestClass other)
            {
                return EquatableResult;
            }

            public override void SetState(bool equal)
            {
                base.SetState(equal);
                EquatableResult = equal;
            }

            #endregion
        }

        #endregion

        [Test]
        public void Test_AreCompletelyEqual_works_only_if_all_equality_methods_are_consistent()
        {
            Assert.AreCompletelyEqual(new EqualityClassWithOperators(), new EqualityClassWithOperators());
        }

        [Test]
        public void Test_AreCompletelyEqual_fails_when_one_equality_method_fails()
        {
            EqualityTestClass expected = new EqualityClassWithOperators();

            EqualityTestClass actual = new EqualityClassWithOperators { ObjectEqualsResult = false };
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(expected, actual));

            actual = new EqualityClassWithOperators { OperatorEqualsResult = false };
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(expected, actual));

            actual = new EqualityClassWithOperators { OperatorInequalsResult = true };
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(expected, actual));
        }

        [Test]
        public void Test_AreCompletelyEqual_fails_when_GetHashCode_doesnt_return_the_same_value()
        {
            EqualityTestClass expected = new EqualityClassWithOperators();
            EqualityTestClass actual = new EqualityClassWithOperators { GetHashCodeResult = 15 };
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(expected, actual));
        }

        [Test]
        public void Test_AreCompletelyEqual_checks_IEquatable()
        {
            EqualityTestClass expected = new EqualityClassWithOperators();

            EqualityTestClass actual = new IsEquatableClass { EquatableResult = true };
            Assert.AreCompletelyEqual(expected, actual);

            actual = new IsEquatableClass { EquatableResult = false };
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(expected, actual));
        }

        [Test]
        public void Test_AreCompletelyEqual_fails_with_types_without_equality_operators_unless_specified()
        {
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyEqual(new EqualityTestClass(), new EqualityTestClass()));
            Assert.AreCompletelyEqual(new EqualityTestClass(), new EqualityTestClass(), false);
        }

        [Test]
        public void Test_AreCompletelyNotEqual_succeeds_if_all_methods_are_consistent()
        {
            EqualityClassWithOperators expected = new EqualityClassWithOperators();
            expected.SetState(false);

            EqualityClassWithOperators actual = new EqualityClassWithOperators();
            actual.SetState(false);

            Assert.AreCompletelyNotEqual(expected, actual);
        }

        [Test]
        public void Test_AreCompletelyNotEqual_fails_if_one_of_the_equality_methods_fail()
        {
            EqualityClassWithOperators expected = new EqualityClassWithOperators();
            EqualityClassWithOperators actual = new EqualityClassWithOperators();
            
            actual.SetState(false);
            actual.ObjectEqualsResult = true;
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(expected, actual));

            actual.SetState(false);
            actual.OperatorEqualsResult = true;
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(expected, actual));

            actual.SetState(false);
            actual.OperatorInequalsResult = false;
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(expected, actual));

            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(new EqualityClassWithOperators(), new EqualityClassWithOperators()));
        }

        [Test]
        public void Test_AreCompletelyNotEqual_checks_IEquatable()
        {
            IsEquatableClass expected = new IsEquatableClass();
            expected.SetState(false);

            IsEquatableClass actual = new IsEquatableClass();

            actual.SetState(false);
            actual.EquatableResult = false;
            Assert.AreCompletelyNotEqual(expected, actual);

            actual.SetState(false);
            actual.EquatableResult = true;
            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(expected, actual));
        }

        [Test]
        public void Test_AreCompletelyNotEqual_fails_with_types_without_equality_operators_unless_specified()
        {
            EqualityTestClass expected = new EqualityTestClass();
            expected.SetState(false);

            EqualityTestClass actual = new EqualityTestClass();
            actual.SetState(false);

            Assert.Throws<AssertionException>(() => Assert.AreCompletelyNotEqual(expected, actual));
            Assert.AreCompletelyNotEqual(expected, actual, false);
        }

        [Test]
        public void Test_AreCompletelyNotEqual_works_with_null()
        {
            IsEquatableClass actual = new IsEquatableClass();
            actual.SetState(false);
            Assert.AreCompletelyNotEqual(null, actual);
        }

        #endregion

        #endregion
    }
}
