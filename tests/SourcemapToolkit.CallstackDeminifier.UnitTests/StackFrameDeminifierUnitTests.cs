using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
    [TestClass]
    public class StackFrameDeminifierUnitTests
    {
        private IStackFrameDeminifier GetStackFrameDeminifierWithMockDependencies(ISourceMapStore sourceMapStore = null, IFunctionMapStore functionMapStore = null, IFunctionMapConsumer functionMapConsumer = null, bool useSimpleStackFrameDeminier = false)
        {
            if (sourceMapStore == null)
            {
                sourceMapStore = new Mock<ISourceMapStore>().Object;
            }

            if (functionMapStore == null)
            {
                functionMapStore = new Mock<IFunctionMapStore>().Object;
            }

            if (functionMapConsumer == null)
            {
                functionMapConsumer = new Mock<IFunctionMapConsumer>().Object;
            }

            if (useSimpleStackFrameDeminier)
            {
                return new SimpleStackFrameDeminifier(functionMapStore, functionMapConsumer);
            }
            else
            {
                return new StackFrameDeminifier(sourceMapStore, functionMapStore, functionMapConsumer);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DeminifyStackFrame_NullInputStackFrame_ThrowsException()
        {
            // Arrange
            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();
            StackFrame stackFrame = null;

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);
        }

        [TestMethod]
        public void DeminifyStackFrame_StackFrameNullProperties_DoesNotThrowException()
        {
            // Arrange
            StackFrame stackFrame = new StackFrame();
            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies();

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

        [TestMethod]
        public void SimpleStackFrameDeminierDeminifyStackFrame_FunctionMapReturnsNull_NoFunctionMapDeminificationError()
        {
            // Arrange
            string filePath = "foo";
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns((List<FunctionMapEntry>)null);

            IFunctionMapStore functionMapStore = functionMapStoreMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, useSimpleStackFrameDeminier: true);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.NoSourceCodeProvided, stackFrameDeminification.DeminificationError);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

        [TestMethod]
        public void SimpleStackFrameDeminierDeminifyStackFrame_GetWRappingFunctionForSourceLocationReturnsNull_NoWrapingFunctionDeminificationError()
        {
            // Arrange
            string filePath = "foo";
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns(new List<FunctionMapEntry>());
            var functionMapStore = functionMapStoreMock.Object;

            var functionMapConsumerMock = new Mock<IFunctionMapConsumer>();
            functionMapConsumerMock
                .Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
                .Returns((FunctionMapEntry)null);
            var functionMapConsumer = functionMapConsumerMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminier: true);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.NoWrapingFunctionFound, stackFrameDeminification.DeminificationError);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

        [TestMethod]
        public void SimpleStackFrameDeminierDeminifyStackFrame_WrapingFunctionFound_NoDeminificationError()
        {
            // Arrange
            string filePath = "foo";
            FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns(new List<FunctionMapEntry>());
            var functionMapStore = functionMapStoreMock.Object;

            var functionMapConsumerMock = new Mock<IFunctionMapConsumer>();
            functionMapConsumerMock
                .Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
                .Returns(wrapingFunctionMapEntry);
            var functionMapConsumer = functionMapConsumerMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer, useSimpleStackFrameDeminier: true);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.None, stackFrameDeminification.DeminificationError);
            Assert.AreEqual(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }


        [TestMethod]
        public void StackFrameDeminierDeminifyStackFrame_SourceMapProviderReturnsNull_NoSourcemapProvidedError()
        {
            // Arrange
            string filePath = "foo";
            FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns(new List<FunctionMapEntry>());
            var functionMapStore = functionMapStoreMock.Object;

            var functionMapConsumerMock = new Mock<IFunctionMapConsumer>();
            functionMapConsumerMock
                .Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
                .Returns(wrapingFunctionMapEntry);
            var functionMapConsumer = functionMapConsumerMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.NoSourceMap, stackFrameDeminification.DeminificationError);
            Assert.AreEqual(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

        [TestMethod]
        public void StackFrameDeminierDeminifyStackFrame_SourceMapParsingNull_SourceMapFailedToParseError()
        {
            // Arrange
            string filePath = "foo";
            FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns(new List<FunctionMapEntry>());
            var functionMapStore = functionMapStoreMock.Object;

            var functionMapConsumerMock = new Mock<IFunctionMapConsumer>();
            functionMapConsumerMock
                .Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
                .Returns(wrapingFunctionMapEntry);
            var functionMapConsumer = functionMapConsumerMock.Object;

            var sourceMapStoreMock = new Mock<ISourceMapStore>();
            sourceMapStoreMock
                .Setup(c => c.GetSourceMapForUrl(It.IsAny<string>()))
                .Returns(new SourceMap());
            var sourceMapStore = sourceMapStoreMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore, functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.SourceMapFailedToParse, stackFrameDeminification.DeminificationError);
            Assert.AreEqual(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

        [TestMethod]
        public void StackFrameDeminierDeminifyStackFrame_SourceMapGeneratedMappingEntryNull_NoMatchingMapingInSourceMapError()
        {
            // Arrange
            string filePath = "foo";
            FunctionMapEntry wrapingFunctionMapEntry = new FunctionMapEntry { DeminfifiedMethodName = "DeminifiedFoo" };
            StackFrame stackFrame = new StackFrame { FilePath = filePath };

            var functionMapStoreMock = new Mock<IFunctionMapStore>();
            functionMapStoreMock
                .Setup(c => c.GetFunctionMapForSourceCode(filePath))
                .Returns(new List<FunctionMapEntry>());
            var functionMapStore = functionMapStoreMock.Object;

            SourceMap sourceMap = new SourceMap() { ParsedMappings = new List<MappingEntry>() };

            var sourceMapStoreMock = new Mock<ISourceMapStore>();
            sourceMapStoreMock
                .Setup(c => c.GetSourceMapForUrl(It.IsAny<string>()))
                .Returns(sourceMap);
            var sourceMapStore = sourceMapStoreMock.Object;

            var functionMapConsumerMock = new Mock<IFunctionMapConsumer>();
            functionMapConsumerMock
                .Setup(c => c.GetWrappingFunctionForSourceLocation(It.IsAny<SourcePosition>(), It.IsAny<List<FunctionMapEntry>>()))
                .Returns(wrapingFunctionMapEntry);
            var functionMapConsumer = functionMapConsumerMock.Object;

            IStackFrameDeminifier stackFrameDeminifier = GetStackFrameDeminifierWithMockDependencies(sourceMapStore: sourceMapStore, functionMapStore: functionMapStore, functionMapConsumer: functionMapConsumer);

            // Act
            StackFrameDeminificationResult stackFrameDeminification = stackFrameDeminifier.DeminifyStackFrame(stackFrame);

            // Assert
            Assert.AreEqual(DeminificationError.NoMatchingMapingInSourceMap, stackFrameDeminification.DeminificationError);
            Assert.AreEqual(wrapingFunctionMapEntry.DeminfifiedMethodName, stackFrameDeminification.DeminifiedStackFrame.MethodName);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.SourcePosition);
            Assert.IsNull(stackFrameDeminification.DeminifiedStackFrame.FilePath);
        }

    }
}
