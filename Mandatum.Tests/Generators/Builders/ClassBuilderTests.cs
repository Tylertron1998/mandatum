using System;
using Mandatum.Generators.Utilities.Builders;
using NUnit.Framework;

namespace Mandatum.Tests.Generators.Builders
{
	[TestFixture]
	public class ClassBuilderTests
	{
		[Test]
		public void Assert_ClassBuilder_DisplaysCorrectly()
		{
			// assign
			var builder = new ClassBuilder("TestClass", "Test.NamespaceTest", Accessibility.Public, new []{ "Import.Test", "Import.TestTwo" });
			
			// act
			builder.AddProperty("TestProperty", "string", Accessibility.Public, GetterType.Public, SetterType.Private);
			builder.AddField("_testField", "int", Accessibility.Private);

			var intMethodBuilder = builder.GetBuilder();
			intMethodBuilder.AppendLine("var x = 1;");
			intMethodBuilder.AppendLine("var y = 3;");
			intMethodBuilder.AppendLine("return x + y;");

			builder.AppendLine();
			builder.AddMethod("TestMethod", "int", Accessibility.Public, intMethodBuilder);
			
			builder.AppendLine();
			
			var voidMethodBuilder = builder.GetBuilder();
			voidMethodBuilder.AppendLine("Console.WriteLine(\"Foo\" + str);");
			builder.AddMethod("VoidMethod", "void", Accessibility.Internal, voidMethodBuilder, new []{ "string str" });
			
			
			var expectedString = @"using Import.Test;
using Import.TestTwo;

namespace Test.NamespaceTest
{
	public class TestClass
	{
		public string TestProperty { get; private set; }
		private int _testField;

		public int TestMethod()
		{
			var x = 1;
			var y = 3;
			return x + y;
		}

		internal void VoidMethod(string str)
		{
			Console.WriteLine(""Foo"" + str);
		}
	}
}
";

			var actualString = builder.ToString();
			
			//assert
			
			Assert.AreEqual(expectedString, actualString);
		}
	}
}