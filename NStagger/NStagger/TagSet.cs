using System;
using System.Collections.Generic;

namespace NStagger
{
    [Serializable]
    public class TagSet
    {
        protected Dictionary<string, int> TagIds;

        public List<string> TagNames { get; }

        public TagSet()
        {
            TagIds = new Dictionary<string, int>();

            TagNames = new List<string>();
        }

        public int Size => TagNames.Count;

        public int GetTagId(string name)
        {
            if (!TagIds.ContainsKey(name))
            {
                throw new TagNameException($"Unknown Tag Name '{name}'.");
            }

            return TagIds[name];
        }

        public int AddTag(string name)
        {
            if (TagNames.Contains(name)) return TagIds[name];

            int newId = TagIds.Count;

            TagIds[name] = newId;

            TagNames.Add(name);

            return newId;
        }

        public int GetTagId(string name, bool extend)
        {
            return extend ? AddTag(name) : GetTagId(name);
        }

        public string GetTagName(int id)
        {
            if (id < 0 || id >= TagNames.Count)
            {
                throw new TagNameException($"Invalid Tag ID '{id}'.");
            }

            return TagNames[id];
        }
    }
}
