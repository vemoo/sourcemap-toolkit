using System;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
    class MyStackFrameDeminifier : IStackFrameDeminifier
    {
        private readonly ISourceMapStore _sourceMapStore;
        public MyStackFrameDeminifier(SourceMapStore sourceMapStore)
        {
            _sourceMapStore = sourceMapStore;
        }

        public StackFrameDeminificationResult DeminifyStackFrame(StackFrame stackFrame)
        {
            var sourceMap = _sourceMapStore.GetSourceMapForUrl(stackFrame.FilePath);

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
                                MethodName = m.OriginalName
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