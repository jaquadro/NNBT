using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing an int tag type.
    /// </summary>
    public sealed class TagInt : Tag
    {
        public TagInt () { }

        public TagInt (int d)
        {
            Data = d;
        }

        public TagInt (Stream stream)
        {
            byte[] bytes = new byte[4];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Data = BitConverter.ToInt32(bytes, 0);
        }

        public int Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Int; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagInt(Data);
        }

        public new TagInt DeepCopy ()
        {
            return new TagInt(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagInt) ? Data == (tag as TagInt).Data : false;
        }

        public override void Write (Stream stream)
        {
            byte[] bytes = BitConverter.GetBytes(Data);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            stream.Write(bytes, 0, 4);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagInt (byte b)
        {
            return new TagInt(b);
        }

        public static implicit operator TagInt (short s)
        {
            return new TagInt(s);
        }

        public static implicit operator TagInt (int i)
        {
            return new TagInt(i);
        }

        public static implicit operator int (TagInt s)
        {
            return s.Data;
        }

        public static implicit operator long (TagInt s)
        {
            return s.Data;
        }

        public static explicit operator TagLong (TagInt tag)
        {
            return new TagLong(tag.Data);
        }

        public static explicit operator TagFloat (TagInt tag)
        {
            return new TagFloat(tag.Data);
        }

        public static explicit operator TagDouble (TagInt tag)
        {
            return new TagDouble(tag.Data);
        }
    }
}