using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using MiKoSolutions.SemanticParsers.CSharp.Yaml;

using SystemFile = System.IO.File;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public sealed class Parser
    {
        private static readonly MetadataReference CorlibReference = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);

        private static readonly Dictionary<Type, string> TypeMapping = new Dictionary<Type, string>
                                                                           {
                                                                               { typeof(AttributeListSyntax), TypeNames.AttributeList },
                                                                               { typeof(ClassDeclarationSyntax), TypeNames.ClassDeclaration },
                                                                               { typeof(ConstructorDeclarationSyntax), TypeNames.ConstructorDeclaration },
                                                                               { typeof(EnumDeclarationSyntax), TypeNames.EnumDeclaration },
                                                                               { typeof(EnumMemberDeclarationSyntax), TypeNames.EnumMemberDeclaration },
                                                                               { typeof(EventDeclarationSyntax), TypeNames.EventDeclaration },
                                                                               { typeof(EventFieldDeclarationSyntax), TypeNames.EventFieldDeclaration },
                                                                               { typeof(FieldDeclarationSyntax), TypeNames.FieldDeclaration },
                                                                               { typeof(FileScopedNamespaceDeclarationSyntax), TypeNames.FileScopedNamespaceDeclaration },
                                                                               { typeof(GlobalStatementSyntax), TypeNames.GlobalStatement },
                                                                               { typeof(IncompleteMemberSyntax), TypeNames.IncompleteMember },
                                                                               { typeof(IndexerDeclarationSyntax), TypeNames.IndexerDeclaration },
                                                                               { typeof(InterfaceDeclarationSyntax), TypeNames.InterfaceDeclaration },
                                                                               { typeof(MethodDeclarationSyntax), TypeNames.MethodDeclaration },
                                                                               { typeof(NamespaceDeclarationSyntax), TypeNames.NamespaceDeclaration },
                                                                               { typeof(PropertyDeclarationSyntax), TypeNames.PropertyDeclaration },
                                                                               { typeof(RecordDeclarationSyntax), TypeNames.RecordDeclaration },
                                                                               { typeof(StructDeclarationSyntax), TypeNames.StructDeclaration },
                                                                               { typeof(UsingDirectiveSyntax), TypeNames.UsingDirective },
                                                                           };

        // we have issues with UTF-8 encodings in files that should have an encoding='iso-8859-1'
        public static File Parse(string filePath, string encoding)
        {
            var encodingToUse = Encoding.GetEncoding(encoding);
            var source = SystemFile.ReadAllText(filePath, encodingToUse);

            var file = ParseCore(source, filePath);

            // Fill gaps in between nodes so that each character inside the file is considered to be part of any container or terminal node
            GapFiller.Fill(file, CharacterPositionFinder.CreateFrom(filePath, encodingToUse));

            return file;
        }

        public static File ParseCore(string source, string filePath = null)
        {
            var document = CreateDocument(source);
            var syntaxTree = document.GetSyntaxTreeAsync().Result;

            var rootNode = syntaxTree.GetRoot();

            var file = new File
                           {
                               Name = filePath,
                               LocationSpan = GetLocationSpan(rootNode),
                               FooterSpan = GetFooterSpan(rootNode),
                           };

            AddChildren(file, rootNode);

            // determine whether we have parsing errors
            var parsingErrors = rootNode.DescendantNodes().OfType<IncompleteMemberSyntax>();
            foreach (var parsingError in parsingErrors)
            {
                file.ParsingErrors.Add(new ParsingError { Location = new LineInfo(parsingError.GetLocation().GetLineSpan().StartLinePosition), ErrorMessage = "Incomplete code" });
            }

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

                case AttributeListSyntax _:
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
                                    LocationSpan = GetLocationSpan(syntax),
                                    HeaderSpan = GetHeaderSpan(syntax),
                                    FooterSpan = GetFooterSpan(syntax),
                                };

            AddChildren(container, syntax);

            return container;
        }

        private static LocationSpan GetLocationSpan(SyntaxNode syntax)
        {
            switch (syntax)
            {
                case CompilationUnitSyntax unit:
                {
                    var start = new LineInfo(1, 0); // line span always starts at 1,0
                    var end = new LineInfo(unit.GetLocation().GetLineSpan().EndLinePosition);

                    return new LocationSpan(start, end);
                }

                default:
                    return new LocationSpan(syntax);
            }
        }


        private static CharacterSpan GetHeaderSpan(SyntaxNode syntax)
        {
            switch (syntax)
            {
                case BaseTypeDeclarationSyntax t: return new CharacterSpan(t.GetFirstToken(), t.OpenBraceToken);
                case NamespaceDeclarationSyntax ns: return new CharacterSpan(ns.GetFirstToken(), ns.OpenBraceToken);
                case FileScopedNamespaceDeclarationSyntax ns: return new CharacterSpan(ns.GetFirstToken(), ns.SemicolonToken);

                default:
                    return CharacterSpan.None;
            }
        }

        private static CharacterSpan GetFooterSpan(SyntaxNode syntax)
        {
            switch (syntax)
            {
                case BaseTypeDeclarationSyntax t: return new CharacterSpan(t.CloseBraceToken);
                case NamespaceDeclarationSyntax ns: return new CharacterSpan(ns.CloseBraceToken);
                case FileScopedNamespaceDeclarationSyntax ns:
                {
                    var last = ns.DescendantNodesAndTokensAndSelf().Last();
                    if (last.HasTrailingTrivia)
                    {
                        // seems we have some trivia left, so this would be out footer (as anything else belongs to the other nodes)
                        return new CharacterSpan(last.GetTrailingTrivia());
                    }

                    return CharacterSpan.None;
                }

                case CompilationUnitSyntax unit:
                {
                    // let's see if we have some whitespaces at the end of the file
                    if (unit.EndOfFileToken.HasLeadingTrivia)
                    {
                        return new CharacterSpan(unit.EndOfFileToken.LeadingTrivia);
                    }

                    return CharacterSpan.None;
                }

                default:
                    return CharacterSpan.None;
            }
        }
    
        private static TerminalNode TerminalNode(SyntaxNode syntax) => new TerminalNode
                                                                           {
                                                                               Type = GetType(syntax),
                                                                               Name = GetName(syntax),
                                                                               LocationSpan = GetLocationSpan(syntax),
                                                                               Span = new CharacterSpan(syntax),
                                                                           };

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
                case EnumMemberDeclarationSyntax em: return em.Identifier.ValueText;

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

        private static void AddChildren(File file, SyntaxNode syntax)
        {
            var children = syntax.ChildNodes().Select(ParseNode).Where(_ => _ != null);

            file.Children.AddRange(children);
        }

        private static void AddChildren(Container container, SyntaxNode syntax)
        {
            var children = syntax.ChildNodes().Select(ParseNode).Where(_ => _ != null);

            container.Children.AddRange(children);
        }

        /// <summary>Creates a Document from a string through creating a project that contains it.</summary>
        /// <param name="source">Classes in the form of a string.</param>
        /// <param name="language">The language the source code is in.</param>
        /// <returns>A Document created from the source string.</returns>
        private static Document CreateDocument(string source)
        {
            var projectName = typeof(Parser).Namespace + ".AdHoc.Project";
            var projectId = ProjectId.CreateNewId(projectName);

            var solution = new AdhocWorkspace().CurrentSolution
                                               .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                                               .AddMetadataReference(projectId, CorlibReference);

            const string newFileName = "ParseData.cs";
            var documentId = DocumentId.CreateNewId(projectId, newFileName);
            solution = solution.AddDocument(documentId, newFileName, SourceText.From(source));

            var project = solution.GetProject(projectId);

            return project.Documents.First();
        }
    }
}