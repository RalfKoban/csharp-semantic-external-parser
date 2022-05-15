using System.IO;
using System.Linq;
using System.Text;

using MiKoSolutions.SemanticParsers.CSharp.Yaml;

using NUnit.Framework;

using File = MiKoSolutions.SemanticParsers.CSharp.Yaml.File;
using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public class ParserTests
    {
        [Test]
        public void Attribute_gets_parsed()
        {
            var file = Parser.ParseCore(@"
using System;

[assembly: AssemblyVersion(""10.0.102.0"")]

");
            var message = ToYaml(file);

            Assert.That(file.Children, Has.Count.EqualTo(2), message);

            Assert.That(file.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(file.Children[1].Type, Is.EqualTo(TypeNames.AttributeList), message);
        }

        [Test]
        public void Namespace_declaration_gets_parsed()
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
");
            var message = ToYaml(file);

            Assert.That(file.Children, Has.Count.EqualTo(4), message);

            Assert.That(file.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(file.Children[1].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(file.Children[2].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)file.Children[3];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.NamespaceDeclaration), message);

            Assert.That(namespaceNode.Children, Has.Count.EqualTo(1), message);

            var classDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(classDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);

            Assert.That(classDeclaration.Children, Has.Count.EqualTo(2), message);
            Assert.That(classDeclaration.Children[0].Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
            Assert.That(classDeclaration.Children[1].Type, Is.EqualTo(TypeNames.FieldDeclaration), message);
        }

        [TestCase( true, 2, 1,  2, 15,   2,  16,  -1,  -1, TypeNames.UsingDirective, "System")]
        [TestCase( true, 3, 1,  3, 27,  17,  43,  -1,  -1, TypeNames.UsingDirective, "System.Collections")]
        [TestCase( true, 4, 1,  4, 35,  44,  78,  -1,  -1, TypeNames.UsingDirective, "System.Collections.Generic")]
        [TestCase(false, 5, 1, 16,  3,  79, 114,   0,  -1, TypeNames.FileScopedNamespaceDeclaration, "My.Namespace.For.Test")]
        [TestCase(false, 7, 1, 16,  3, 115, 141, 300, 302, TypeNames.ClassDeclaration, "MyClass")]
        [TestCase(true, 10, 1, 13,  7, 142, 219,  -1,  -1, TypeNames.MethodDeclaration, "DoSomething")]
        [TestCase(true, 14, 1, 15, 78, 220, 299,  -1,  -1, TypeNames.FieldDeclaration, "m_field")]
        public void File_scoped_namespace_declaration_gets_parsed(bool terminalNode, int startLine, int startPos, int endLine, int endPos, int headerStartPos, int headerEndPos, int footerStartPos, int footerEndPos, string typeName, string name)
        {
            var file = Parser.ParseCore(@"
using System;
using System.Collections;
using System.Collections.Generic;

namespace My.Namespace.For.Test;

public class MyClass
{
    void DoSomething(int parameter)
    {
        // does something
    }

    private Dictionary<int, string> m_field = new Dictionary<int, string>();
}
");

            var message = ToYaml(file);

            Assert.That(file.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(1, 0), new LineInfo(17, 0))), message);
            Assert.That(file.FooterSpan, Is.EqualTo(CharacterSpan.None), message);

            Assert.That(file.Children, Has.Count.EqualTo(4), message);

            if (terminalNode)
            {
                var node = file.Descendants().OfType<TerminalNode>().First(_ => _.Name == name);

                Assert.That(node.Type, Is.EqualTo(typeName), message);
                Assert.That(node.Name, Is.EqualTo(name), message);
                Assert.That(node.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(startLine, startPos), new LineInfo(endLine, endPos))), message);
                Assert.That(node.Span, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), message);
            }
            else
            {
                var node = file.Descendants().OfType<Container>().First(_ => _.Name == name);

                Assert.That(node.Type, Is.EqualTo(typeName), message);
                Assert.That(node.Name, Is.EqualTo(name), message);
                Assert.That(node.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(startLine, startPos), new LineInfo(endLine, endPos))), message);
                Assert.That(node.HeaderSpan, Is.EqualTo(new CharacterSpan(headerStartPos, headerEndPos)), message);
                Assert.That(node.FooterSpan, Is.EqualTo(new CharacterSpan(footerStartPos, footerEndPos)), message);
            }
        }

        [Test]
        public void File_scoped_namespace_declaration_gets_parsed()
        {
            var file = Parser.ParseCore(@"
using System;
using System.Collections;
using System.Collections.Generic;

namespace My.Namespace.For.Test;

public class MyClass
{
    void DoSomething(int parameter)
    {
        // does something
    }

    private Dictionary<int, string> m_field = new Dictionary<int, string>();
}
");

            var message = ToYaml(file);

            Assert.That(file.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(1, 0), new LineInfo(17, 0))), message);
            Assert.That(file.FooterSpan, Is.EqualTo(CharacterSpan.None), message);

            Assert.That(file.Children, Has.Count.EqualTo(4), message);

            var namespaceNode = (Container)file.Children[3];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.FileScopedNamespaceDeclaration), message);

            var classDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(classDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);

            Assert.That(classDeclaration.Children, Has.Count.EqualTo(2), message);
            Assert.That(classDeclaration.Children[0].Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
            Assert.That(classDeclaration.Children[1].Type, Is.EqualTo(TypeNames.FieldDeclaration), message);
        }

        [Test]
        public void Enum_gets_parsed()
        {
            var file = Parser.ParseCore(@"
using System;
using System.ComponentModel;

namespace My.Namespace.For.Test
{
    public enum SomeEnum
    {
        [Description(""the default value"")]
        None = 0,
        Something,
        Anything,
    }
}");

            var message = ToYaml(file);

            Assert.That(file.Children, Has.Count.EqualTo(3), message);

            Assert.That(file.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(file.Children[1].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)file.Children[2];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.NamespaceDeclaration), message);
            Assert.That(namespaceNode.Children, Has.Count.EqualTo(1), message);

            var enumDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(enumDeclaration.Type, Is.EqualTo(TypeNames.EnumDeclaration), message);

            Assert.That(enumDeclaration.Children, Has.Count.EqualTo(3), message);
            Assert.That(enumDeclaration.Children[0].Type, Is.EqualTo(TypeNames.EnumMemberDeclaration), message);
            Assert.That(enumDeclaration.Children[1].Type, Is.EqualTo(TypeNames.EnumMemberDeclaration), message);
            Assert.That(enumDeclaration.Children[2].Type, Is.EqualTo(TypeNames.EnumMemberDeclaration), message);
        }

        [Test]
        public void Struct_gets_parsed()
        {
            var file = Parser.ParseCore(@"
using System;

namespace My.Namespace.For.Test
{
    public struct MyStruct
    {
        public MyStruct(int field) => myField = field;

        public int MyProperty { get; } => myField;

        private int myField;
    }
}");

            var message = ToYaml(file);

            Assert.That(file.Children, Has.Count.EqualTo(2), message);

            Assert.That(file.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)file.Children[1];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.NamespaceDeclaration), message);
            Assert.That(namespaceNode.Children, Has.Count.EqualTo(1), message);

            var structDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(structDeclaration.Type, Is.EqualTo(TypeNames.StructDeclaration), message);

            Assert.That(structDeclaration.Children, Has.Count.EqualTo(3), message);
            Assert.That(structDeclaration.Children[0].Type, Is.EqualTo(TypeNames.ConstructorDeclaration), message);
            Assert.That(structDeclaration.Children[1].Type, Is.EqualTo(TypeNames.PropertyDeclaration), message);
            Assert.That(structDeclaration.Children[2].Type, Is.EqualTo(TypeNames.FieldDeclaration), message);
        }

        [Test]
        public void Nested_class_gets_parsed()
        {
            var file = Parser.ParseCore(@"
using System;

namespace My.Namespace.For.Test
{
    public class MyClass
    {
        void DoSomething(int parameter)
        {
            // does something
        }

        private class Nested
        {
            public void DoAnything()
            {
            }

            public event EventHandler MyEvent1;

            public event EventHandler MyEvent2
            {
                add  { }
                remove  { }
            }

            public this[string key] => null;

            public int Count { get; set; }
        }
    }
}");

            var message = ToYaml(file);

            Assert.That(file.Children, Has.Count.EqualTo(2), message);
            Assert.That(file.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)file.Children[1];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.NamespaceDeclaration), message);

            var classDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(classDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);

            Assert.That(classDeclaration.Children, Has.Count.EqualTo(2), message);
            Assert.That(classDeclaration.Children[0].Type, Is.EqualTo(TypeNames.MethodDeclaration), message);

            var nestedClassDeclaration = (Container)classDeclaration.Children[1];

            Assert.That(nestedClassDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);
            Assert.That(nestedClassDeclaration.Children, Has.Count.EqualTo(5), message);
            Assert.That(nestedClassDeclaration.Children[0].Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
            Assert.That(nestedClassDeclaration.Children[1].Type, Is.EqualTo(TypeNames.EventFieldDeclaration), message);
            Assert.That(nestedClassDeclaration.Children[2].Type, Is.EqualTo(TypeNames.EventDeclaration), message);
            Assert.That(nestedClassDeclaration.Children[3].Type, Is.EqualTo(TypeNames.IndexerDeclaration), message);
            Assert.That(nestedClassDeclaration.Children[4].Type, Is.EqualTo(TypeNames.PropertyDeclaration), message);
        }

        [Test]
        public void Header_footer_gets_parsed()
        {
            var fileName = Path.GetTempFileName();

            SystemFile.WriteAllText(fileName, @"class Socket
{
   void Connect(string server)
   {
      SocketLibrary.Connect(mSocket, server);
   }

   void Disconnect()
   {
      SocketLibrary.Disconnect(mSocket);
   }
}");

            var file = Parser.Parse(fileName, "UTF-8");

            var message = ToYaml(file);

            /*
---
type: file
name: xxx
locationSpan : {start: [1, 0], end: [12, 1]}
footerSpan : [0,-1]
parsingErrorsDetected : false
parsingError: []
children:

  - type : class
    name : Socket
    locationSpan : {start: [1, 1, end: [12, 1]}
    headerSpan : [0, 16]
    footerSpan : [186, 186]
    children :

    - type : method
      name : Connect
      locationSpan : {start: [3, 1], end: [6,6]}
      span : [17, 107]

    - type : method
      name : Disconnect
      locationSpan : {start: [7,1], end: [11,6]}
      span : [108, 185]
             */

            try
            {
                Assert.Multiple(() =>
                {
                    Assert.That(file.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(1, 0), new LineInfo(12, 1))), message);
                    Assert.That(file.FooterSpan, Is.EqualTo(CharacterSpan.None), message);

                    Assert.That(file.Children, Has.Count.EqualTo(1), message);

                    var classDeclaration = (Container)file.Children[0];

                    Assert.That(classDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);
                    Assert.That(classDeclaration.Name, Is.EqualTo("Socket"), message);
                    Assert.That(classDeclaration.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(1, 1), new LineInfo(12, 1))), message);
                    Assert.That(classDeclaration.HeaderSpan, Is.EqualTo(new CharacterSpan(0, 16)), message);
                    Assert.That(classDeclaration.FooterSpan, Is.EqualTo(new CharacterSpan(186, 186)), message);

                    Assert.That(classDeclaration.Children, Has.Count.EqualTo(2), message);

                    var connectMethod = (TerminalNode)classDeclaration.Children[0];

                    Assert.That(connectMethod.Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
                    Assert.That(connectMethod.Name, Is.EqualTo("Connect"), message);
                    Assert.That(connectMethod.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(3, 1), new LineInfo(6, 6))), message);
                    Assert.That(connectMethod.Span, Is.EqualTo(new CharacterSpan(17, 107)), message);

                    var disconnectMethod = (TerminalNode)classDeclaration.Children[1];

                    Assert.That(disconnectMethod.Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
                    Assert.That(disconnectMethod.Name, Is.EqualTo("Disconnect"), message);
                    Assert.That(disconnectMethod.LocationSpan, Is.EqualTo(new LocationSpan(new LineInfo(7, 1), new LineInfo(11, 6))), message);
                    Assert.That(disconnectMethod.Span, Is.EqualTo(new CharacterSpan(108, 185)), message);
                });
            }
            finally
            {
                SystemFile.Delete(fileName);
            }
        }

        private static string ToYaml(File file)
        {
            var sb = new StringBuilder();
            
            using (var writer = new StringWriter(sb))
            {
                YamlWriter.Write(writer, file);
            }

            return sb.ToString();
        }
    }
}