using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

namespace NNbt.Serialization
{
    public interface INbtTypeResolver
    {
        Type Resolve (TagCompound tag);
    }

    public class NbtSerializer
    {
        public static TagCompound Serialize (object obj)
        {
            return ToTagCompound(obj);
        }

        private static Tag ToTag (object obj, PropertyInfo info, NbtPropertyAttribute attrib)
        {
            if (attrib is NbtListAttribute && obj is IList)
                return ToTagList(obj, (attrib as NbtListAttribute).SubType ?? TagType.End);
            else
                return ToTag(obj, attrib.Type ?? InferType(info.PropertyType));
        }

        private static Tag ToTag (object obj, TagType tagType)
        {
            switch (tagType) {
                case TagType.Byte:
                    return new TagByte((byte)obj);
                case TagType.Short:
                    return new TagShort((short)obj);
                case TagType.Int:
                    return new TagInt((int)obj);
                case TagType.Long:
                    return new TagLong((long)obj);
                case TagType.Float:
                    return new TagFloat((float)obj);
                case TagType.Double:
                    return new TagDouble((double)obj);
                case TagType.String:
                    return new TagString((string)obj);
                case TagType.ByteArray:
                    return new TagByteArray((byte[])obj);
                case TagType.IntArray:
                    return new TagIntArray((int[])obj);
                case TagType.List:
                    return ToTagList(obj);
                default:
                    return ToTagCompound(obj);
            }
        }

        private static TagList ToTagList (object obj)
        {
            return ToTagList(obj, TagType.End);
        }

        private static TagList ToTagList (object obj, TagType subType)
        {
            Type type = obj.GetType();
            if (subType == TagType.End && type.IsGenericType)
                subType = InferType(type.GetGenericArguments()[0]);

            if (subType == TagType.End)
                subType = TagType.Compound;

            return ToTagList(obj as IList, subType);
        }

        private static TagList ToTagList (IList obj, TagType subType)
        {
            TagList list = new TagList(subType);

            for (int i = 0; i < obj.Count; i++)
                list.Add(ToTag(obj[i], subType));

            return list;
        }

        private static TagCompound ToTagCompound (object obj)
        {
            Type type = obj.GetType();
            if (type.IsSubclassOf(typeof(IDictionary)))
                return ToTagCompound(obj as IDictionary);

            TagCompound compound = new TagCompound();

            foreach (PropertyInfo prop in type.GetProperties()) {
                foreach (NbtPropertyAttribute attrib in Attribute.GetCustomAttributes(prop, typeof(NbtPropertyAttribute), true)) {
                    string name = attrib.Name ?? prop.Name;
                    object propObj = prop.GetValue(obj, null);
                    if (propObj == null)
                        continue;

                    compound.Add(name, ToTag(propObj, prop, attrib));
                }
            }

            return compound;
        }

        private static TagCompound ToTagCompound (IDictionary obj)
        {
            Type type = obj.GetType();

            Type keyType = typeof(object);
            if (type.IsSubclassOf(typeof(IDictionary<,>)))
                keyType = type.GetGenericArguments()[0];

            // Check if all keys are strings.  If not, return an empty compound
            if (keyType != typeof(string)) {
                foreach (var k in obj.Keys) {
                    if (k.GetType() != typeof(string))
                        return new TagCompound();
                }
            }

            TagCompound compound = new TagCompound();
            foreach (var k in obj.Keys) {
                object v = obj[k];
                compound.Add(k as string, ToTag(v, InferType(v.GetType())));
            }

            return compound;
        }

        private static TagType InferType (Type type)
        {
            if (type == typeof(byte) || type == typeof(sbyte))
                return TagType.Byte;
            if (type == typeof(short) || type == typeof(ushort))
                return TagType.Short;
            if (type == typeof(int) || type == typeof(uint))
                return TagType.Int;
            if (type == typeof(long) || type == typeof(ulong))
                return TagType.Long;
            if (type == typeof(float))
                return TagType.Float;
            if (type == typeof(double))
                return TagType.Double;
            if (type == typeof(string))
                return TagType.String;
            if (type == typeof(byte[]) || type == typeof(sbyte[]))
                return TagType.ByteArray;
            if (type == typeof(int[]) || type == typeof(uint[]))
                return TagType.IntArray;
            if (type.GetInterfaces().Contains(typeof(IDictionary)))
                return TagType.Compound;
            if (type.GetInterfaces().Contains(typeof(IList)))
                return TagType.List;

            return TagType.Compound;
        }


        public static T Deserialize<T> (TagCompound tag)
        {
            return (T)FromTagCompound(tag, typeof(T));
        }

        private static object FromTagCompound (TagCompound tag, Type type)
        {
            type = ResolveTargetType(tag, type);

            ConstructorInfo defaultConstructor = type.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor == null)
                return null;

            object obj = defaultConstructor.Invoke(null);

            foreach (PropertyInfo prop in type.GetProperties()) {
                foreach (NbtPropertyAttribute attrib in Attribute.GetCustomAttributes(prop, typeof(NbtPropertyAttribute), true)) {
                    if (!prop.CanWrite)
                        continue;

                    string name = attrib.Name ?? prop.Name;
                    if (!tag.ContainsKey(name))
                        continue;

                    object propObj = FromTag(tag[name], prop, attrib);
                    if (propObj == null)
                        continue;

                    prop.SetValue(obj, propObj, null);
                }
            }

            return obj;
        }

        private static object FromTag (Tag tag, PropertyInfo info, NbtPropertyAttribute attrib)
        {
            return FromTag(tag, info.PropertyType);
        }

        private static Type ResolveTargetType (Tag tag, Type type)
        {
            if (IsListType(type))
                type = GetListSubType(type);

            NbtTypeResolverAttribute resolverAttrib = type.GetCustomAttributes(typeof(NbtTypeResolverAttribute), false)
                .FirstOrDefault() as NbtTypeResolverAttribute;
            if (resolverAttrib != null)
                type = resolverAttrib.Type ?? type;

            if (type.GetInterfaces().Contains(typeof(INbtTypeResolver))) {
                ConstructorInfo resolverConstructor = type.GetConstructor(Type.EmptyTypes);
                if (resolverConstructor != null) {
                    INbtTypeResolver resolver = resolverConstructor.Invoke(null) as INbtTypeResolver;
                    type = resolver.Resolve(tag as TagCompound);
                }
            }

            return type;
        }

        private static bool IsListType (Type type)
        {
            return type.GetInterfaces().Contains(typeof(IList));
        }

        private static Type GetListSubType (Type type)
        {
            if (type.IsGenericType)
                return type.GetGenericArguments()[0];
            else
                return typeof(object);
        }

        private static object FromTag (Tag tag, Type type)
        {
            switch (tag.TagType) {
                case TagType.Byte:
                    return (tag as TagByte).Data;
                case TagType.Short:
                    return (tag as TagShort).Data;
                case TagType.Int:
                    return (tag as TagInt).Data;
                case TagType.Long:
                    return (tag as TagLong).Data;
                case TagType.Float:
                    return (tag as TagFloat).Data;
                case TagType.Double:
                    return (tag as TagDouble).Data;
                case TagType.String:
                    return (tag as TagString).Data;
                case TagType.ByteArray:
                    return (tag as TagByteArray).Data;
                case TagType.IntArray:
                    return (tag as TagIntArray).Data;
                case TagType.List:
                    return FromTagList(tag as TagList, type);
                default:
                    return FromTagCompound(tag as TagCompound, type);
            }
        }

        private static object FromTagList (TagList tag, Type listType)
        {
            ConstructorInfo defaultConstructor = listType.GetConstructor(Type.EmptyTypes);
            if (defaultConstructor == null)
                return null;

            if (!listType.GetInterfaces().Contains(typeof(IList)))
                return defaultConstructor.Invoke(null);

            IList obj = defaultConstructor.Invoke(null) as IList;

            foreach (Tag item in tag) {
                Type subType = ResolveTargetType(item, listType);
                object itemObj = FromTag(item, subType);
                if (itemObj == null)
                    continue;

                obj.Add(itemObj);
            }

            return obj;
        }
    }
}
