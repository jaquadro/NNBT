using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NNbt
{
    public class NbtTree
    {
        public NbtTree ()
        {
            Name = "";
            Root = new TagCompound();
        }

        public NbtTree (TagCompound root)
        {
            Name = "";
            Root = root;
        }

        public NbtTree (TagCompound root, string name)
        {
            Name = name;
            Root = root;
        }

        public NbtTree (Stream stream)
        {
            int typeByte = stream.ReadByte();
            if (typeByte == -1)
                throw new EndOfStreamException();

            TagType type = (TagType)typeByte;
            if (type == TagType.Compound) {
                Name = new TagString(stream);
                Root = new TagCompound(stream);
            }
            else {
                Name = "";
                Root = new TagCompound();
            }
        }

        public string Name { get; set; }
        public TagCompound Root { get; private set; }

        public void Write (Stream stream)
        {
            if (Root == null)
                throw new InvalidOperationException("Tree has no root");

            stream.WriteByte((byte)TagType.Compound);

            new TagString(Name).Write(stream);
            Root.Write(stream);
        }
    }
}
