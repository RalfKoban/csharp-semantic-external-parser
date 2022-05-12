﻿using System;
using System.Diagnostics;

using Microsoft.CodeAnalysis;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    [DebuggerDisplay("Start: {Start}, End: {End}")]
    public struct LocationSpan : IEquatable<LocationSpan>
    {
        public LocationSpan(SyntaxNode node)
        {
            var span = node.GetLocation().GetLineSpan();

            Start = new LineInfo(span.StartLinePosition);
            End = new LineInfo(span.EndLinePosition);
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
    }
}