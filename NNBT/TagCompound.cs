using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NNbt
{
    /// <summary>
    /// An NBT node representing a compound tag type containing other nodes.
    /// </summary>
    public sealed class TagCompound : Tag, IDictionary<string, Tag>
    {
        private Dictionary<string, Tag> _tags;

        public TagCompound ()
        {
            _tags = new Dictionary<string, Tag>();
        }

        public TagCompound (Stream stream)
        {
            while (true) {
                int tagByte = stream.ReadByte();
                if (tagByte == -1)
                    throw new EndOfStreamException();

                if (!NbtRegistry.IsRegistered((TagType)tagByte))
                    throw new IOException("Unexpected tag type: " + tagByte);

                TagType type = (TagType)tagByte;
                if (type == TagType.End)
                    break;

                TagString name = new TagString(stream);
                Tag value = NbtRegistry.Read(stream, type);

                _tags[name.Data] = value;
            }
        }

        public override TagType TagType
        {
            get { return TagType.Compound; }
        }

        public int Count
        {
            get { return _tags.Count; }
        }

        protected override Tag DeepCopyCore ()
        {
            TagCompound list = new TagCompound();
            foreach (KeyValuePair<string, Tag> item in _tags)
                list[item.Key] = item.Value.DeepCopy();
            return list;
        }

        public new TagCompound DeepCopy ()
        {
            return DeepCopyCore() as TagCompound;
        }

        public override void Write (Stream stream)
        {
            foreach (var item in _tags)
                WriteTag(stream, item.Key, item.Value);

            stream.WriteByte((byte)TagType.End);
        }

        private static void WriteTag (Stream stream, TagString name, Tag tag)
        {
            if (tag.TagType != TagType.End) {
                stream.WriteByte((byte)tag.TagType);

                name.Write(stream);
                tag.Write(stream);
            }
        }

        // Shallow Merge
        public void MergeFrom (TagCompound tree)
        {
            foreach (KeyValuePair<string, Tag> node in tree) {
                if (_tags.ContainsKey(node.Key)) {
                    continue;
                }

                _tags.Add(node.Key, node.Value);
            }
        }

        public override string ToString ()
        {
            return _tags.ToString();
        }

        #region IDictionary<string, Tag> Members

        /// <summary>
        /// Adds a named subnode to the set.
        /// </summary>
        /// <param name="key">The name of the subnode.</param>
        /// <param name="value">The subnode to add.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="ArgumentException">A subnode with the same key already exists in the set.</exception>
        public void Add (string key, Tag value)
        {
            _tags.Add(key, value);
        }

        /// <summary>
        /// Checks if a subnode exists in the set with the specified name.
        /// </summary>
        /// <param name="key">The name of a subnode to check.</param>
        /// <returns>Status indicating whether a subnode with the specified name exists.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool ContainsKey (string key)
        {
            return _tags.ContainsKey(key);
        }

        /// <summary>
        /// Gets a collection containing all the names of subnodes in this set.
        /// </summary>
        public ICollection<string> Keys
        {
            get { return _tags.Keys; }
        }

        /// <summary>
        /// Removes a subnode with the specified name.
        /// </summary>
        /// <param name="key">The name of the subnode to remove.</param>
        /// <returns>Status indicating whether a subnode was removed.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool Remove (string key)
        {
            return _tags.Remove(key);
        }

        /// <summary>
        /// Gets the subnode associated with the given name.
        /// </summary>
        /// <param name="key">The name of the subnode to get.</param>
        /// <param name="value">When the function returns, contains the subnode assicated with the specified key.  If no subnode was found, contains a default value.</param>
        /// <returns>Status indicating whether a subnode was found.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        public bool TryGetValue (string key, out Tag value)
        {
            return _tags.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets a collection containing all the subnodes in this set.
        /// </summary>
        public ICollection<Tag> Values
        {
            get { return _tags.Values; }
        }

        /// <summary>
        /// Gets or sets the subnode with the associated name.
        /// </summary>
        /// <param name="key">The name of the subnode to get or set.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is null.</exception>
        /// <exception cref="KeyNotFoundException">The property is retrieved and key does not exist in the collection.</exception>
        public Tag this[string key]
        {
            get { return _tags[key]; }
            set { _tags[key] = value; }
        }

        #endregion

        #region ICollection<KeyValuePair<string, Tag>> Members

        /// <summary>
        /// Adds a subnode to the to the set with the specified name.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TVal}"/> structure representing the key and subnode to add to the set.</param>
        /// <exception cref="ArgumentNullException">The key of <paramref name="item"/> is null.</exception>
        /// <exception cref="ArgumentException">A subnode with the same key already exists in the set.</exception>
        public void Add (KeyValuePair<string, Tag> item)
        {
            _tags.Add(item.Key, item.Value);
        }

        /// <summary>
        /// Removes all of the subnodes from this node.
        /// </summary>
        public void Clear ()
        {
            _tags.Clear();
        }

        /// <summary>
        /// Checks if a specific subnode with a specific name is contained in the set.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TValue}"/> structure representing the key and subnode to look for.</param>
        /// <returns>Status indicating if the subnode and key combination exists in the set.</returns>
        public bool Contains (KeyValuePair<string, Tag> item)
        {
            Tag value;
            if (!_tags.TryGetValue(item.Key, out value)) {
                return false;
            }
            return value == item.Value;
        }

        /// <summary>
        /// Copies the elements of the <see cref="ICollection{T}"/> to an array of type <see cref="KeyValuePair{TKey, TVal}"/>, starting at the specified array index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array"/> that is the destination of the subnodes copied. The Array must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">The number of elements in the source <see cref="ICollection{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo (KeyValuePair<string, Tag>[] array, int arrayIndex)
        {
            if (array == null) {
                throw new ArgumentNullException();
            }
            if (arrayIndex < 0) {
                throw new ArgumentOutOfRangeException();
            }
            if (array.Length - arrayIndex < _tags.Count) {
                throw new ArgumentException();
            }

            foreach (KeyValuePair<string, Tag> item in _tags) {
                array[arrayIndex] = item;
                arrayIndex++;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the node is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Removes the specified key and subnode combination from the set.
        /// </summary>
        /// <param name="item">The <see cref="KeyValuePair{TKey, TVal}"/> structure representing the key and value to remove from the set.</param>
        /// <returns>Status indicating whether a subnode was removed.</returns>
        public bool Remove (KeyValuePair<string, Tag> item)
        {
            if (Contains(item)) {
                _tags.Remove(item.Key);
                return true;
            }
            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<string, Tag>> Members

        /// <summary>
        /// Returns an enumerator that iterates through all of the subnodes in the set.
        /// </summary>
        /// <returns>An enumerator for this node.</returns>
        public IEnumerator<KeyValuePair<string, Tag>> GetEnumerator ()
        {
            return _tags.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returns an enumerator that iterates through all of the subnodes in the set.
        /// </summary>
        /// <returns>An enumerator for this node.</returns>
        IEnumerator IEnumerable.GetEnumerator ()
        {
            return _tags.GetEnumerator();
        }

        #endregion
    }
}