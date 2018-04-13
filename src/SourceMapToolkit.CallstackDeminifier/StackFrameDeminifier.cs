using System;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
    class StackFrameDeminifier : IStackFrameDeminifier
    {
        private readonly ISourceMapStore _sourceMapStore;
        public StackFrameDeminifier(SourceMapStore sourceMapStore)
        {
            _sourceMapStore = sourceMapStore;
        }

        public StackFrameDeminificationResult DeminifyStackFrame(StackFrame stackFrame)
        {
            var sourceMap = _sourceMapStore.GetSourceMapForUrl(stackFrame.FilePath);

            var noNames = sourceMap.Names.Count == 0;

            for (var i = 0; i < sourceMap.ParsedMappings.Count; i++)
            {
                var m = sourceMap.ParsedMappings[i];
                if (m.GeneratedSourcePosition.ZeroBasedLineNumber == stackFrame.SourcePosition.ZeroBasedLineNumber)
                {
                    if (m.GeneratedSourcePosition.ZeroBasedColumnNumber >= stackFrame.SourcePosition.ZeroBasedColumnNumber)
                    {
                        if (m.GeneratedSourcePosition.ZeroBasedColumnNumber > stackFrame.SourcePosition.ZeroBasedColumnNumber && i > 0)
                        {
                            m = sourceMap.ParsedMappings[i - 1];
                        }

                        return new StackFrameDeminificationResult
                        {
                            DeminificationError = DeminificationError.None,
                            DeminifiedStackFrame = new StackFrame
                            {
                                SourcePosition = m.OriginalSourcePosition,
                                FilePath = m.OriginalFileName,
                                MethodName = noNames
                                    ? stackFrame.MethodName
                                    : m.OriginalName,
                            },
                        };
                    }
                }
            }

            return new StackFrameDeminificationResult
            {
                DeminificationError = DeminificationError.NoMatchingMapingInSourceMap,
                DeminifiedStackFrame = stackFrame,
            };
        }
    }
}