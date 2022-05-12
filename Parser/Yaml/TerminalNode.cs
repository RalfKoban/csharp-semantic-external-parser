using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    public sealed class TerminalNode : Node
    {
        [YamlMember(Alias = "span", Order = 4)]
        public CharacterSpan Span { get; set; }
    }
}