using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace NNbt
{
    public static class NbtRegistry
    {
        private static Dictionary<TagType, Type> _registry = new Dictionary<TagType, Type>();

        public static void Register<T> (TagType tagType)
        {
            Register(typeof(T), tagType);
        }

        public static void Register (Type type, TagType tagType)
        {
            if (GetStreamConstructor(type) == null)
                throw new ArgumentException("Type must contain a public constructor with signature (Stream)");

            _registry[tagType] = type;
        }

        public static bool IsRegistered (TagType tagType)
        {
            return _registry.ContainsKey(tagType);
        }

        public static Tag Read (Stream stream, TagType tagType)
        {
            if (!_registry.ContainsKey(tagType))
                throw new ArgumentException("Unregistered tag type: " + tagType);

            Type type = _registry[tagType];
            ConstructorInfo streamConstructor = GetStreamConstructor(type);

            return streamConstructor.Invoke(new object[] { stream }) as Tag;
        }

        private static ConstructorInfo GetStreamConstructor (Type type)
        {
            return type.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(Stream) }, null);
        }

        static NbtRegistry ()
        {
            Register<TagByte>(TagType.Byte);
            Register<TagShort>(TagType.Short);
            Register<TagInt>(TagType.Int);
            Register<TagLong>(TagType.Long);
            Register<TagFloat>(TagType.Float);
            Register<TagDouble>(TagType.Double);
            Register<TagByteArray>(TagType.ByteArray);
            Register<TagIntArray>(TagType.IntArray);
            Register<TagString>(TagType.String);
            Register<TagList>(TagType.List);
            Register<TagCompound>(TagType.Compound);
            Register<TagNull>(TagType.End);

            // Tag Extensions
            Register<TagGuid>(TagType.Guid);
        }
    }
}
