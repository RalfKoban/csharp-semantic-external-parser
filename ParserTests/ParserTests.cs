using System.IO;
using System.Text;
using MiKoSolutions.SemanticParsers.CSharp.Yaml;
using NUnit.Framework;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public class ParserTests
    {
        private Parser ObjectUnderTest { get; set; }

        [SetUp]
        public void Setup()
        {
            ObjectUnderTest = new Parser();
        }

        [Test]
        public void Test1()
        {
            var file = Parser.ParseCore(@"
using System;
using System.Collections;
using System.Collections.Generic;

namespace My.Namespace.For.Test
{
    public class MyClass
    {
        void DoSomething(int parameter)
        {
            // does something
        }

        private Dictionary<int, string> m_field = new Dictionary<int, string>();
    }
}

[assembly: AssemblyVersion(""10.0.102.0"")]

");
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                YamlWriter.Write(writer, file);
            }

            var message = sb.ToString();

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];
            Assert.That(compilationUnit.Children, Has.Count.EqualTo(3), message);
        }
    }
}