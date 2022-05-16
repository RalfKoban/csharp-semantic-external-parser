using MiKoSolutions.SemanticParsers.CSharp.Yaml;

namespace MiKoSolutions.SemanticParsers.CSharp
{
    public static class GapFiller
    {
        public static void Fill(File file, CharacterPositionFinder finder)
        {
            // adjust location spans based on spans
            foreach (var node in file.Descendants())
            {
                switch (node)
                {
                    case TerminalNode t:
                        FillTerminalNode(t, finder);
                        break;

                    case Container c:
                        FillContainer(c, finder);
                        break;
                }
            }
        }

        private static void FillContainer(Container node, CharacterPositionFinder finder)
        {
            if (node.FooterSpan != CharacterSpan.None)
            {
                var span = node.GetTotalSpan();

                var start = finder.GetLineInfo(span.Start);
                var end = finder.GetLineInfo(span.End);

                node.LocationSpan = new LocationSpan(start, end);
            }
        }

        private static void FillTerminalNode(TerminalNode node, CharacterPositionFinder finder)
        {
            var span = node.Span;

            var start = finder.GetLineInfo(span.Start);
            var end = finder.GetLineInfo(span.End);

            node.LocationSpan = new LocationSpan(start, end);
        }
    }
}