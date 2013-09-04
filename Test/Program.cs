using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NNbt;
using NNbt.Serialization;

namespace Test
{
    class Program
    {
        static void Main (string[] args)
        {
            TagCompound tag = NbtSerializer.Serialize(new WhatTheBlah());
            WhatTheBlah obj = NbtSerializer.Deserialize<WhatTheBlah>(tag);
        }
    }

    public class Item
    {
        [NbtProperty]
        public short Id { get; set; }

        [NbtProperty]
        public short Data { get; set; }

        [NbtProperty]
        public short Count { get; set; }
    }

    public class WhatTheBlah
    {
        [NbtProperty]
        public int X { get; set; }

        [NbtProperty]
        public int Y { get; set; }

        [NbtProperty]
        public List<float> Coords { get; set; }

        [NbtProperty]
        public Item Equipped { get; set; }

        [NbtList]
        public List<Item> Inventory { get; set; }

        [NbtProperty]
        public Layer CurLayer { get; set; }

        [NbtProperty]
        public List<Layer> Layers { get; internal set; }

        public WhatTheBlah ()
        {
            X = 5;
            Y = 10;
            Coords = new List<float> { 3.5f, 7.8f, 9.3f, 10.9f };

            Equipped = null;
            Inventory = new List<Item>() {
                new Item() { Id = 6, Count = 1 },
                new Item() { Id = 280, Count = 3 },
            };
            CurLayer = new ObjectLayer() { X = 1, Y = 2, ObjectClass = 3 };
            Layers = new List<Layer>() {
                new TileLayer() { X = 4, Y = 9, TileX = 16, TileY = 16 },
                new ObjectLayer() { X = 5, Y = 10, ObjectClass = 15 },
            };
        }
    }

    [NbtTypeResolver(typeof(Layer.TypeResolver))]
    public abstract class Layer
    {
        [NbtProperty]
        public abstract string Id { get; }

        [NbtProperty]
        public int X { get; set; }

        [NbtProperty]
        public int Y { get; set; }

        /*[NbtTypeResolver]
        internal static Type Resolve (TagNodeCompound tag)
        {
            if (!tag.ContainsKey("id"))
                return null;

            string id = tag["id"].ToTagString();
            switch (id) {
                case "Tile":
                    return typeof(TileLayer);
                case "Object":
                    return typeof(ObjectLayer);
                default:
                    return null;
            }
        }*/

        internal class TypeResolver : INbtTypeResolver
        {
            Type INbtTypeResolver.Resolve (TagCompound tag)
            {
                if (!tag.ContainsKey("Id"))
                    return null;

                string id = tag["Id"] as TagString;
                switch (id) {
                    case "Tile":
                        return typeof(TileLayer);
                    case "Object":
                        return typeof(ObjectLayer);
                    default:
                        return null;
                }
            }
        }
    }

    public class TileLayer : Layer
    {
        public override string Id
        {
            get { return "Tile"; }
        }

        [NbtProperty]
        public int TileX { get; set; }

        [NbtProperty]
        public int TileY { get; set; }
    }

    public class ObjectLayer : Layer
    {
        public override string Id
        {
            get { return "Object"; }
        }

        [NbtProperty]
        public int ObjectClass { get; set; }
    }
}
