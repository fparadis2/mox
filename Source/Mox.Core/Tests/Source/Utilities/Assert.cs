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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Mox.Transactions;

namespace Mox
{
    /// <summary>
    /// Contains assert utilities for unit tests.
    /// </summary>
    public partial class Assert : NUnit.Framework.Assert
    {
        #region Methods

        #region Utility

        /// <summary>
        /// Returns the string representation of an object
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static string GetDisplayString(object value)
        {
            if (object.ReferenceEquals(value, null))
            {
                return "null";
            }

            if (value is string)
            {
                return (string)value;
            }

            if (value is System.Collections.IEnumerable)
            {
                const int MaxItems = 100;

                System.Collections.IEnumerable collection = (System.Collections.IEnumerable)value;                
                return collection.
                    Cast<object>().
                    Take(MaxItems).
                    Select((o, i) => GetDisplayString(o) + (i == MaxItems - 1 ? "..." : string.Empty)).
                    Join(", ");
            }

            return value.ToString();
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Asserts that the given <paramref name="action"/> doesn't throw.
        /// </summary>
        /// <param name="action"></param>
        [DebuggerStepThrough]
        public static void DoesntThrow(Action action)
        {
            Throw.IfNull(action, "action");

            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.Fail("An exception of type {0} was thrown: {1}", e.GetType().Name, e.Message);
            }
        }

        /// <summary>
        /// Asserts that the given <paramref name="action"/> throws an exception of the given <typeparamref name="TException"/> type.
        /// </summary>
        /// <param name="action"></param>
        /// <typeparam name="TException">Type of the expected exception.</typeparam>
        [DebuggerStepThrough]
        public static void Throws<TException>(Action action)
            where TException : Exception
        {
            Throw.IfNull(action, "action");

            bool thrown = true;

            try
            {
                action();
                thrown = false;                
            }
            catch (TException)
            {
            }
            catch (Exception e)
            {
                Assert.Fail("An exception of type {0} was thrown but expected an exception of type {1}: {2}", e.GetType().Name, typeof(TException).Name, e.Message);
            }

            if (!thrown)
            {
                Assert.Fail("No exception was thrown (expected an exception of type {0}).", typeof(TException).Name);
            }
        }

        #endregion

        #region Logging

        /// <summary>
        /// Asserts that the given <paramref name="operation"/> doesn't log any errors/warnings.
        /// </summary>
        public static void DoesntLogErrorsOrWarnings(Action<LogContext> operation)
        {
            Throw.IfNull(operation, "operation");

            LogContext context = new LogContext();
            operation(context);
            if (context.Errors.Count != 0)   { Assert.Fail("Expected no errors but {0} were logged. First error: {1}", context.Errors.Count, context.Errors[0]); }
            if (context.Warnings.Count != 0) { Assert.Fail("Expected no warnings but {0} were logged. First warning: {1}", context.Warnings.Count, context.Warnings[0]); }
        }

        /// <summary>
        /// Asserts that the given <paramref name="operation"/> logs some errors/warnings.
        /// </summary>
        public static void Logs(Action<LogContext> operation, params string[] errorCodes)
        {
            Throw.IfNull(operation, "operation");

            LogContext context = new LogContext();
            operation(context);

            List<string> usedErrorCodes = new List<string>(errorCodes);

            foreach (LogMessage message in context.AllMessages)
            {
                if (!usedErrorCodes.Remove(message.Code))
                {
                    Assert.Fail("Received unexpected log message: {0}", message);
                }
            }

            if (usedErrorCodes.Count > 0)
            {
                Assert.Fail("Expected to receive log messages with these error codes: {0}", usedErrorCodes.Join(", "));
            }
        }

        #endregion

        #region Event Asserts

        /// <summary>
        /// Asserts that the given <paramref name="eventSink"/> is not triggered by the given <paramref name="action"/>.
        /// </summary>
        /// <param name="eventSink">Event sink bound to the event</param>
        /// <param name="action">Operation that should not raise the event</param>
        public static void EventNotCalled(IEventSink eventSink, Action action)
        {
            EventCalled(eventSink, action, 0, false);
        }

        /// <summary>
        /// Asserts that the given <paramref name="eventSink"/> is triggered exactly once by the given <paramref name="action"/>.
        /// </summary>
        /// <param name="eventSink">Event sink binded to the event</param>
        /// <param name="action">Operation that triggers the event</param>
        public static void EventCalledOnce(IEventSink eventSink, Action action)
        {
            EventCalled(eventSink, action, 1, false);
        }

        /// <summary>
        /// Asserts that the given <paramref name="eventSink"/> is triggered at least once by the given <paramref name="action"/>.
        /// </summary>
        /// <param name="eventSink">Event sink binded to the event</param>
        /// <param name="action">Operation that triggers the event</param>
        public static void EventCalled(IEventSink eventSink, Action action)
        {
            EventCalled(eventSink, action, 1, true);
        }

        /// <summary>
        /// Asserts that the given <paramref name="eventSink"/> is triggered exactly X times by the given <paramref name="action"/>.
        /// </summary>
        /// <param name="eventSink">Event sink binded to the event</param>
        /// <param name="action">Operation that triggers the event</param>
        /// <param name="expectedTimes">Number of times the event should be called.</param>
        public static void EventCalled(IEventSink eventSink, Action action, int expectedTimes)
        {
            EventCalled(eventSink, action, expectedTimes, false);
        }

        private static void EventCalled(IEventSink eventSink, Action action, int expectedTimes, bool atLeast)
        {
            int timesCalledBefore = eventSink.TimesCalled;
            action();
            int timesCalled = eventSink.TimesCalled - timesCalledBefore;

            if (expectedTimes == timesCalled || (expectedTimes < timesCalled && atLeast))
            {
                return; // Ok
            }
            else
            {
                string expectedLine;
                if (expectedTimes == 0 && !atLeast)
                {
                    expectedLine = "event to never be called";
                }
                else
                {
                    expectedLine = string.Format("event to be called {0}{1} times", atLeast ? "at least " : "", expectedTimes);
                }

                Assert.Fail("Expected {0} but event was called {1} times", expectedLine, timesCalled);
            }
        }

        #endregion

        #region Serialization

        /// <summary>
        /// Asserts that given <paramref name="instance"/> is serializable and returns the result.
        /// </summary>
        /// <remarks>
        /// This method does not check that the result is correct, it only checks that the operation doesn't throw.
        /// </remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static T IsSerializable<T>(T instance)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, instance);
                    stream.Seek(0, SeekOrigin.Begin);
                    return (T)formatter.Deserialize(stream);
                }
            }
            catch (SerializationException e)
            {
                Fail("Object {0} could not be serialized: {1}", instance, e);
                return default(T);
            }
        }

        #endregion

        #region Complete equality

        /// <summary>
        /// Asserts that <paramref name="actual"/> behaves as being completely equals to <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreCompletelyEqual<T>(T expected, T actual)
        {
            AreCompletelyEqual(expected, actual, true);
        }

        /// <summary>
        /// Asserts that <paramref name="actual"/> behaves as being completely equal to <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="checkOperators"></param>
        public static void AreCompletelyEqual<T>(T expected, T actual, bool checkOperators)
        {
            AreCompletelyEqualGeneric(expected, actual, true, checkOperators);
        }

        /// <summary>
        /// Asserts that <paramref name="actual"/> behaves as being completely not equal to <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        public static void AreCompletelyNotEqual<T>(T expected, T actual)
        {
            AreCompletelyNotEqual(expected, actual, true);
        }

        /// <summary>
        /// Asserts that <paramref name="actual"/> behaves as being completely not equal to <paramref name="expected"/>.
        /// </summary>
        /// <param name="expected"></param>
        /// <param name="actual"></param>
        /// <param name="checkOperators"></param>
        public static void AreCompletelyNotEqual<T>(T expected, T actual, bool checkOperators)
        {
            AreCompletelyEqualGeneric(expected, actual, false, checkOperators);
        }

        private static void AreCompletelyEqualGeneric<T>(T expected, T actual, bool equality, bool checkOperators)
        {
            if (equality)
            {
                Assert.IsNotNull(expected, "expected");
            }

            Assert.IsNotNull(actual, "actual");

            Assert.AreEqual(equality, actual.Equals(expected), "{0} is equal (object.Equals) to {1}", actual, expected);

            if (!object.ReferenceEquals(expected, null))
            {
                Assert.AreEqual(equality, expected.Equals(actual), "{1} is equal (object.Equals) to {0}", actual, expected);
            }

            if (equality)
            {
                Debug.Assert(!object.ReferenceEquals(expected, null), "Should have been checked at beginning of method");
                Assert.AreEqual(expected.GetHashCode(), actual.GetHashCode(), "HashCode for {0} ({1}) is not equal to HashCode for {2} ({3}). Equal objects should have the same HashCode.", actual, actual.GetHashCode(), expected, expected.GetHashCode());
            }

            Type equatableInterface = GetIEquatableInterface(actual.GetType(), typeof(T));
            if (equatableInterface != null)
            {
                MethodInfo equalsMethod = equatableInterface.GetMethod("Equals", BindingFlags.Public | BindingFlags.Instance);
                Assert.AreEqual(equality, (bool)equalsMethod.Invoke(actual, new object[] { expected }), "{0} is equal ({2}.Equals) to {1}", actual, expected, equatableInterface.Name);
            }

            if (checkOperators)
            {
                MethodInfo equalityOperator = GetEqualityOperator(actual.GetType(), "op_Equality");
                Assert.AreEqual(equality, (bool)equalityOperator.Invoke(null, new object[] { actual, expected }), "{0} is equal (operator !=) to {1}", actual, expected);

                MethodInfo inequalityOperator = GetEqualityOperator(actual.GetType(), "op_Inequality");
                Assert.AreEqual(equality, !(bool)inequalityOperator.Invoke(null, new object[] { actual, expected }), "{0} is equal (operator !=) to {1}", actual, expected);
            }
        }

        private static MethodInfo GetEqualityOperator(Type type, string name)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);
            Assert.IsNotNull(method, "Cannot find method {0} on type {1}", name, type.FullName);
            return method;
        }

        private static Type GetIEquatableInterface(Type actualType, Type expectedType)
        {
            foreach (Type interfaceType in actualType.GetInterfaces())
            {
                if (interfaceType.FullName.StartsWith("System.IEquatable"))
                {
                    Type[] genericArgs = interfaceType.GetGenericArguments();
                    if (genericArgs.Length == 1 && genericArgs[0].IsAssignableFrom(expectedType))
                    {
                        return interfaceType;
                    }
                }
            }

            return null;
        }

        #endregion

        #endregion
    }
}
