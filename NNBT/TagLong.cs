using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing an long tag type.
    /// </summary>
    public sealed class TagLong : Tag
    {
        public TagLong () { }

        public TagLong (long d)
        {
            Data = d;
        }

        public TagLong (Stream stream)
        {
            byte[] bytes = new byte[8];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Data = BitConverter.ToInt64(bytes, 0);
        }

        public long Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Long; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagLong(Data);
        }

        public new TagLong DeepCopy ()
        {
            return new TagLong(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagLong) ? Data == (tag as TagLong).Data : false;
        }

        public override void Write (Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(Data);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, 8);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagLong (byte b)
        {
            return new TagLong(b);
        }

        public static implicit operator TagLong (short s)
        {
            return new TagLong(s);
        }

        public static implicit operator TagLong (int i)
        {
            return new TagLong(i);
        }

        public static implicit operator TagLong (long l)
        {
            return new TagLong(l);
        }

        public static implicit operator long (TagLong tag)
        {
            return tag.Data;
        }

        public static explicit operator TagFloat (TagLong tag)
        {
            return new TagFloat(tag.Data);
        }

        public static explicit operator TagDouble (TagLong tag)
        {
            return new TagDouble(tag.Data);
        }
    }
}