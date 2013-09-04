using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An abstract base class representing a node in an NBT tree.
    /// </summary>
    public abstract class Tag
    {
        /// <summary>
        /// Gets the underlying tag type of the node.
        /// </summary>
        public abstract TagType TagType { get; }

        /// <summary>
        /// Makes a deep copy of the NBT node.
        /// </summary>
        /// <returns>A new NBT node.</returns>
        public Tag DeepCopy ()
        {
            return DeepCopyCore();
        }

        protected virtual Tag DeepCopyCore ()
        {
            return null;
        }

        public virtual bool DataEqual (Tag tag)
        {
            return false;
        }

        public virtual void Write (Stream stream)
        { }
    }
}