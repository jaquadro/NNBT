using System;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a null tag type.
    /// </summary>
    public sealed class TagNull : Tag
    {
        public static readonly TagNull Default = new TagNull();

        public override TagType TagType
        {
            get { return TagType.End; }
        }

        protected override Tag DeepCopyCore ()
        {
            return new TagNull();
        }

        public new TagNull DeepCopy ()
        {
            return new TagNull();
        }

        public override string ToString ()
        {
            return "";
        }
    }
}