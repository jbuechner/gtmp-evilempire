using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gtmp.evilempire.tests
{
    [TestClass]
    public class ConversionTests
    {
        [TestMethod]
        public void AsIntWithNullValue()
        {
            var result = ((object)null).AsInt();
            Assert.AreEqual(null, result);
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void AsIntWithStringValue()
        {
            var result = "123".AsInt();
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(123, result);
        }

        [TestMethod]
        public void AsValueWithoutHexSpecifier()
        {
            var result = "FF".AsInt();
            Assert.IsFalse(result.HasValue);
        }

        [TestMethod]
        public void AsHexWithSpecifier()
        {
            var result = "0x89".AsInt();
            Assert.IsTrue(result.HasValue);
            Assert.AreEqual(0x89, result);
        }
    }
}
