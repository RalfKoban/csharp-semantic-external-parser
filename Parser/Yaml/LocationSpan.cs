using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    [DebuggerDisplay("Start: {Start}, End: {End}")]
    public struct LocationSpan : IEquatable<LocationSpan>
    {
        public LocationSpan(SyntaxNode node) : this(FindStartLinePosition(node), FindEndPosition(node))
        {
        }

        public LocationSpan(FileLinePositionSpan span) : this(span.StartLinePosition, span.EndLinePosition)
        {
        }

        public LocationSpan(LinePosition start, LinePosition end) : this(new LineInfo(start), new LineInfo(end))
        {
        }

        public LocationSpan(LineInfo start, LineInfo end)
        {
            Start = start;
            End = end;
        }

        public LineInfo Start { get; }

        public LineInfo End { get; }

        public static bool operator ==(LocationSpan left, LocationSpan right) => Equals(left, right);

        public static bool operator !=(LocationSpan left, LocationSpan right) => !Equals(left, right);

        public bool Equals(LocationSpan other) => Start == other.Start && End == other.End;

        public override bool Equals(object obj) => obj is LocationSpan other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public override string ToString() => $"Start: {Start}, End: {End}";

        private static LinePosition FindStartLinePosition(SyntaxNode node)
        {
            var span = node.HasLeadingTrivia
                           ? node.GetLeadingTrivia().First().FullSpan
                           : node.FullSpan;

            var lineSpan = node.SyntaxTree.GetLineSpan(span);

            return lineSpan.StartLinePosition;
        }

        private static LinePosition FindEndPosition(SyntaxNode node)
        {
            var span = node.HasTrailingTrivia
                            ? node.GetTrailingTrivia().Last().FullSpan
                            : node.FullSpan;

            var lineSpan = node.SyntaxTree.GetLineSpan(new TextSpan(span.Start, span.Length - 1)); // we need to subtract 1 because otherwise Roslyn would report another line (which we do not want in the parser's context)

            return lineSpan.EndLinePosition;
        }
    }
}