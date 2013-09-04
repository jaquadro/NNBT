using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NNbt.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NbtPropertyAttribute : Attribute
    {
        public TagType? Type { get; set; }
        public string Name { get; set; }

        public NbtPropertyAttribute ()
        { }

        public NbtPropertyAttribute (string name)
        {
            Name = name;
        }

        public NbtPropertyAttribute (TagType propertyType)
        {
            Type = propertyType;
        }
    }

    public class NbtListAttribute : NbtPropertyAttribute
    {
        public TagType? SubType { get; set; }

        public NbtListAttribute ()
            : base(TagType.List)
        { }

        public NbtListAttribute (string name)
            : base(TagType.List)
        {
            Name = name;
        }

        public NbtListAttribute (TagType subType)
            : base(TagType.List)
        {
            SubType = subType;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class NbtTypeResolverAttribute : Attribute
    {
        public Type Type { get; set; }

        public NbtTypeResolverAttribute (Type type)
        {
            Type = type;
        }
    }
}
