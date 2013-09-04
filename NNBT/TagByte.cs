using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a byte tag type.
    /// </summary>
    public sealed class TagByte : Tag
    {
        public TagByte () { }

        public TagByte (byte d)
        {
            Data = d;
        }

        public TagByte (Stream stream)
        {
            int data = stream.ReadByte();
            if (data == -1)
                throw new EndOfStreamException();

            Data = (byte)data;
        }

        public byte Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Byte; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagByte(Data);
        }

        public new TagByte DeepCopy ()
        {
            return new TagByte(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagByte) ? Data == (tag as TagByte).Data : false;
        }

        public override void Write (Stream stream)
        {
            stream.WriteByte(Data);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagByte (sbyte b)
        {
            return new TagByte(unchecked((byte)b));
        }

        public static implicit operator sbyte (TagByte b)
        {
            return unchecked((sbyte)b.Data);
        }

        public static implicit operator TagByte (byte b)
        {
            return new TagByte(b);
        }

        public static implicit operator byte (TagByte b)
        {
            return b.Data;
        }

        public static implicit operator short (TagByte b)
        {
            return b.Data;
        }

        public static implicit operator int (TagByte b)
        {
            return b.Data;
        }

        public static implicit operator long (TagByte b)
        {
            return b.Data;
        }

        public static explicit operator TagShort (TagByte tag)
        {
            return new TagShort(tag.Data);
        }

        public static explicit operator TagInt (TagByte tag)
        {
            return new TagInt(tag.Data);
        }

        public static explicit operator TagLong (TagByte tag)
        {
            return new TagLong(tag.Data);
        }

        public static explicit operator TagFloat (TagByte tag)
        {
            return new TagFloat(tag.Data);
        }

        public static explicit operator TagDouble (TagByte tag)
        {
            return new TagDouble(tag.Data);
        }
    }
}