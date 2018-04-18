using System.Collections.Generic;

namespace SourcemapToolkit.CallstackDeminifier
{
	public class DeminifyStackTraceResult
	{
		public IReadOnlyList<StackFrame> MinifiedStackFrames { get; }
		public IReadOnlyList<StackFrameDeminificationResult> DeminifiedStackFrameResults { get; }
		
		public DeminifyStackTraceResult(IReadOnlyList<StackFrame> minifiedStackFrames, IReadOnlyList<StackFrameDeminificationResult> deminifiedStackFrameResults)
		{
			MinifiedStackFrames = minifiedStackFrames;
			DeminifiedStackFrameResults = deminifiedStackFrameResults;
		}
	}
}
