using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SourcemapToolkit.CallstackDeminifier;

namespace SourcemapToolkit.CallstackTestApp
{
    public class DeminifyController : Controller
    {
        [HttpGet]
        public string Deminify(string stack)
        {
            var dm = StackTraceDeminfierFactory.GetMyStackTraceDeminfier(new SourceMapProvider());
            var res = dm.DeminifyStackTrace(stack);

            return string.Join("\n",
                res.DeminifiedStackFrameResults
                    .Select(x => x.DeminifiedStackFrame)
                    .Select(x => $"{x.MethodName}@{x.FilePath}:{x.SourcePosition.ZeroBasedLineNumber + 1}:{x.SourcePosition.ZeroBasedColumnNumber + 1}"));
        }
    }

    class SourceMapProvider : ISourceMapProvider
    {
        public StreamReader GetSourceMapContentsForCallstackUrl(string correspondingCallStackFileUrl)
        {
            return new StreamReader(File.OpenRead("/home/berni/Documentos/src/sourcemap-toolkit/src/SourcemapToolkit.CallstackTestApp/wwwroot/dist/bundle.js.map"));
        }
    }
}