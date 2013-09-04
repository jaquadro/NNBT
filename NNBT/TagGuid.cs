using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a GUID tag type.
    /// </summary>
    public sealed class TagGuid : Tag
    {
        public TagGuid () { }

        public TagGuid (Guid d)
        {
            Data = d;
        }

        public TagGuid (Stream stream)
        {
            byte[] bytes = new byte[16];
            if (stream.Read(bytes, 0, bytes.Length) < bytes.Length)
                throw new EndOfStreamException();

            Data = new Guid(bytes);
        }

        public Guid Data { get; set; }

        public override TagType TagType
        {
            get { return TagType.Guid; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagGuid(Data);
        }

        public new TagGuid DeepCopy ()
        {
            return new TagGuid(Data);
        }

        public override bool DataEqual (Tag tag)
        {
            return (tag is TagGuid) ? Data == (tag as TagGuid).Data : false;
        }

        public override void Write (Stream stream)
        {
            stream.Write(Data.ToByteArray(), 0, 16);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }
    }
}