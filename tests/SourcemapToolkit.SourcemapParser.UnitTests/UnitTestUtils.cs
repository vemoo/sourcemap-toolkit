﻿using System.IO;
using System.Text;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
    public static class UnitTestUtils
    {
        public static StreamReader StreamReaderFromString(string streamContents)
        {
            var byteArray = Encoding.UTF8.GetBytes(streamContents);
            return new StreamReader(new MemoryStream(byteArray));
        }

        public static MappingEntry GetSimpleEntry(SourcePosition generatedSourcePosition,
            SourcePosition originalSourcePosition, string originalFileName)
        {
            return new MappingEntry
            {
                GeneratedSourcePosition = generatedSourcePosition,
                OriginalSourcePosition = originalSourcePosition,
                OriginalFileName = originalFileName
            };
        }

        public static SourcePosition GenerateSourcePosition(int lineNumber, int colNumber = 0)
        {
            return new SourcePosition(lineNumber, colNumber);
        }
    }
}