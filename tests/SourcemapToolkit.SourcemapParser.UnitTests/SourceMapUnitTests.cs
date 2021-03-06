﻿using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
    [TestClass]
    public class SourceMapUnitTests
    {
        [TestMethod]
        public void GetMappingEntryForGeneratedSourcePosition_NullMappingList_ReturnNull()
        {
            // Arrange
            var sourceMap = new SourceMap();
            var sourcePosition = new SourcePosition(4, 3);

            // Act
            var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

            // Asset
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetMappingEntryForGeneratedSourcePosition_NoMatchingEntryInMappingList_ReturnNull()
        {
            // Arrange
            var sourceMap = new SourceMap();
            sourceMap.ParsedMappings = new List<MappingEntry>
            {
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 0),
                }
            };
            var sourcePosition = new SourcePosition(4, 3);

            // Act
            var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

            // Asset
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetMappingEntryForGeneratedSourcePosition_MatchingEntryInMappingList_ReturnMatchingEntry()
        {
            // Arrange
            var sourceMap = new SourceMap();
            var matchingMappingEntry = new MappingEntry
            {
                GeneratedSourcePosition = new SourcePosition(8, 13)
            };
            sourceMap.ParsedMappings = new List<MappingEntry>
            {
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 0)
                },
                matchingMappingEntry
            };
            var sourcePosition = new SourcePosition(8, 13);

            // Act
            var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

            // Asset
            Assert.AreEqual(matchingMappingEntry, result);
        }

        [TestMethod]
        public void GetMappingEntryForGeneratedSourcePosition_NoExactMatchHasSimilarOnSameLine_ReturnSimilarEntry()
        {
            // Arrange
            var sourceMap = new SourceMap();
            var matchingMappingEntry = new MappingEntry
            {
                GeneratedSourcePosition = new SourcePosition(10, 13)
            };
            sourceMap.ParsedMappings = new List<MappingEntry>
            {
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 0)
                },
                matchingMappingEntry
            };
            var sourcePosition = new SourcePosition(10, 14);

            // Act
            var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

            // Asset
            Assert.AreEqual(matchingMappingEntry, result);
        }

        [TestMethod]
        public void
            GetMappingEntryForGeneratedSourcePosition_NoExactMatchHasSimilarOnDifferentLinesLine_ReturnSimilarEntry()
        {
            // Arrange
            var sourceMap = new SourceMap();
            var matchingMappingEntry = new MappingEntry
            {
                GeneratedSourcePosition = new SourcePosition(23, 15)
            };
            sourceMap.ParsedMappings = new List<MappingEntry>
            {
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 0)
                },
                matchingMappingEntry
            };
            var sourcePosition = new SourcePosition(24, 0);

            // Act
            var result = sourceMap.GetMappingEntryForGeneratedSourcePosition(sourcePosition);

            // Asset
            Assert.AreEqual(matchingMappingEntry, result);
        }

        [TestMethod]
        public void GetRootMappingEntryForGeneratedSourcePosition_NoChildren_ReturnsSameEntry()
        {
            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 5);
            var mappingEntry = UnitTestUtils.GetSimpleEntry(generated1, original1, "generated.js");

            var sourceMap = new SourceMap
            {
                Sources = new List<string> {"generated.js"},
                ParsedMappings = new List<MappingEntry> {mappingEntry}
            };

            // Act
            var rootEntry = sourceMap.GetMappingEntryForGeneratedSourcePosition(generated1);

            // Assert
            Assert.AreEqual(rootEntry, mappingEntry);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ApplyMap_NullSubmap_ThrowsException()
        {
            // Arrange
            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
            var mapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

            var map = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js"},
                ParsedMappings = new List<MappingEntry> {mapping}
            };

            // Act
            var combinedMap = map.ApplySourceMap(null);

            // Assert (decorated expected exception)
        }

        [TestMethod]
        public void ApplyMap_NoMatchingSources_ReturnsSameMap()
        {
            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 3);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 2);
            var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "someOtherSource.js");

            var childMap = new SourceMap
            {
                File = "notSourceOne.js",
                Sources = new List<string> {"someOtherSource.js"},
                ParsedMappings = new List<MappingEntry> {childMapping}
            };

            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 7);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 3);
            var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

            var parentMap = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js"},
                ParsedMappings = new List<MappingEntry> {parentMapping}
            };

            // Act
            var combinedMap = parentMap.ApplySourceMap(childMap);

            // Assert
            Assert.IsNotNull(combinedMap);
            var firstMapping = combinedMap.ParsedMappings[0];
            Assert.IsTrue(firstMapping.IsValueEqual(parentMapping));
        }

        [TestMethod]
        public void ApplyMap_NoMatchingMappings_ReturnsSameMap()
        {
            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 2);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 10);
            var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

            var childMap = new SourceMap
            {
                File = "sourceOne.js",
                Sources = new List<string> {"sourceTwo.js"},
                ParsedMappings = new List<MappingEntry> {childMapping}
            };

            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 4);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 5);
            var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

            var parentMap = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js"},
                ParsedMappings = new List<MappingEntry> {parentMapping}
            };

            // Act
            var combinedMap = parentMap.ApplySourceMap(childMap);

            // Assert
            Assert.IsNotNull(combinedMap);
            var firstMapping = combinedMap.ParsedMappings[0];
            Assert.IsTrue(firstMapping.IsValueEqual(parentMapping));
        }

        [TestMethod]
        public void ApplyMap_MatchingSources_ReturnsCorrectMap()
        {
            // Expect mapping with same source filename as the applied source-map to be replaced

            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 4);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 3);
            var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

            var childMap = new SourceMap
            {
                File = "sourceOne.js",
                Sources = new List<string> {"sourceTwo.js"},
                ParsedMappings = new List<MappingEntry> {childMapping}
            };

            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 4);
            var parentMapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

            var parentMap = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js"},
                ParsedMappings = new List<MappingEntry> {parentMapping}
            };

            // Act
            var combinedMap = parentMap.ApplySourceMap(childMap);

            // Assert
            Assert.IsNotNull(combinedMap);
            Assert.AreEqual(1, combinedMap.ParsedMappings.Count);
            Assert.AreEqual(1, combinedMap.Sources.Count);
            var rootMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated2);
            Assert.AreEqual(0, rootMapping.OriginalSourcePosition.CompareTo(childMapping.OriginalSourcePosition));
        }

        [TestMethod]
        public void ApplyMap_PartialMatchingSources_ReturnsCorrectMap()
        {
            // Expect mappings with same source filename as the applied source-map to be replaced
            // mappings with a different source filename should stay the same

            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 1, colNumber: 5);
            var childMapping = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceTwo.js");

            var childMap = new SourceMap
            {
                File = "sourceOne.js",
                Sources = new List<string> {"sourceTwo.js"},
                ParsedMappings = new List<MappingEntry> {childMapping}
            };

            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 2);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
            var mapping = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceOne.js");

            var generated3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
            var original3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 2);
            var mapping2 = UnitTestUtils.GetSimpleEntry(generated3, original3, "noMapForThis.js");

            var parentMap = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js", "noMapForThis.js"},
                ParsedMappings = new List<MappingEntry> {mapping, mapping2}
            };

            // Act
            var combinedMap = parentMap.ApplySourceMap(childMap);

            // Assert
            Assert.IsNotNull(combinedMap);
            Assert.AreEqual(2, combinedMap.ParsedMappings.Count);
            Assert.AreEqual(2, combinedMap.Sources.Count);
            var firstCombinedMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated3);
            Assert.IsTrue(firstCombinedMapping.IsValueEqual(mapping2));
            var secondCombinedMapping = combinedMap.GetMappingEntryForGeneratedSourcePosition(generated2);
            Assert.AreEqual(0,
                secondCombinedMapping.OriginalSourcePosition.CompareTo(childMapping.OriginalSourcePosition));
        }

        [TestMethod]
        public void ApplyMap_ExactMatchDeep_ReturnsCorrectMappingEntry()
        {
            // Arrange
            var generated1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
            var original1 = UnitTestUtils.GenerateSourcePosition(lineNumber: 2, colNumber: 10);
            var mapLevel2 = UnitTestUtils.GetSimpleEntry(generated1, original1, "sourceThree.js");

            var grandChildMap = new SourceMap
            {
                File = "sourceTwo.js",
                Sources = new List<string> {"sourceThree.js"},
                ParsedMappings = new List<MappingEntry> {mapLevel2}
            };

            var generated2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
            var original2 = UnitTestUtils.GenerateSourcePosition(lineNumber: 3, colNumber: 5);
            var mapLevel1 = UnitTestUtils.GetSimpleEntry(generated2, original2, "sourceTwo.js");

            var childMap = new SourceMap
            {
                File = "sourceOne.js",
                Sources = new List<string> {"sourceTwo.js"},
                ParsedMappings = new List<MappingEntry> {mapLevel1}
            };

            var generated3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 5, colNumber: 5);
            var original3 = UnitTestUtils.GenerateSourcePosition(lineNumber: 4, colNumber: 3);
            var mapLevel0 = UnitTestUtils.GetSimpleEntry(generated3, original3, "sourceOne.js");

            var parentMap = new SourceMap
            {
                File = "generated.js",
                Sources = new List<string> {"sourceOne.js"},
                ParsedMappings = new List<MappingEntry> {mapLevel0}
            };

            // Act
            var firstCombinedMap = parentMap.ApplySourceMap(childMap);

            // Assert
            Assert.IsNotNull(firstCombinedMap);
            var secondCombinedMap = firstCombinedMap.ApplySourceMap(grandChildMap);
            Assert.IsNotNull(secondCombinedMap);
            var rootMapping = secondCombinedMap.GetMappingEntryForGeneratedSourcePosition(generated3);
            Assert.AreEqual(0, rootMapping.OriginalSourcePosition.CompareTo(mapLevel2.OriginalSourcePosition));
        }
    }
}