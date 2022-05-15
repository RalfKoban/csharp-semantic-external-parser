using System.IO;
using System.Text;

using MiKoSolutions.SemanticParsers.CSharp.Yaml;

using NUnit.Framework;
using File = MiKoSolutions.SemanticParsers.CSharp.Yaml.File;

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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(2), message);

            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[1].Type, Is.EqualTo(TypeNames.AttributeList), message);
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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(4), message);

            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[1].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[2].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)compilationUnit.Children[3];

            Assert.That(namespaceNode.Type, Is.EqualTo(TypeNames.NamespaceDeclaration), message);

            Assert.That(namespaceNode.Children, Has.Count.EqualTo(1), message);

            var classDeclaration = (Container)namespaceNode.Children[0];

            Assert.That(classDeclaration.Type, Is.EqualTo(TypeNames.ClassDeclaration), message);

            Assert.That(classDeclaration.Children, Has.Count.EqualTo(2), message);
            Assert.That(classDeclaration.Children[0].Type, Is.EqualTo(TypeNames.MethodDeclaration), message);
            Assert.That(classDeclaration.Children[1].Type, Is.EqualTo(TypeNames.FieldDeclaration), message);
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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(4), message);

            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[1].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[2].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)compilationUnit.Children[3];

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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(3), message);

            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);
            Assert.That(compilationUnit.Children[1].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)compilationUnit.Children[2];

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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(2), message);

            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)compilationUnit.Children[1];

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

            Assert.That(file.Children, Has.Count.EqualTo(1), message);

            var compilationUnit = file.Children[0];

            Assert.That(compilationUnit.Children, Has.Count.EqualTo(2), message);
            Assert.That(compilationUnit.Children[0].Type, Is.EqualTo(TypeNames.UsingDirective), message);

            var namespaceNode = (Container)compilationUnit.Children[1];

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