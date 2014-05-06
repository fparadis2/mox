using System;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class ObjectHashTests
    {
        #region Mock Types

        private class MyObject : Object
        {
            private int m_int;
            private static readonly Property<int> IntProperty = Property<int>.RegisterProperty<MyObject>("Int", o => o.m_int);
            public int Int
            {
                get { return m_int; }
                set { SetValue(IntProperty, value, ref m_int); }
            }

            private int m_ignoreHash;
            private static readonly Property<int> IgnoreHashProperty = Property<int>.RegisterProperty<MyObject>("IgnoreHash", o => o.m_ignoreHash, PropertyFlags.IgnoreHash);
            public int IgnoreHash
            {
                get { return m_ignoreHash; }
                set { SetValue(IgnoreHashProperty, value, ref m_ignoreHash); }
            }

            private MyObject m_reference;
            private static readonly Property<MyObject> ReferenceProperty = Property<MyObject>.RegisterProperty<MyObject>("Reference", o => o.m_reference);
            public MyObject Reference
            {
                get { return m_reference; }
                set { SetValue(ReferenceProperty, value, ref m_reference); }
            }
        }

        private class MyDerivedObject : MyObject
        {
        }

        #endregion

        #region Variables

        private MockObjectManager m_manager;

        #endregion

        #region Setup

        [SetUp]
        public void SetUp()
        {
            m_manager = new MockObjectManager();
        }

        #endregion

        #region Tests

        [Test]
        public void Test_Null_references_have_a_zero_hash()
        {
            ObjectHash hash = new ObjectHash(m_manager);
            Assert.AreEqual(0, hash.Hash(null));
        }

        [Test]
        public void Test_Identical_objects_have_the_same_hash()
        {
            var obj = m_manager.CreateAndAdd<MyObject>();
            Assert.HashIsEqual(obj, obj);
            Assert.HashIsEqual(obj, m_manager.CreateAndAdd<MyObject>());
        }

        [Test]
        public void Test_Objects_with_identical_properties_have_the_same_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();
            a.Int = 3;

            var b = m_manager.CreateAndAdd<MyObject>();
            b.Int = 3;

            Assert.HashIsEqual(a, b);
        }

        [Test]
        public void Test_Objects_with_different_properties_dont_have_the_same_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();
            a.Int = 3;

            var b = m_manager.CreateAndAdd<MyObject>();
            b.Int = 4;

            Assert.HashIsNotEqual(a, b);
        }

        [Test]
        public void Test_Properties_with_IgnoreHash_dont_contribute_to_the_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();
            a.IgnoreHash = 3;

            var b = m_manager.CreateAndAdd<MyObject>();
            b.IgnoreHash = 4;

            Assert.HashIsEqual(a, b);
        }

        [Test]
        public void Test_Objects_always_include_their_concrete_type_in_the_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();
            var b = m_manager.CreateAndAdd<MyDerivedObject>();

            Assert.HashIsNotEqual(a, b);
        }

        [Test]
        public void Test_References_to_other_objects_contribute_to_the_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();

            var b = m_manager.CreateAndAdd<MyObject>();
            b.Int = 3;

            var c = m_manager.CreateAndAdd<MyObject>();
            c.Int = 4;

            Assert.HashChanges(a, () => a.Reference = b);
            Assert.HashChanges(a, () => a.Reference = c);
            Assert.HashChanges(a, () => c.Int = 5);
        }

        [Test]
        public void Test_Referencing_identical_objects_doesnt_change_the_hash()
        {
            var a = m_manager.CreateAndAdd<MyObject>();

            var b = m_manager.CreateAndAdd<MyObject>();
            b.Int = 3;

            var c = m_manager.CreateAndAdd<MyObject>();
            c.Int = 3;

            a.Reference = b;
            Assert.HashDoesntChange(a, () => a.Reference = c);
        }

        [Test]
        public void Test_Cyclic_References_are_not_supported()
        {
            var a = m_manager.CreateAndAdd<MyObject>();
            a.Reference = a;

            Assert.Throws<InvalidOperationException>(() => new ObjectHash(m_manager).Hash(a));
        }

        #endregion
    }
}
