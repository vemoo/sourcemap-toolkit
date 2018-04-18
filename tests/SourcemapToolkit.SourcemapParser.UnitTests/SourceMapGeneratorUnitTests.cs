using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
    [TestClass]
    public class SourceMapGeneratorUnitTests
    {
        [TestMethod]
        public void SerializeMappingEntry_DifferentLineNumber_SemicolonAdded()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();

            var state = new MappingGenerateState(new List<string>() {"Name"}, new List<string>() {"Source"});
            state.LastGeneratedPosition.ZeroBasedColumnNumber = 1;

            var entry = new MappingEntry()
            {
                GeneratedSourcePosition = new SourcePosition(1, 0),
                OriginalFileName = state.Sources[0],
                OriginalName = state.Names[0],
                OriginalSourcePosition = new SourcePosition(1, 0),
            };

            // Act
            var result = new StringBuilder();
            sourceMapGenerator.SerializeMappingEntry(entry, state, result);

            // Assert
            Assert.IsTrue(result.ToString().IndexOf(';') >= 0);
        }

        [TestMethod]
        public void SerializeMappingEntry_NoOriginalFileName_OneSegment()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();

            var state = new MappingGenerateState(new List<string>() {"Name"}, new List<string>() {"Source"});

            var entry = new MappingEntry()
            {
                GeneratedSourcePosition = new SourcePosition(0, 10),
                OriginalSourcePosition = new SourcePosition(0, 1),
            };

            // Act
            var result = new StringBuilder();
            sourceMapGenerator.SerializeMappingEntry(entry, state, result);

            // Assert
            Assert.AreEqual("U", result.ToString());
        }

        [TestMethod]
        public void SerializeMappingEntry_WithOriginalFileNameNoOriginalName_FourSegments()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();

            var state = new MappingGenerateState(new List<string>() {"Name"}, new List<string>() {"Source"});
            state.IsFirstSegment = false;

            var entry = new MappingEntry()
            {
                GeneratedSourcePosition = new SourcePosition(0, 10),
                OriginalFileName = state.Sources[0],
                OriginalSourcePosition = new SourcePosition(0, 5),
            };

            // Act
            var result = new StringBuilder();
            sourceMapGenerator.SerializeMappingEntry(entry, state, result);

            // Assert
            Assert.AreEqual(",UAAK", result.ToString());
        }

        [TestMethod]
        public void SerializeMappingEntry_WithOriginalFileNameAndOriginalName_FiveSegments()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();

            var state = new MappingGenerateState(new List<string>() {"Name"}, new List<string>() {"Source"});
            state.LastGeneratedPosition.ZeroBasedLineNumber = 1;

            var entry = new MappingEntry()
            {
                GeneratedSourcePosition = new SourcePosition(1, 5),
                OriginalSourcePosition = new SourcePosition(1, 6),
                OriginalFileName = state.Sources[0],
                OriginalName = state.Names[0],
            };

            // Act
            var result = new StringBuilder();
            sourceMapGenerator.SerializeMappingEntry(entry, state, result);

            // Assert
            Assert.AreEqual("KACMA", result.ToString());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SerializeMapping_NullInput_ThrowsException()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();
            SourceMap input = null;

            // Act
            var output = sourceMapGenerator.SerializeMapping(input);
        }

        [TestMethod]
        public void SerializeMapping_SimpleSourceMap_CorrectlySerialized()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();
            var input = this.GetSimpleSourceMap();

            // Act
            var output = sourceMapGenerator.SerializeMapping(input);

            // Assert
            Assert.AreEqual(
                "{\"version\":3,\"file\":\"CommonIntl\",\"mappings\":\"AACAA,aAAA,CAAc;\",\"sources\":[\"input/CommonIntl.js\"],\"names\":[\"CommonStrings\",\"afrikaans\"]}",
                output);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SerializeMappingIntoBast64_NullInput_ThrowsException()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();
            SourceMap input = null;

            // Act
            var output = sourceMapGenerator.GenerateSourceMapInlineComment(input);
        }

        [TestMethod]
        public void SerializeMappingBase64_SimpleSourceMap_CorrectlySerialized()
        {
            // Arrange
            var sourceMapGenerator = new SourceMapGenerator();
            var input = this.GetSimpleSourceMap();

            // Act
            var output = sourceMapGenerator.GenerateSourceMapInlineComment(input);

            // Assert
            Assert.AreEqual(
                "//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoiQ29tbW9uSW50bCIsIm1hcHBpbmdzIjoiQUFDQUEsYUFBQSxDQUFjOyIsInNvdXJjZXMiOlsiaW5wdXQvQ29tbW9uSW50bC5qcyJdLCJuYW1lcyI6WyJDb21tb25TdHJpbmdzIiwiYWZyaWthYW5zIl19",
                output);
        }

        private SourceMap GetSimpleSourceMap()
        {
            var input = new SourceMap()
            {
                File = "CommonIntl",
                Names = new List<string>() {"CommonStrings", "afrikaans"},
                Sources = new List<string>() {"input/CommonIntl.js"},
                Version = 3,
            };
            input.ParsedMappings = new List<MappingEntry>()
            {
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 0),
                    OriginalFileName = input.Sources[0],
                    OriginalName = input.Names[0],
                    OriginalSourcePosition = new SourcePosition(1, 0),
                },
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 13),
                    OriginalFileName = input.Sources[0],
                    OriginalSourcePosition = new SourcePosition(1, 0),
                },
                new MappingEntry
                {
                    GeneratedSourcePosition = new SourcePosition(0, 14),
                    OriginalFileName = input.Sources[0],
                    OriginalSourcePosition = new SourcePosition(1, 14),
                },
            };

            return input;
        }
    }
}