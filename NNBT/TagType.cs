using System;

namespace NNbt
{
    /// <summary>
    /// Defines the type of an NBT tag.
    /// </summary>
    public enum TagType
    {
        /// <summary>
        /// A null tag, used to terminate lists.
        /// </summary>
        End = 0,

        /// <summary>
        /// A tag containing an 8-bit signed integer.
        /// </summary>
        Byte = 1,

        /// <summary>
        /// A tag containing a 16-bit signed integer.
        /// </summary>
        Short = 2,

        /// <summary>
        /// A tag containing a 32-bit signed integer.
        /// </summary>
        Int = 3,

        /// <summary>
        /// A tag containing a 64-bit signed integer.
        /// </summary>
        Long = 4,

        /// <summary>
        /// A tag containing a 32-bit (single precision) floating-point value.
        /// </summary>
        Float = 5,

        /// <summary>
        /// A tag containing a 64-bit (double precision) floating-point value.
        /// </summary>
        Double = 6,

        /// <summary>
        /// A tag containing an array of unsigned 8-bit byte values.
        /// </summary>
        ByteArray = 7,

        /// <summary>
        /// A tag containing a string of text.
        /// </summary>
        String = 8,

        /// <summary>
        /// A tag containing a sequential list of tags, where all tags of of the same type.
        /// </summary>
        List = 9,

        /// <summary>
        /// A tag containing a key-value store of tags, where each tag can be of any type.
        /// </summary>
        Compound = 10,

        /// <summary>
        /// A tag containing an array of signed 32-bit values.
        /// </summary>
        IntArray = 11,

        //-----------------------------------------------------
        // NbtEdge Extensions

        /// <summary>
        /// A tag containing a 16-byte GUID
        /// </summary>
        Guid = 64,
    }
}