using System.IO;

using MiKoSolutions.SemanticParsers.CSharp.Yaml.Converters;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    public static class YamlWriter
    {
        public static void Write(TextWriter writer, object graph)
        {
            var serializer = new SerializerBuilder()
                .WithTypeConverter(new CharacterSpanConverter())
                .WithTypeConverter(new LocationSpanConverter())
                .WithTypeConverter(new ParsingErrorConverter())
                .Build();
            serializer.Serialize(writer, graph);
        }
    }
}