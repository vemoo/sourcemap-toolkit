using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
    [TestClass]
    public class StackTraceDeminifierUnitTests
    {
        [TestMethod]
        public void DeminifyStackTrace_UnableToParseStackTraceString_ReturnsEmptyList()
        {
            // Arrange
            var stackTraceParserMock = new Mock<IStackTraceParser>();
            var stackTraceString = "foobar";
            stackTraceParserMock
                .Setup(x => x.ParseStackTrace(stackTraceString))
                .Returns(new List<StackFrame>());

            var stackFrameDeminifierMock = new Mock<IStackFrameDeminifier>();

            var stackTraceDeminifier = new StackTraceDeminifier(
                stackFrameDeminifierMock.Object, stackTraceParserMock.Object);
            // Act
            var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

            // Assert
            Assert.AreEqual(0, result.DeminifiedStackFrameResults.Count);
        }

        [TestMethod]
        public void DeminifyStackTrace_UnableToDeminifyStackTrace_ResultContainsNullDeminifiedFrame()
        {
            // Arrange
            var stackTraceParserMock = new Mock<IStackTraceParser>();
            var minifiedStackFrames = new List<StackFrame> { new StackFrame(null, null, null) };
            var stackTraceString = "foobar";
            stackTraceParserMock
                .Setup(x => x.ParseStackTrace(stackTraceString))
                .Returns(minifiedStackFrames);

            var stackFrameDeminifierMock = new Mock<IStackFrameDeminifier>();
            stackFrameDeminifierMock
                .Setup(x => x.DeminifyStackFrame(minifiedStackFrames[0]))
                .Returns((StackFrameDeminificationResult)null);

            var stackTraceDeminifier = new StackTraceDeminifier(
                stackFrameDeminifierMock.Object, stackTraceParserMock.Object);

            // Act
            var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

            // Assert
            Assert.AreEqual(1, result.DeminifiedStackFrameResults.Count);
            Assert.AreEqual(minifiedStackFrames[0], result.MinifiedStackFrames[0]);
            Assert.IsNull(result.DeminifiedStackFrameResults[0]);
        }

        [TestMethod]
        public void DeminifyStackTrace_AbleToDeminifyStackTrace_ResultContainsDeminifiedFrame()
        {
            // Arrange
            var stackTraceParserMock = new Mock<IStackTraceParser>();
            var minifiedStackFrames = new List<StackFrame> { new StackFrame(null, null, null) };
            var stackTraceString = "foobar";
            stackTraceParserMock
                .Setup(x => x.ParseStackTrace(stackTraceString))
                .Returns(minifiedStackFrames);

            var stackFrameDeminifierMock = new Mock<IStackFrameDeminifier>();
            var stackFrameDeminification = StackFrameDeminificationResult.Ok(null);
            stackFrameDeminifierMock
                .Setup(x => x.DeminifyStackFrame(minifiedStackFrames[0]))
                .Returns(stackFrameDeminification);

            var stackTraceDeminifier = new StackTraceDeminifier(
                stackFrameDeminifierMock.Object, stackTraceParserMock.Object);

            // Act
            var result = stackTraceDeminifier.DeminifyStackTrace(stackTraceString);

            // Assert
            Assert.AreEqual(1, result.DeminifiedStackFrameResults.Count);
            Assert.AreEqual(minifiedStackFrames[0], result.MinifiedStackFrames[0]);
            Assert.AreEqual(stackFrameDeminification, result.DeminifiedStackFrameResults[0]);
        }
    }
}