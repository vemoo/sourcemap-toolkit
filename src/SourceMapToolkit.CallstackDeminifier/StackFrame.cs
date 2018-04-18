using SourcemapToolkit.SourcemapParser;

namespace SourcemapToolkit.CallstackDeminifier
{
    /// <summary>
    /// Represents a single entry in a JavaScript stack frame. 
    /// </summary>
    public class StackFrame
    {
        /// <summary>
        /// The name of the method
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// The path of the file where this code is defined
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// The zero-based position of this stack entry.
        /// </summary>
        public SourcePosition SourcePosition { get; }

        public StackFrame(string methodName, string filePath, SourcePosition sourcePosition)
        {
            MethodName = methodName;
            FilePath = filePath;
            SourcePosition = sourcePosition;
        }
    }
}