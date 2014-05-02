using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Mox
{
    [TestFixture]
    public class HashTests
    {
        #region Tests

        [Test]
        public void Test_Two_New_hashes_are_equal()
        {
            Hash h1 = new Hash();
            Hash h2 = new Hash();

            Assert.AreEqual(h1.Value, h2.Value);
        }

        [Test]
        public void Test_Adding_a_byte()
        {
            Hash hash = new Hash();
            hash.AddByte(2);

            Assert.AreNotEqual(new Hash().Value, hash.Value);
        }

        [Test]
        public void Test_Adding_an_int()
        {
            Hash hash = new Hash();
            hash.Add(2);

            Assert.AreNotEqual(new Hash().Value, hash.Value);
        }

        [Test]
        public void Test_Adding_a_large_int()
        {
            Hash hash = new Hash();
            hash.Add(int.MaxValue);
            hash.Add(int.MinValue);

            Assert.AreNotEqual(new Hash().Value, hash.Value);
        }

        [Test]
        public void Test_Adding_a_long()
        {
            Hash hash = new Hash();
            hash.Add(2L);

            Assert.AreNotEqual(new Hash().Value, hash.Value);
        }

        [Test]
        public void Test_Adding_a_large_long()
        {
            Hash hash = new Hash();
            hash.Add(long.MaxValue);
            hash.Add(long.MinValue);

            Assert.AreNotEqual(new Hash().Value, hash.Value);
        }

        [Test]
        public void Test_Adding_a_bool()
        {
            Hash hash1 = new Hash();
            hash1.Add(true);

            Hash hash2 = new Hash();
            hash2.Add(false);

            Assert.AreNotEqual(new Hash().Value, hash1.Value);
            Assert.AreNotEqual(new Hash().Value, hash2.Value);
            Assert.AreNotEqual(hash1.Value, hash2.Value);
        }

        [Test]
        public void Test_Adding_a_string()
        {
            Hash hash1 = new Hash();
            hash1.Add("Hello");

            Hash hash2 = new Hash();
            hash2.Add("World");

            Assert.AreNotEqual(new Hash().Value, hash1.Value);
            Assert.AreNotEqual(new Hash().Value, hash2.Value);
            Assert.AreNotEqual(hash1.Value, hash2.Value);
        }

        #endregion
    }
}
