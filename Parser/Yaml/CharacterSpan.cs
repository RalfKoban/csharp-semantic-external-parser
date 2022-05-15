using System;

using Microsoft.CodeAnalysis;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    public struct CharacterSpan : IEquatable<CharacterSpan>
    {
        public static readonly CharacterSpan None = new CharacterSpan(0, -1);

        public CharacterSpan(SyntaxNode node) : this(node.Span.Start, node.Span.End -1) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        {
        }

        public CharacterSpan(SyntaxToken token) : this(token.Span.Start, token.Span.End - 1) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
        {
        }

        public CharacterSpan(SyntaxToken start, SyntaxToken end) : this(start.Span.Start, end.Span.End - 1) // Roslyn reports that the end of the full span is the start of the next span, hence we have to subtract 1 from the end
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
    }
}