using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a short tag type.
    /// </summary>
    public sealed class TagShort : Tag
    {
        public TagShort () { }

        public TagShort (short d)
        {
            Data = d;
        }

        public TagShort (Stream stream)
        {
            byte[] bytes = new byte[2];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Data = BitConverter.ToInt16(bytes, 0);
        }

        public short Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Short; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagShort(Data);
        }

        public new TagShort DeepCopy ()
        {
            return new TagShort(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagShort) ? Data == (tag as TagShort).Data : false;
        }

        public override void Write (Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(Data);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, 2);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagShort (byte b)
        {
            return new TagShort(b);
        }

        public static implicit operator TagShort (short s)
        {
            return new TagShort(s);
        }

        public static implicit operator short (TagShort s)
        {
            return s.Data;
        }

        public static implicit operator int (TagShort s)
        {
            return s.Data;
        }

        public static implicit operator long (TagShort s)
        {
            return s.Data;
        }

        public static explicit operator TagInt (TagShort tag)
        {
            return new TagInt(tag.Data);
        }

        public static explicit operator TagLong (TagShort tag)
        {
            return new TagLong(tag.Data);
        }

        public static explicit operator TagFloat (TagShort tag)
        {
            return new TagFloat(tag.Data);
        }

        public static explicit operator TagDouble (TagShort tag)
        {
            return new TagDouble(tag.Data);
        }
    }
}