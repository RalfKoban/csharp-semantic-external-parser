using System;

using Microsoft.CodeAnalysis;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    public struct CharacterSpan : IEquatable<CharacterSpan>
    {
        public static readonly CharacterSpan None = new CharacterSpan(0, -1);

        public CharacterSpan(SyntaxTriviaList trivia) : this(trivia.FullSpan.Start, trivia.FullSpan.End -1) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        {
        }

        public CharacterSpan(SyntaxNode node) : this(FindFullSpanStart(node), FindFullSpanEnd(node)) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        {
        }

        public CharacterSpan(SyntaxToken token) : this(FindFullSpanStart(token), FindFullSpanEnd(token))
        {
        }

        public CharacterSpan(SyntaxToken start, SyntaxToken end) : this(FindFullSpanStart(start), FindFullSpanEnd(end)) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        {
        }

        public CharacterSpan(int start, int end)
        {
            if (start > end && (start != 0 || end != -1))
            {
                throw new ArgumentException($"{nameof(start)} should be less than {nameof(end)} but {start} is greater than {end}!", nameof(start));
            }

            Start = start;
            End = end;
        }

        public int Start { get; }

        public int End { get; }

        public static bool operator ==(CharacterSpan left, CharacterSpan right) => Equals(left, right);

        public static bool operator !=(CharacterSpan left, CharacterSpan right) => !Equals(left, right);

        public bool Equals(CharacterSpan other) => Start == other.Start && End == other.End;

        public override bool Equals(object obj) => obj is CharacterSpan other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }

        public override string ToString() => $"Span: {Start}, {End}";

        private static int FindFullSpanStart(SyntaxNode node)
        {
            var start = node.HasLeadingTrivia
                        ? node.GetLeadingTrivia().First().Span.Start
                        : node.FullSpan.Start;

            return start;
        }

        private static int FindFullSpanEnd(SyntaxNode node)
        {
            var end = node.HasTrailingTrivia
                      ? node.GetTrailingTrivia().Last().Span.End
                      : node.FullSpan.End;

            return end - 1; // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        }

        private static int FindFullSpanStart(SyntaxToken token)
        {
            var start = token.HasLeadingTrivia
                        ? token.LeadingTrivia.First().Span.Start
                        : token.FullSpan.Start;

            return start;
        }

        private static int FindFullSpanEnd(SyntaxToken token)
        {
            var end = token.HasTrailingTrivia
                      ? token.TrailingTrivia.Last().Span.End
                      : token.FullSpan.End;

            return end - 1; // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        }
    }
}