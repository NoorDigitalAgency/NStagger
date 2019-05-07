using System;

namespace Stagger
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
            if (!(featuresCount >= 0 && featuresCount <= features.Length && featuresCount <= values.Length))
            {
                throw new Exception("Invalid index of features list or value list specified.");
            }

            if (updateCounts == null)
            {
                throw new Exception("Update Count List not set.");
            }

            if (currentWeights == null)
            {
                throw new Exception("Current Weight List not set.");
            }

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

            if (!(featuresCount >= 0 && featuresCount <= features.Length && featuresCount <= values.Length))
            {
                throw new Exception("Invalid index of features list or value list specified.");
            }

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
            bestWeights = new double[Size];

            bestUpdateCounts = new int[Size];

            Array.Copy(sumWeights, bestWeights, Math.Min(Size, sumWeights.Length));

            Array.Copy(updateCounts, bestUpdateCounts, Math.Min(Size, updateCounts.Length));
        }

        public void EndTraining()
        {
            if (bestWeights != null)
            {
                sumWeights = new double[bestWeights.Length];

                Array.Copy(bestWeights, sumWeights, bestWeights.Length);

                updateCounts = new int[bestUpdateCounts.Length];

                Array.Copy(bestUpdateCounts, updateCounts, bestUpdateCounts.Length);
            }

            bestWeights = null;

            bestUpdateCounts = null;

            oldSumWeights = new double[Size];

            Array.Copy(sumWeights, oldSumWeights, Math.Min(Size, sumWeights.Length));

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