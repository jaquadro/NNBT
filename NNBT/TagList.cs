using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a list tag type containing other nodes.
    /// </summary>
    /// <remarks>
    /// A list node contains 0 or more nodes of the same type.  The nodes are unnamed
    /// but can be accessed by sequential index.
    /// </remarks>
    public sealed class TagList : Tag, IList<Tag>
    {
        private List<Tag> _items = null;

        public TagList (TagType type)
        {
            ValueType = type;
            _items = new List<Tag>();
        }

        public TagList (TagType type, List<Tag> items)
        {
            ValueType = type;
            _items = items;
        }

        public TagList (Stream stream)
        {
            int valByte = stream.ReadByte();
            if (valByte == -1)
                throw new EndOfStreamException();

            if (!NbtRegistry.IsRegistered((TagType)valByte))
                throw new IOException("Unexpected tag type: " + valByte);

            ValueType = (TagType)valByte;

            byte[] lenBytes = new byte[4];
            if (stream.Read(lenBytes, 0, 4) < 4)
                throw new EndOfStreamException();

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            int length = BitConverter.ToInt32(lenBytes, 0);
            if (length < 0)
                throw new IOException("Unexpected stream data");

            for (int i = 0; i < length; i++)
                Add(NbtRegistry.Read(stream, ValueType));
        }

        public TagType ValueType { get; private set; }

        public override TagType TagType
        {
            get { return TagType.List; }
        }

        public int Count
        {
            get { return _items.Count; }
        }

        protected override Tag DeepCopyCore ()
        {
            TagList list = new TagList(ValueType);
            foreach (Tag item in _items)
                list.Add(item.DeepCopy());
            return list;
        }

        public new TagList DeepCopy ()
        {
            return DeepCopyCore() as TagList;
        }

        public override void Write (Stream stream)
        {
            byte[] lenBytes = BitConverter.GetBytes(Count);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(lenBytes);

            stream.WriteByte((byte)ValueType);
            stream.Write(lenBytes, 0, 4);

            foreach (Tag v in _items)
                v.Write(stream);
        }

        public Tag Find (Predicate<Tag> match)
        {
            return _items.Find(match);
        }

        public List<Tag> FindAll (Predicate<Tag> match)
        {
            return _items.FindAll(match);
        }

        public int RemoveAll (Predicate<Tag> match)
        {
            return _items.RemoveAll(match);
        }

        public void Reverse ()
        {
            _items.Reverse();
        }

        public void Reverse (int index, int count)
        {
            _items.Reverse(index, count);
        }

        public void Sort ()
        {
            _items.Sort();
        }

        public override string ToString ()
        {
            return _items.ToString();
        }

        public void ChangeValueType (TagType type)
        {
            if (type == ValueType)
                return;

            _items.Clear();
            ValueType = type;
        }

        #region IList<Tag> Members

        /// <summary>
        /// Searches for the specified subnode and returns the zero-based index of the first occurrence within the entire node's list.
        /// </summary>
        /// <param name="item">The subnode to locate.</param>
        /// <returns>The zero-based index of the subnode within the node's list if found, or -1 otherwise.</returns>
        public int IndexOf (Tag item)
        {
            return _items.IndexOf(item);
        }

        /// <summary>
        /// Inserts a subnode into the node's list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the subnode should be inserted.</param>
        /// <param name="item">The subnode to insert.</param>
        /// <exception cref="ArgumentException">Thrown when a subnode being inserted has the wrong tag type.</exception>
        public void Insert (int index, Tag item)
        {
            if (item.TagType != ValueType) {
                throw new ArgumentException("The tag type of item is invalid for this node");
            }
            _items.Insert(index, item);
        }

        /// <summary>
        /// Removes the subnode from the node's list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index to remove a subnode at.</param>
        public void RemoveAt (int index)
        {
            _items.RemoveAt(index);
        }

        /// <summary>
        /// Gets or sets the subnode in the node's list at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index to get or set from.</param>
        /// <returns>The subnode at the specified index.</returns>
        /// <exception cref="ArgumentException">Thrown when a subnode being assigned has the wrong tag type.</exception>
        public Tag this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                if (value.TagType != ValueType) {
                    throw new ArgumentException("The tag type of the assigned subnode is invalid for this node");
                }
                _items[index] = value;
            }
        }

        #endregion

        #region ICollection<Tag> Members

        /// <summary>
        /// Adds a subnode to the end of the node's list.
        /// </summary>
        /// <param name="item">The subnode to add.</param>
        /// <exception cref="ArgumentException">Thrown when a subnode being added has the wrong tag type.</exception>
        public void Add (Tag item)
        {
            if (item.TagType != ValueType) {
                throw new ArgumentException("The tag type of item is invalid for this node");
            }

            _items.Add(item);
        }

        /// <summary>
        /// Removes all subnode's from the node's list.
        /// </summary>
        public void Clear ()
        {
            _items.Clear();
        }

        /// <summary>
        /// Checks if a subnode is contained within the node's list.
        /// </summary>
        /// <param name="item">The subnode to check for existance.</param>
        /// <returns>Status indicating if the subnode exists in the node's list.</returns>
        public bool Contains (Tag item)
        {
            return _items.Contains(item);
        }

        /// <summary>
        /// Copies the entire node's list to a compatible one-dimensional array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the subnodes copied. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public void CopyTo (Tag[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets a value indicating whether the node is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the first occurance of a subnode from the node's list.
        /// </summary>
        /// <param name="item">The subnode to remove.</param>
        /// <returns>Status indicating whether a subnode was removed.</returns>
        public bool Remove (Tag item)
        {
            return _items.Remove(item);
        }

        #endregion

        #region IEnumerable<Tag> Members

        /// <summary>
        /// Returns an enumerator that iterates through all of the subnodes in the node's list.
        /// </summary>
        /// <returns>An enumerator for this node.</returns>
        public IEnumerator<Tag> GetEnumerator ()
        {
            return _items.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through all of the subnodes in the node's list.
        /// </summary>
        /// <returns>An enumerator for this node.</returns>
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return _items.GetEnumerator();
        }

        #endregion
    }
}