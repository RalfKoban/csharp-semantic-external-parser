using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using MiKoSolutions.SemanticParsers.CSharp.Yaml;

using Container = MiKoSolutions.SemanticParsers.CSharp.Yaml.Container;
using File = MiKoSolutions.SemanticParsers.CSharp.Yaml.File;
using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public sealed class Parser
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        private static readonly Dictionary<Type, string> TypeMapping = new Dictionary<Type, string>()
        {
            { typeof(UsingDirectiveSyntax), "using" },
            { typeof(NamespaceDeclarationSyntax), "namespace" },
            { typeof(ClassDeclarationSyntax), "class" },
            { typeof(StructDeclarationSyntax), "struct" },
            { typeof(InterfaceDeclarationSyntax), "interface" },
            { typeof(EnumDeclarationSyntax), "enum" },
            { typeof(ConstructorDeclarationSyntax), "constructor" },
            { typeof(MethodDeclarationSyntax), "method" },
            { typeof(IndexerDeclarationSyntax), "indexer" },
            { typeof(PropertyDeclarationSyntax), "property" },
            { typeof(EventDeclarationSyntax), "event" },
            { typeof(FieldDeclarationSyntax), "field" },
            { typeof(AttributeListSyntax), "attribute" },
        };

        // we have issues with UTF-8 encodings in files that should have an encoding='iso-8859-1'
        public static File Parse(string filePath, string encoding)
        {
            var encodingToUse = Encoding.GetEncoding(encoding);
            var source = SystemFile.ReadAllText(filePath, encodingToUse);

            return ParseCore(source, filePath);
        }

        public static File ParseCore(string source, string filePath = null)
        {
            var document = CreateDocument(source);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;

            var rootNode = syntaxTree.GetRoot();
            var root = new Container
            {
                Name = "Compilation unit",
                HeaderSpan = CharacterSpan.None,
                FooterSpan = CharacterSpan.None,
                LocationSpan = new LocationSpan(rootNode)
            };

            AddChildren(root, rootNode);

            var file = new File
            {
                Name = filePath,
                LocationSpan = root.LocationSpan,
                FooterSpan = CharacterSpan.None, // there is no footer
            };

            file.Children.Add(root);

            return file;
        }

        private static Node ParseNode(SyntaxNode node)
        {
            switch (node)
            {
                case BaseNamespaceDeclarationSyntax _:
                case BaseTypeDeclarationSyntax _:
                    return Container(node);

                case UsingDirectiveSyntax _:
                case MemberDeclarationSyntax _:
                    return TerminalNode(node);

                default:
                    return null;
            }
        }

        private static Container Container(SyntaxNode syntax)
        {
            var container = new Container
            {
                Type = GetType(syntax),
                Name = GetName(syntax),
                LocationSpan = new LocationSpan(syntax),
                HeaderSpan = new CharacterSpan(), // TODO RKN
                FooterSpan = new CharacterSpan(), //syntax.GetTrailingTrivia() // TODO RKN,
            };

            AddChildren(container, syntax);
            return container;
        }

        private static TerminalNode TerminalNode(SyntaxNode syntax)
        {
            return new TerminalNode
            {
                Type = GetType(syntax),
                Name = GetName(syntax),
                LocationSpan = new LocationSpan(syntax),
                Span = new CharacterSpan(syntax),
            };
        }

        private static string GetName(SyntaxNode syntax)
        {
            switch (syntax)
            {
                case UsingDirectiveSyntax u: return u.Name.ToString();
                case BaseNamespaceDeclarationSyntax ns: return ns.Name.ToString();
                case BaseTypeDeclarationSyntax t: return t.Identifier.ValueText;
                case ConstructorDeclarationSyntax c: return c.Identifier.ValueText;
                case MethodDeclarationSyntax m: return m.Identifier.ValueText;
                case IndexerDeclarationSyntax i: return i.ThisKeyword.ValueText;
                case PropertyDeclarationSyntax p: return p.Identifier.ValueText;
                case EventDeclarationSyntax e: return e.Identifier.ValueText;
                case FieldDeclarationSyntax f: return f.Declaration.Variables.First().Identifier.ValueText;
                case AttributeListSyntax a: return a.Attributes.First().Name.ToString();

                default:
                    return syntax.ChildNodes().First().ToString();
            }
        }

        private static string GetType(SyntaxNode syntax)
        {
            var type = syntax.GetType();
            return TypeMapping.TryGetValue(type, out var result)
                ? result
                : type.Name;
        }

        private static void AddChildren(Container container, SyntaxNode syntax)
        {
            container.Children.AddRange(syntax.ChildNodes().Select(ParseNode).Where(_ => _ != null));
        }

        /// <summary>
        /// Creates a Document from a string through creating a project that contains it.
        /// </summary>
        /// <param name="source">Classes in the form of a string.</param>
        /// <param name="language">The language the source code is in.</param>
        /// <returns>A Document created from the source string.</returns>
        private static Document CreateDocument(string source)
        {
            var projectName = typeof(Parser).Namespace + ".AdHoc.Project";
            var projectId = ProjectId.CreateNewId(debugName: projectName);

            var solution = new AdhocWorkspace().CurrentSolution
                .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                .AddMetadataReference(projectId, CorlibReference);

            const string newFileName = "Parse.cs";
            var documentId = DocumentId.CreateNewId(projectId, debugName: newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));

            return solution.GetProject(projectId).Documents.First();
        }
    }
}