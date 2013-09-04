using System;
using System.IO;
using System.Text;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a string tag type.
    /// </summary>
    public sealed class TagString : Tag
    {
        public TagString () { }

        public TagString (string d)
        {
            Data = d ?? "";
        }

        public TagString (Stream stream)
        {
            byte[] lenBytes = new byte[2];
            if (stream.Read(lenBytes, 0, 2) < 2)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            short length = BitConverter.ToInt16(lenBytes, 0);
            if (length < 0)
                throw new IOException("Unexpected stream data");

            byte[] strBytes = new byte[length];
            if (stream.Read(strBytes, 0, length) < length)
                throw new EndOfStreamException();

            Data = Encoding.UTF8.GetString(strBytes);
        }

        public string Data { get; set; }

        public int Length
        {
            get { return Data.Length; }
        }

        public override TagType TagType
        {
            get { return TagType.String; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagString(Data);
        }

        public new TagString DeepCopy ()
        {
            return new TagString(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagString) ? Data == (tag as TagString).Data : false;
        }

        public override void Write (Stream stream)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(Data);

            int length = Math.Min(bytes.Length, short.MaxValue);
            byte[] lenBytes = BitConverter.GetBytes((short)length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            stream.Write(lenBytes, 0, 2);
            stream.Write(bytes, 0, length);
        }

        public override string ToString ()
        {
            return Data;
        }

        public static implicit operator TagString (string s)
        {
            return new TagString(s);
        }

        public static implicit operator string (TagString s)
        {
            return s.Data;
        }
    }
}