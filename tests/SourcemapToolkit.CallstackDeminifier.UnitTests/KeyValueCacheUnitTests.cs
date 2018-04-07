using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
    [TestClass]
    public class KeyValueCacheUnitTests
    {
        [TestMethod]
        public void GetValue_KeyNotInCache_CallValueGetter()
        {
            // Arrange
            Func<string, string> valueGetter = x => "foo";
            var keyValueCache = new KeyValueCache<string, string>(valueGetter);
            // Act
            string result = keyValueCache.GetValue("bar");
            // Assert
            Assert.AreEqual("foo", result);

        }

        [TestMethod]
        public void GetValue_CallGetTwice_OnlyCallValueGetterOnce()
        {
            // Arrange
            int i = 0;
            Func<string, string> valueGetter = x =>
            {
                i++;
                return "foo";
            };

            var keyValueCache = new KeyValueCache<string, string>(valueGetter);
            keyValueCache.GetValue("bar"); // Place the value in the cache

            // Act
            string result = keyValueCache.GetValue("bar");

            // Assert
            Assert.AreEqual("foo", result);
            Assert.AreEqual(i, 1);
        }

        [TestMethod]
        public void GetValue_CallGetTwiceValueGetterReturnsNull_CallGetterTwice()
        {
            // Arrange
            int i = 0;
            Func<string, string> valueGetter = x =>
            {
                i++;
                return null;
            };
            var keyValueCache = new KeyValueCache<string, string>(valueGetter);
            keyValueCache.GetValue("bar"); // Place null in the cache

            // Act
            string result = keyValueCache.GetValue("bar");

            // Assert
            Assert.AreEqual(null, result);
            Assert.AreEqual(i, 2);
        }

        [TestMethod]
        public void GetValue_CallGetMultipleTimesFirstGetterReturnsNull_CacheFirstNonNullValue()
        {
            // Arrange
            int i = 0;
            string value = null;
            Func<string, string> valueGetter = x =>
            {
                i++;
                var val = value;
                value = "foo";
                return val;
            };

            var keyValueCache = new KeyValueCache<string, string>(valueGetter);
            keyValueCache.GetValue("bar"); // Place null in the cache            
            keyValueCache.GetValue("bar"); // Place a non null value in the cahce

            // Act
            string result = keyValueCache.GetValue("bar");

            // Assert
            Assert.AreEqual("foo", result);
            Assert.AreEqual(i, 2);
        }
    }
}
