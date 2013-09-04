using System;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing an int array tag type.
    /// </summary>
    public sealed class TagIntArray : Tag
    {
        public TagIntArray () { }

        public TagIntArray (int[] d)
        {
            Data = d;
        }

        public TagIntArray (Stream stream)
        {
            byte[] lenBytes = new byte[4];
            if (stream.Read(lenBytes, 0, 4) < 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            int length = BitConverter.ToInt32(lenBytes, 0);
            if (length < 0)
                throw new IOException("Unexpected stream data");

            Data = new int[length];

            byte[] buffer = new byte[4];
            for (int i = 0; i < length; i++) {
                if (stream.Read(buffer, 0, 4) < 4)
                    throw new EndOfStreamException();

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);

                Data[i] = BitConverter.ToInt32(buffer, 0);
            }
        }

        public int[] Data { get; set; }

        public int Length
        {
            get { return Data.Length; }
        }

        public override TagType TagType
        {
            get { return TagType.IntArray; }
        }

        public int this[int index]
        {
            get { return Data[index]; }
            set { Data[index] = value; }
        }

        protected override Tag DeepCopyCore ()
        {
            if (Data == null)
                return new TagIntArray();

            int[] arr = new int[Data.Length];
            Data.CopyTo(arr, 0);

            return new TagIntArray(arr);
        }

        public new TagIntArray DeepCopy ()
        {
            return DeepCopyCore() as TagIntArray;
        }

        public override bool DataEqual (Tag tag)
        {
            TagIntArray tagArray = tag as TagIntArray;
            if (tag == null)
                return false;

            int[] arr1 = Data;
            int[] arr2 = tagArray.Data;

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

            byte[] data = new byte[Data.Length * 4];
            for (int i = 0; i < Data.Length; i++) {
                byte[] buffer = BitConverter.GetBytes(Data[i]);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buffer);
                Array.Copy(buffer, 0, data, i * 4, 4);
            }

            stream.Write(data, 0, data.Length);
        }

        public override string ToString ()
        {
            return Data.ToString();
        }

        public static implicit operator TagIntArray (int[] i)
        {
            return new TagIntArray(i);
        }

        public static implicit operator int[] (TagIntArray tag)
        {
            return tag.Data;
        }
    }
}