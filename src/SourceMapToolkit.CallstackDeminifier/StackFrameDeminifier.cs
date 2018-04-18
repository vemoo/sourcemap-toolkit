using System;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
    internal class StackFrameDeminifier : IStackFrameDeminifier
    {
        private readonly ISourceMapStore _sourceMapStore;
        
        public StackFrameDeminifier(ISourceMapStore sourceMapStore)
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

                        var methodName = noNames
                            ? stackFrame.MethodName
                            : m.OriginalName;
                        
                        return StackFrameDeminificationResult.Ok(new StackFrame(methodName, m.OriginalFileName,
                            m.OriginalSourcePosition));;
                    }
                }
            }

            return  StackFrameDeminificationResult.Error(DeminificationError.NoMatchingMapingInSourceMap, stackFrame);
        }
    }
}