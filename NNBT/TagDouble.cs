using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a float tag type.
    /// </summary>
    public sealed class TagDouble : Tag
    {
        public TagDouble () { }

        public TagDouble (double d)
        {
            Data = d;
        }

        public TagDouble (Stream stream)
        {
            byte[] bytes = new byte[8];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Data = BitConverter.ToDouble(bytes, 0);
        }

        public double Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Double; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagDouble(Data);
        }

        public new TagDouble DeepCopy ()
        {
            return new TagDouble(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagDouble) ? Data == (tag as TagDouble).Data : false;
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

        public static implicit operator TagDouble (byte b)
        {
            return new TagDouble(b);
        }

        public static implicit operator TagDouble (short s)
        {
            return new TagDouble(s);
        }

        public static implicit operator TagDouble (int i)
        {
            return new TagDouble(i);
        }

        public static implicit operator TagDouble (long l)
        {
            return new TagDouble(l);
        }

        public static implicit operator TagDouble (float f)
        {
            return new TagDouble(f);
        }

        public static implicit operator TagDouble (double d)
        {
            return new TagDouble(d);
        }
    }
}