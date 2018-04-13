using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SourcemapToolkit.SourcemapParser.UnitTests;

namespace SourcemapToolkit.CallstackDeminifier.UnitTests
{
    [TestClass]
    public class StackTraceDeminifierEndToEndTests
    {
        private const string SourceMapString = "{\"version\":3,\"sources\":[\"webpack:///webpack/bootstrap\",\"webpack:///./src/index.ts\"],\"names\":[\"installedModules\",\"__webpack_require__\",\"moduleId\",\"exports\",\"module\",\"i\",\"l\",\"modules\",\"call\",\"m\",\"c\",\"d\",\"name\",\"getter\",\"o\",\"Object\",\"defineProperty\",\"configurable\",\"enumerable\",\"get\",\"r\",\"value\",\"n\",\"__esModule\",\"object\",\"property\",\"prototype\",\"hasOwnProperty\",\"p\",\"s\",\"els\",\"callstackdisplay\",\"document\",\"getElementById\",\"deminified\",\"crashbutton\",\"causeCrash\",\"input\",\"longLocalVariableName\",\"window\",\"onerror\",\"message\",\"source\",\"lineno\",\"colno\",\"error\",\"stack\",\"event\",\"innerText\",\"fetch\",\"encodeURIComponent\",\"method\",\"then\",\"res\",\"text\",\"txt\",\"deminify\",\"console\",\"log\",\"x\",\"length\",\"level3\",\"onload\",\"addEventListener\"],\"mappings\":\"aACA,IAAAA,KAGA,SAAAC,EAAAC,GAGA,GAAAF,EAAAE,GACA,OAAAF,EAAAE,GAAAC,QAGA,IAAAC,EAAAJ,EAAAE,IACAG,EAAAH,EACAI,GAAA,EACAH,YAUA,OANAI,EAAAL,GAAAM,KAAAJ,EAAAD,QAAAC,IAAAD,QAAAF,GAGAG,EAAAE,GAAA,EAGAF,EAAAD,QAKAF,EAAAQ,EAAAF,EAGAN,EAAAS,EAAAV,EAGAC,EAAAU,EAAA,SAAAR,EAAAS,EAAAC,GACAZ,EAAAa,EAAAX,EAAAS,IACAG,OAAAC,eAAAb,EAAAS,GACAK,cAAA,EACAC,YAAA,EACAC,IAAAN,KAMAZ,EAAAmB,EAAA,SAAAjB,GACAY,OAAAC,eAAAb,EAAA,cAAiDkB,OAAA,KAIjDpB,EAAAqB,EAAA,SAAAlB,GACA,IAAAS,EAAAT,KAAAmB,WACA,WAA2B,OAAAnB,EAAA,SAC3B,WAAiC,OAAAA,GAEjC,OADAH,EAAAU,EAAAE,EAAA,IAAAA,GACAA,GAIAZ,EAAAa,EAAA,SAAAU,EAAAC,GAAsD,OAAAV,OAAAW,UAAAC,eAAAnB,KAAAgB,EAAAC,IAGtDxB,EAAA2B,EAAA,GAIA3B,IAAA4B,EAAA,kCClEA,IAAMC,GACFC,uBACI,OAAOC,SAASC,eAAe,qBAEnCC,iBACI,OAAOF,SAASC,eAAe,eAEnCE,kBACI,OAAOH,SAASC,eAAe,iBAIvC,SAAAG,IACI,IAMgBC,EALRC,EAiBRC,OAAOC,QAAU,SAAUC,EAASC,EAAQC,EAAQC,EAAOC,GACvD,IAAIC,EACAD,EACAC,EAAQD,EAAMC,MACPP,OAAOQ,OAAUR,OAAOQ,MAAcF,QAC7CC,EAASP,OAAOQ,MAAcF,MAAMC,OAExChB,EAAIC,iBAAiBiB,UAAYF,EAOzC,SAAkBA,GACdhB,EAAII,WAAWc,UAAY,GAC3BC,MAAM,4BAA4BC,mBAAmBJ,IAAYK,OAAQ,QACpEC,KAAK,SAAAC,GAAO,OAAAA,EAAIC,SAChBF,KAAK,SAAAG,GACFzB,EAAII,WAAWc,UAAYO,IAX/BC,CAASV,IAzBLR,EAAwB,GAKhBD,EAJZC,GAAyB,EAS7B,SAAgBD,GAGRoB,QAAQC,UADJC,GACUC,OAASvB,GAN3BwB,CADAxB,GAAgB,GAkCxBE,OAAOuB,OAAS,SAAUf,GACtBjB,EAAIK,YAAY4B,iBAAiB,QAAS,WACtC3B\",\"file\":\"bundle.js\",\"sourceRoot\":\"\"}";

        private StackTraceDeminifier GetStackTraceDeminifierWithDependencies()
        {
            var sourceMapProviderMock = new Mock<ISourceMapProvider>();
            sourceMapProviderMock
                .Setup(x => x.GetSourceMapContentsForCallstackUrl("http://localhost:11323/crashcauser.min.js"))
                .Returns(UnitTestUtils.StreamReaderFromString(SourceMapString));
            var sourceMapProvider = sourceMapProviderMock.Object;

            return StackTraceDeminfierFactory.GetStackTraceDeminfier(sourceMapProvider);
        }

        private static void ValidateDeminifyStackTraceResults(DeminifyStackTraceResult results)
        {
            Assert.AreEqual(6, results.DeminifiedStackFrameResults.Count);
            Assert.AreEqual(DeminificationError.None, results.DeminifiedStackFrameResults[0].DeminificationError);
            Assert.AreEqual("level3", results.DeminifiedStackFrameResults[0].DeminifiedStackFrame.MethodName);
            Assert.AreEqual("level3", results.DeminifiedStackFrameResults[1].DeminifiedStackFrame.MethodName);
            Assert.AreEqual("level2", results.DeminifiedStackFrameResults[2].DeminifiedStackFrame.MethodName);
            Assert.AreEqual("level1", results.DeminifiedStackFrameResults[3].DeminifiedStackFrame.MethodName);
            Assert.AreEqual("causeCrash", results.DeminifiedStackFrameResults[4].DeminifiedStackFrame.MethodName);
            Assert.AreEqual("window", results.DeminifiedStackFrameResults[5].DeminifiedStackFrame.MethodName);
        }

        [TestMethod]
        public void DeminifyStackTrace_ChromeStackTraceString_CorrectDeminificationWhenPossible()
        {
            // Arrange
            StackTraceDeminifier stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
            string chromeStackTrace = @"TypeError: Cannot read property 'length' of undefined
    at http://localhost:11323/crashcauser.min.js:1:125
    at i (http://localhost:11323/crashcauser.min.js:1:137)
    at t (http://localhost:11323/crashcauser.min.js:1:75)
    at n (http://localhost:11323/crashcauser.min.js:1:50)
    at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
    at HTMLButtonElement.<anonymous> (http://localhost:11323/crashcauser.min.js:1:445)";

            // Act
            DeminifyStackTraceResult results = stackTraceDeminifier.DeminifyStackTrace(chromeStackTrace);

            // Assert
            ValidateDeminifyStackTraceResults(results);
        }

        [TestMethod]
        public void DeminifyStackTrace_FireFoxStackTraceString_CorrectDeminificationWhenPossible()
        {
            // Arrange
            StackTraceDeminifier stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
            string fireFoxStackTrace = @"i/<@http://localhost:11323/crashcauser.min.js:1:112
i@http://localhost:11323/crashcauser.min.js:1:95
t@http://localhost:11323/crashcauser.min.js:1:75
n@http://localhost:11323/crashcauser.min.js:1:50
causeCrash@http://localhost:11323/crashcauser.min.js:1:341
window.onload/<@http://localhost:11323/crashcauser.min.js:1:445";

            // Act
            DeminifyStackTraceResult results = stackTraceDeminifier.DeminifyStackTrace(fireFoxStackTrace);

            // Assert
            ValidateDeminifyStackTraceResults(results);
        }

        [TestMethod]
        public void DeminifyStackTrace_IE11StackTraceString_CorrectDeminificationWhenPossible()
        {
            // Arrange
            StackTraceDeminifier stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
            string ieStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:112)
   at i (http://localhost:11323/crashcauser.min.js:1:95)
   at t (http://localhost:11323/crashcauser.min.js:1:75)
   at n (http://localhost:11323/crashcauser.min.js:1:50)
   at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:445)";

            // Act
            DeminifyStackTraceResult results = stackTraceDeminifier.DeminifyStackTrace(ieStackTrace);

            // Assert
            ValidateDeminifyStackTraceResults(results);
        }

        [TestMethod]
        public void DeminifyStackTrace_EdgeStackTraceString_CorrectDeminificationWhenPossible()
        {
            // Arrange
            StackTraceDeminifier stackTraceDeminifier = GetStackTraceDeminifierWithDependencies();
            string dgeStackTrace = @"TypeError: Unable to get property 'length' of undefined or null reference
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:112)
   at i (http://localhost:11323/crashcauser.min.js:1:95)
   at t (http://localhost:11323/crashcauser.min.js:1:75)
   at n (http://localhost:11323/crashcauser.min.js:1:50)
   at causeCrash (http://localhost:11323/crashcauser.min.js:1:341)
   at Anonymous function (http://localhost:11323/crashcauser.min.js:1:445)";

            // Act
            DeminifyStackTraceResult results = stackTraceDeminifier.DeminifyStackTrace(dgeStackTrace);

            // Assert
            ValidateDeminifyStackTraceResults(results);
        }
    }
}