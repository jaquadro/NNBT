using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing an unsigned byte array tag type.
    /// </summary>
    public sealed class TagByteArray : Tag
    {
        public TagByteArray () { }

        public TagByteArray (byte[] d)
        {
            Data = d;
        }

        public TagByteArray (Stream stream)
        {
            byte[] lenBytes = new byte[4];
            if (stream.Read(lenBytes, 0, 4) < 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            int length = BitConverter.ToInt32(lenBytes, 0);
            if (length < 0)
                throw new IOException("Unexpected stream data");

            Data = new byte[length];
            if (stream.Read(Data, 0, length) < length)
                throw new EndOfStreamException();
        }

        public byte[] Data { get; set; }

        public int Length
        {
            get { return Data.Length; }
        }

        public override TagType TagType
        {
            get { return TagType.ByteArray; }
        }

        public byte this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        protected override Tag DeepCopyCore ()
        {
            if (Data == null)
                return new TagByteArray();

            byte[] arr = new byte[Data.Length];
            Data.CopyTo(arr, 0);

            return new TagByteArray(arr);
        }

        public new TagByteArray DeepCopy ()
        {
            return DeepCopyCore() as TagByteArray;
        }

        public override bool DataEqual (Tag tag)
        {
            TagByteArray tagArray = tag as TagByteArray;
            if (tagArray == null)
                return false;

            byte[] arr1 = Data;
            byte[] arr2 = tagArray.Data;

            if (arr1 == arr2)
                return true;
            if (arr1 == null || arr2 == null)
                return false;

            if (arr1.Length != arr2.Length)
                return false;

            for (int i = 0, n = arr1.Length; i < n; i++)
                if (arr1[i] != arr2[i])
                    return false;

            return true;
        }

        public override void Write (Stream stream)
        {
            byte[] lenBytes = BitConverter.GetBytes(Data.Length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            stream.Write(lenBytes, 0, 4);
            stream.Write(Data, 0, Data.Length);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagByteArray (byte[] b)
        {
            return new TagByteArray(b);
        }

        public static implicit operator byte[] (TagByteArray b)
        {
            return b.Data;
        }
    }
}