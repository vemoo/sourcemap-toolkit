using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SourcemapToolkit.SourcemapParser.UnitTests
{
	[TestClass]
	public class Base64ConverterUnitTests
	{
		[DataTestMethod]
		[DataRow('C', 2)]
		[DataRow('9', 61)]
		public void FromBase64_ValidBase64Input(char c, int expected)
		{
			// Act
			var value = Base64Converter.FromBase64(c);
			// Assert
			Assert.AreEqual(expected, value);
		}
		
		[DataTestMethod]
		[DataRow(2, 'C')]
		[DataRow(61,'9')]
		public void ToBase64_ValidIntegerInput(int i, char expected)
		{
			// Act
			var value = Base64Converter.ToBase64(i);
			// Assert
			Assert.AreEqual(expected, value);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void FromBase64_InvalidBase64Input_ThrowsException()
		{
			// Act
			Base64Converter.FromBase64('@');
		}
		

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ToBase64_NegativeIntegerInput_ThrowsException()
		{
			// Act
			Base64Converter.ToBase64(-1);
		}

		[TestMethod]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void ToBase64_InvalidIntegerInput_ThrowsException()
		{
			// Act
			Base64Converter.ToBase64(64);
		}
	}
}
