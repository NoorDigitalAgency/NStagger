using System;
using System.Collections.Generic;
using System.Linq;

namespace Stagger
{
    [Serializable]
    public abstract class FeatureMap
    {
        private readonly Dictionary<string, int> featureMap;

        protected FeatureMap()
        {
            featureMap = new Dictionary<string, int>();
        }

        public int Size => featureMap.Count;

        public int GetFeatureId(string feature, bool extend)
        {
            if (!featureMap.ContainsKey(feature))
            {
                if (extend)
                {
                    return AddFeature(feature);
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                return featureMap[feature];
            }
        }

        protected int AddFeature(string feature)
        {
            int size = Size;

            featureMap.Add(feature, size);

            NewFeature(size);

            return size;
        }

        protected void PruneFeatures()
        {
            int toId = 0;

            foreach (KeyValuePair<string, int> pair in featureMap.ToArray())
            {
                string feature = pair.Key;

                int fromId = pair.Value;

                if (KeepFeatureId(fromId))
                {
                    if (fromId != toId)
                    {
                        featureMap[feature] = toId;

                        CopyFeature(fromId, toId);
                    }

                    toId++;
                }
                else
                {
                    featureMap.Remove(feature);
                }
            }
        }

        protected abstract void CopyFeature(int fromId, int toId);

        protected abstract bool KeepFeatureId(int id);

        protected abstract void NewFeature(int id);
    }
}