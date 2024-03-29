﻿using System;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml.Converters
{
    public sealed class CharacterSpanConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(CharacterSpan);

        public object ReadYaml(IParser parser, Type type) => throw new NotImplementedException();

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (value is CharacterSpan span)
            {
                emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));
                emitter.Emit(new Scalar(span.Start.ToString()));
                emitter.Emit(new Scalar(span.End.ToString()));
                emitter.Emit(new SequenceEnd());
            }
            else
            {
                throw new NotImplementedException("wrong type");
            }
        }
    }
}