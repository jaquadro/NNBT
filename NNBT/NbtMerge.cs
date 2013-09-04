using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NNbt
{
    public static class Id
    {
        private static Regex _pattern = new Regex(@"^(id|Id|ID|\wid|\w+(Id|ID))$");

        public static bool ContainsId (TagCompound tag)
        {
            foreach (var key in tag.Keys) {
                if (_pattern.IsMatch(key))
                    return true;
            }

            return false;
        }

        public static Tag GetId (Tag tag)
        {
            if (tag is TagCompound) {
                TagCompound ctag = tag as TagCompound;
                foreach (var kv in ctag) {
                    if (_pattern.IsMatch(kv.Key))
                        return kv.Value;
                }
            }
            if (tag is TagString)
                return tag;

            return null;
        }

        public static Tag FindObjectWithId (TagList tag, object id)
        {
            foreach (Tag o in tag) {
                if (id.Equals(GetId(o)))
                    return o;
            }

            return null;
        }

        public static int FindIndexWithId (TagList tag, object id)
        {
            for (int i = 0; i < tag.Count; i++) {
                if (id.Equals(GetId(tag[i])))
                    return i;
            }

            return -1;
        }
    }

    public abstract class DiffOperation
    {
        public abstract void Write (StringBuilder builder, string context);

        public abstract void Apply (TagCompound tag, string key);
        public abstract void Apply (TagList tag, int key);

        static public DiffOperation Merge (DiffOperation left, DiffOperation right)
        {
            // Remove always trumps other operations
            if (right is RemoveOperation)
                return right;
            if (left is RemoveOperation)
                return left;

            // Change operation trumps other operations and right changes take precedent
            if (right is ChangeOperation)
                return right;
            if (left is ChangeOperation)
                return left;

            // Recurseively merge array and object ops
            Debug.Assert(left.GetType() == right.GetType());
            if (left is ChangeObjectOperation)
                return ChangeObjectOperation.Merge(left as ChangeObjectOperation, right as ChangeObjectOperation);
            if (left is ChangePositionArrayOperation)
                return ChangePositionArrayOperation.Merge(left as ChangePositionArrayOperation, right as ChangePositionArrayOperation);
            if (left is ChangeIdArrayOperation)
                return ChangeIdArrayOperation.Merge(left as ChangeIdArrayOperation, right as ChangeIdArrayOperation);

            Debug.Assert(false);
            return null;
        }
    }

    class RemoveOperation : DiffOperation
    {
        public override void Write (StringBuilder builder, string key)
        {
            builder.AppendFormat("{0} = null\n", key);
        }

        public override void Apply (TagCompound tag, string key)
        {
            tag.Remove(key);
        }

        public override void Apply (TagList tag, int key)
        {
            tag[key] = null;
        }
    }

    class ChangeOperation : DiffOperation
    {
        Tag _value;

        public ChangeOperation (Tag value)
        {
            _value = value;
        }

        public override void Write (StringBuilder builder, string key)
        {
            builder.AppendFormat("{0} = {1}\n", key, _value);
        }

        public override void Apply (TagCompound tag, string key)
        {
            tag[key] = _value;
        }

        public override void Apply (TagList tag, int key)
        {
            while (key >= tag.Count)
                tag.Add(null);
            tag[key] = _value;
        }
    }

    class ChangeObjectOperation : DiffOperation
    {
        public CompoundDiff Diff { get; set; }

        public ChangeObjectOperation (CompoundDiff diff)
        {
            Diff = diff;
        }

        public override void Write (StringBuilder builder, string key)
        {
            Diff.Write(builder, key);
        }

        public override void Apply (TagCompound tag, string key)
        {
            Diff.Apply(tag[key] as TagCompound);
        }

        public override void Apply (TagList tag, int key)
        {
            Diff.Apply(tag[key] as TagCompound);
        }

        public static ChangeObjectOperation Merge (ChangeObjectOperation left, ChangeObjectOperation right)
        {
            return new ChangeObjectOperation(CompoundDiff.Merge(left.Diff, right.Diff));
        }
    }

    class ChangeIdArrayOperation : DiffOperation
    {
        public CompoundDiff Diff { get; set; }

        public ChangeIdArrayOperation (CompoundDiff diff)
        {
            Diff = diff;
        }

        public override void Write (StringBuilder builder, string key)
        {
            Diff.Write(builder, key);
        }

        public override void Apply (TagCompound tag, string key)
        {
            Diff.ApplyId(tag[key] as TagList);
            FilterNull(tag[key] as TagList);
        }

        public override void Apply (TagList tag, int key)
        {
            Diff.ApplyId(tag[key] as TagList);
            FilterNull(tag[key] as TagList);
        }

        private void FilterNull (TagList tag)
        {
            for (int i = 0; i < tag.Count; i++) {
                if (tag[i] == null) {
                    tag.RemoveAt(i);
                    i--;
                }
            }
        }

        public static ChangeIdArrayOperation Merge (ChangeObjectOperation left, ChangeObjectOperation right)
        {
            return new ChangeIdArrayOperation(CompoundDiff.Merge(left.Diff, right.Diff));
        }
    }

    class ChangePositionArrayOperation : DiffOperation
    {
        public ListDiff Diff { get; set; }

        public ChangePositionArrayOperation (ListDiff diff)
        {
            Diff = diff;
        }

        public override void Write (StringBuilder builder, string key)
        {
            Diff.Write(builder, key);
        }

        public override void Apply (TagCompound tag, string key)
        {
            Diff.Apply(tag[key] as TagList);
            FilterNull(tag[key] as TagList);
        }

        public override void Apply (TagList tag, int key)
        {
            Diff.Apply(tag[key] as TagList);
            FilterNull(tag[key] as TagList);
        }

        private void FilterNull (TagList tag)
        {
            for (int i = 0; i < tag.Count; i++) {
                if (tag[i] == null) {
                    tag.RemoveAt(i);
                    i--;
                }
            }
        }

        public static ChangePositionArrayOperation Merge (ChangePositionArrayOperation left, ChangePositionArrayOperation right)
        {
            return new ChangePositionArrayOperation(ListDiff.Merge(left.Diff, right.Diff));
        }
    }

    public class CompoundDiff
    {
        public Dictionary<object, DiffOperation> Operations = new Dictionary<object, DiffOperation>();

        public void Write (StringBuilder builder, params string[] context)
        {
            string c = context.Length > 0 ? context[0] + "." : "";
            foreach (var kv in Operations)
                kv.Value.Write(builder, c + kv.Key);
        }

        public void Apply (TagCompound tag)
        {
            foreach (var kv in Operations)
                kv.Value.Apply(tag, kv.Key as string);
        }

        public void ApplyId (TagList tag)
        {
            foreach (var kv in Operations) {
                int index = Id.FindIndexWithId(tag, kv.Key);
                if (index == -1) {
                    tag.Add(null);
                    index = tag.Count - 1;
                }
                kv.Value.Apply(tag, index);
            }
        }

        public bool IsEmpty
        {
            get { return Operations.Count == 0; }
        }

        public static CompoundDiff Merge (CompoundDiff left, CompoundDiff right)
        {
            // Find combined key list
            List<object> keys = left.Operations.Keys.ToList<object>();
            foreach (var kv in right.Operations)
                if (!left.Operations.ContainsKey(kv.Key))
                    keys.Add(kv.Key);

            // Merge the operations for each key.
            CompoundDiff diff = new CompoundDiff();
            foreach (object key in keys) {
                if (left.Operations.ContainsKey(key) && right.Operations.ContainsKey(key))
                    diff.Operations[key] = DiffOperation.Merge(left.Operations[key], right.Operations[key]);
                else if (left.Operations.ContainsKey(key))
                    diff.Operations[key] = left.Operations[key];
                else if (right.Operations.ContainsKey(key))
                    diff.Operations[key] = right.Operations[key];
            }

            return diff;
        }
    }

    public class ListDiff
    {
        public Dictionary<int, DiffOperation> Operations = new Dictionary<int, DiffOperation>();

        public void Write (StringBuilder builder, params string[] context)
        {
            string c = context.Length > 0 ? context[0] : "";
            foreach (var kv in Operations)
                kv.Value.Write(builder, c + "[" + kv.Key + "]");
        }

        public void Apply (TagList tag)
        {
            foreach (var kv in Operations)
                kv.Value.Apply(tag, kv.Key);
        }

        public bool IsEmpty
        {
            get { return Operations.Count == 0; }
        }

        public static ListDiff Merge (ListDiff left, ListDiff right)
        {
            List<int> keys = left.Operations.Keys.ToList<int>();
            foreach (var kv in right.Operations)
                if (!left.Operations.ContainsKey(kv.Key))
                    keys.Add(kv.Key);

            ListDiff diff = new ListDiff();
            foreach (int key in keys) {
                if (left.Operations.ContainsKey(key) && right.Operations.ContainsKey(key))
                    diff.Operations[key] = DiffOperation.Merge(left.Operations[key], right.Operations[key]);
                else if (left.Operations.ContainsKey(key))
                    diff.Operations[key] = left.Operations[key];
                else if (right.Operations.ContainsKey(key))
                    diff.Operations[key] = right.Operations[key];
            }

            return diff;
        }
    }

    public static class NbtMerge
    {
        private static DiffOperation DiffValue (Tag first, Tag second)
        {
            if (first == null && second == null)
                return null;
            else if (second == null)
                return new RemoveOperation();
            else if (first == null)
                return new ChangeOperation(second);
            else if (first.TagType != second.TagType)
                return new ChangeOperation(second);
            else {
                switch (first.TagType) {
                    case TagType.Byte:
                    case TagType.Short:
                    case TagType.Int:
                    case TagType.Long:
                    case TagType.Float:
                    case TagType.Double:
                    case TagType.String:
                    case TagType.ByteArray:
                    case TagType.IntArray:
                        return first.DataEqual(second) ? null : new ChangeOperation(second);
                    case TagType.Compound:
                        CompoundDiff compDiff = Diff(first as TagCompound, second as TagCompound);
                        return compDiff.IsEmpty ? null : new ChangeObjectOperation(compDiff);
                    case TagType.List:
                        if (AreIdArrays(first as TagList, second as TagList)) {
                            CompoundDiff subDiff = IdDiff(first as TagList, second as TagList);
                            return subDiff.IsEmpty ? null : new ChangeIdArrayOperation(subDiff);
                        }
                        else {
                            ListDiff subDiff = PositionDiff(first as TagList, second as TagList);
                            return subDiff.IsEmpty ? null : new ChangePositionArrayOperation(subDiff);
                        }
                    default:
                        throw new Exception("Unexpected tag type");
                }
            }
        }

        private static bool AreIdArrays (TagList a, TagList b)
        {
            bool allContainsId = true;
            bool allString = true;

            foreach (Tag tag in a) {
                if (tag.TagType != TagType.String)
                    allString = false;
                if (tag.TagType != TagType.Compound || !Id.ContainsId(tag as TagCompound))
                    allContainsId = false;
            }

            foreach (Tag tag in b) {
                if (tag.TagType != TagType.String)
                    allString = false;
                if (tag.TagType != TagType.Compound || !Id.ContainsId(tag as TagCompound))
                    allContainsId = false;
            }

            return allContainsId || allString;
        }

        public static CompoundDiff Diff (TagCompound first, TagCompound second)
        {
            CompoundDiff diff = new CompoundDiff();

            foreach (var kv in first) {
                Tag firstValue = kv.Value;
                Tag secondValue = second.ContainsKey(kv.Key) ? second[kv.Key] : null;

                DiffOperation op = DiffValue(firstValue, secondValue);
                if (op != null)
                    diff.Operations[kv.Key] = op;
            }

            foreach (var kv in second) {
                if (!first.ContainsKey(kv.Key))
                    diff.Operations[kv.Key] = new ChangeOperation(kv.Value);
            }

            return diff;
        }

        public static CompoundDiff IdDiff (TagList a, TagList b)
        {
            CompoundDiff diff = new CompoundDiff();

            for (int i = 0; i < a.Count; i++) {
                object id = Id.GetId(a[i]);
                DiffOperation op = DiffValue(a[i], Id.FindObjectWithId(b, id));
                if (op != null)
                    diff.Operations[id] = op;
            }

            for (int i = 0; i < b.Count; i++) {
                object id = Id.GetId(b[i]);
                object aObject = Id.FindObjectWithId(a, id);
                if (aObject != null)
                    diff.Operations[id] = new ChangeOperation(b[i]);
            }

            return diff;
        }

        public static ListDiff PositionDiff (TagList a, TagList b)
        {
            ListDiff diff = new ListDiff();

            int n = Math.Max(a.Count, b.Count);
            for (int i = 0; i < n; i++) {
                Tag firstValue = (i >= a.Count) ? null : a[i];
                Tag secondValue = (i >= b.Count) ? null : b[i];

                DiffOperation op = DiffValue(firstValue, secondValue);
                if (op != null)
                    diff.Operations[i] = op;
            }

            return diff;
        }

        public static TagCompound Merge (TagCompound parent, TagCompound left, TagCompound right)
        {
            CompoundDiff leftDiff = Diff(parent, left);
            CompoundDiff rightDiff = Diff(parent, right);
            CompoundDiff diff = CompoundDiff.Merge(leftDiff, rightDiff);

            TagCompound res = parent.DeepCopy();

            diff.Apply(res);
            return res;
        }

        public static TagCompound MergeDetailed (TagCompound parent, TagCompound left, TagCompound right, 
            out CompoundDiff leftDiff, out CompoundDiff rightDiff, out CompoundDiff mergedDiff)
        {
            leftDiff = Diff(parent, left);
            rightDiff = Diff(parent, right);
            mergedDiff = CompoundDiff.Merge(leftDiff, rightDiff);

            TagCompound res = parent.DeepCopy();

            mergedDiff.Apply(res);
            return res;
        }
    }
}
