﻿using System.Diagnostics;

using YamlDotNet.Serialization;

namespace MiKoSolutions.SemanticParsers.CSharp.Yaml
{
    [DebuggerDisplay("Type={Type}, Name={Name}, ClassType={GetType().Name}")]
    public abstract class Node
    {
        [YamlIgnore]
        private string _type;

        [YamlIgnore]
        private string _name;

        [YamlMember(Alias = "type", Order = 1)]
        public string Type
        {
            get => _type;
            set => _type = value is null ? null : string.Intern(value); // performance optimization for large files
        }

        [YamlMember(Alias = "name", Order = 2)]
        public string Name
        {
            get => _name;
            set => _name = value is null ? null : string.Intern(value); // performance optimization for large files
        }

        [YamlMember(Alias = "locationSpan", Order = 3)]
        public LocationSpan LocationSpan { get; set; }

        public abstract CharacterSpan GetTotalSpan();
    }
}