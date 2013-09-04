using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a float tag type.
    /// </summary>
    public sealed class TagFloat : Tag
    {
        public TagFloat () { }

        public TagFloat (float d)
        {
            Data = d;
        }

        public TagFloat (Stream stream)
        {
            byte[] bytes = new byte[4];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            Data = BitConverter.ToSingle(bytes, 0);
        }

        public float Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Float; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagFloat(Data);
        }

        public new TagFloat DeepCopy ()
        {
            return new TagFloat(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagFloat) ? Data == (tag as TagFloat).Data : false;
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

        public static implicit operator TagFloat (byte b)
        {
            return new TagFloat(b);
        }

        public static implicit operator TagFloat (short s)
        {
            return new TagFloat(s);
        }

        public static implicit operator TagFloat (int i)
        {
            return new TagFloat(i);
        }

        public static implicit operator TagFloat (long l)
        {
            return new TagFloat(l);
        }

        public static implicit operator TagFloat (float f)
        {
            return new TagFloat(f);
        }

        public static explicit operator TagDouble (TagFloat tag)
        {
            return new TagDouble(tag.Data);
        }
    }
}