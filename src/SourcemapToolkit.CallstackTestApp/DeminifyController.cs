using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using SourcemapToolkit.CallstackDeminifier;

namespace SourcemapToolkit.CallstackTestApp
{
    public class DeminifyController : Controller
    {
        readonly IHostingEnvironment _hostingEnvironment;
        public DeminifyController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public string Deminify(string stack)
        {
            var dm = StackTraceDeminfierFactory.GetStackTraceDeminfier(new SourceMapProvider(_hostingEnvironment.WebRootFileProvider));
            var res = dm.DeminifyStackTrace(stack);

            return string.Join("\n",
                res.DeminifiedStackFrameResults
                    .Select(x => x.DeminifiedStackFrame)
                    .Select(x => $"{x.MethodName}@{x.FilePath}:{x.SourcePosition.ZeroBasedLineNumber + 1}:{x.SourcePosition.ZeroBasedColumnNumber + 1}"));
        }
    }

    class SourceMapProvider : ISourceMapProvider
    {
        readonly IFileProvider _fileProvider;

        public SourceMapProvider(IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        private static StreamReader TryGetSourceMap(string x, Func<string, Stream> func)
        {
            var noExt = Regex.Replace(x, @"\.js$", "", RegexOptions.IgnoreCase);
            var s = func(noExt + ".map");
            if (s == null)
            {
                s = func(noExt + ".js.map");
            }

            if (s != null)
            {
                return new StreamReader(s);
            }
            else
            {
                return null;
            }
        }


        public StreamReader GetSourceMapContentsForCallstackUrl(string correspondingCallStackFileUrl)
        {
            var url = new Uri(correspondingCallStackFileUrl);
            var fi = _fileProvider.GetFileInfo(url.LocalPath);
            if (fi.Exists)
            {
                return TryGetSourceMap(fi.PhysicalPath, sourceMapPath =>
                {
                    if (File.Exists(sourceMapPath))
                    {
                        return File.OpenRead(sourceMapPath);
                    }
                    else
                    {
                        return null;
                    }
                });
            }

            using (var client = new HttpClient())
            {
                return TryGetSourceMap(correspondingCallStackFileUrl, sourceMapUrl =>
                {
                    var res = client.GetAsync(sourceMapUrl).Result;
                    if (res.StatusCode == HttpStatusCode.NotFound)
                    {
                        return null;
                    }
                    else
                    {
                        var bytes = res.Content.ReadAsByteArrayAsync().Result;

                        return new MemoryStream(bytes);
                    }
                });
            }
        }
    }
}