using System;
using System.Diagnostics;

namespace NStagger
{
    [Serializable]
    public class Perceptron : FeatureMap
    {
        private double[] sumWeights;

        private double[] oldSumWeights;

        private double[] currentWeights;

        private double[] bestWeights;

        private int[] updateCounts;

        private int[] bestUpdateCounts;

        public void UpdateWeights(int[] features, double[] values, int featuresCount, bool positive)
        {
            Debug.Assert(featuresCount >= 0 && featuresCount <= features.Length && featuresCount <= values.Length);

            Debug.Assert(updateCounts != null);

            Debug.Assert(currentWeights != null);

            if (positive)
            {
                for (int i = 0; i < featuresCount; i++)
                {
                    int feature = features[i];
                    
                    updateCounts[feature]++;
                    
                    currentWeights[feature] += values[i];
                }
            }
            else
            {
                for (int i = 0; i < featuresCount; i++)
                {
                    int feature = features[i];
                    
                    updateCounts[feature]++;
                    
                    currentWeights[feature] -= values[i];
                }
            }
        }

        public void UpdateWeights(int[] features, double[] values, bool positive)
        {
            UpdateWeights(features, values, features.Length, positive);
        }

        public double Score(int[] features, double[] values, int featuresCount, bool average)
        {
            double sum = 0.0;

            Debug.Assert(featuresCount >= 0 && featuresCount <= features.Length && featuresCount <= values.Length);

            if (currentWeights == null || average)
            {
                for (int i = 0; i < featuresCount; i++)
                {
                    sum += sumWeights[features[i]] * values[i];
                }
            }
            else
            {
                for (int i = 0; i < featuresCount; i++)
                {
                    sum += currentWeights[features[i]] * values[i];
                }
            }

            return sum;
        }

        public double Score(int[] features, double[] values, bool average)
        {
            return Score(features, values, features.Length, average);
        }

        public void StartTraining()
        {
            sumWeights = new double[0x1000];

            currentWeights = new double[0x1000];

            updateCounts = new int[0x1000];

            bestWeights = null;

            bestUpdateCounts = null;
        }

        public void AccumulateWeights()
        {
            for (int i = 0; i < sumWeights.Length; i++)
            {
                sumWeights[i] += currentWeights[i];
            }
        }

        public void MakeBestWeight()
        {
            bestWeights = Arrays.CopyOf(sumWeights, Size);

            bestUpdateCounts = Arrays.CopyOf(updateCounts, Size);
        }

        public void EndTraining()
        {
            if (bestWeights != null)
            {
                sumWeights = bestWeights;

                updateCounts = bestUpdateCounts;
            }

            bestWeights = null;

            bestUpdateCounts = null;

            oldSumWeights = Arrays.CopyOf(sumWeights, Size);

            PruneFeatures();

            currentWeights = null;

            oldSumWeights = null;

            updateCounts = null;

            Array.Resize(ref sumWeights, Size);
        }

        protected override void CopyFeature(int fromId, int toId)
        {
            sumWeights[toId] = oldSumWeights[fromId];
        }

        protected override bool KeepFeatureId(int id)
        {
            return id < sumWeights.Length && Math.Abs(sumWeights[id]) > double.Epsilon && updateCounts[id] > 0;
        }

        protected override void NewFeature(int id)
        {
            if (id >= sumWeights.Length)
            {
                int newLen = sumWeights.Length + 0x1000;

                Array.Resize(ref sumWeights, newLen);

                Array.Resize(ref currentWeights, newLen);

                Array.Resize(ref updateCounts, newLen);
            }
        }
    }
}