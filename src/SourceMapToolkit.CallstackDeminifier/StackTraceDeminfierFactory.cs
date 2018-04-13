using System;
using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
    public class StackTraceDeminfierFactory
    {
        public static StackTraceDeminifier GetStackTraceDeminfier(ISourceMapProvider sourceMapProvider)
        {
            var sourceMapStore = new SourceMapStore(sourceMapProvider);
            var stackFrameDeminifier = new StackFrameDeminifier(sourceMapStore);

            var stackTraceParser = new StackTraceParser();

            return new StackTraceDeminifier(stackFrameDeminifier, stackTraceParser);
        }
    }
}