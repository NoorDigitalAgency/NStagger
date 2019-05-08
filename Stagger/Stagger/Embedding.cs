using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Stagger
{
    [Serializable]
    public class Embedding
    {
        public Dictionary<string, float[]> Map;

        public Embedding()
        {
            Map = new Dictionary<string, float[]>();
        }

        public void FromStreamReader(StreamReader reader)
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string[] fields = line.Split('\t');

                Debug.Assert(fields.Length >= 2);

                float[] values = new float[fields.Length - 1];

                for (int i = 0; i < values.Length; i++)
                {
                    values[i] = float.Parse(fields[i + 1]);
                }

                Map[fields[0].ToLower()] = values;
            }
        }

        public void FromFile(string filePath)
        {
            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                FromStreamReader(reader);

                reader.Close();
            }
        }

        public void Rescale(float sigma)
        {
            double t0 = 0.0, t1 = 0.0, t2 = 0.0;

            foreach (float[] values in Map.Values)
            {
                t0 += values.Length;

                foreach (float value in values)
                {
                    t1 += value;
                }
            }

            double avg = t1 / t0;

            foreach (float[] values in Map.Values)
            {
                t0 += values.Length;

                foreach (float value in values)
                {
                    double d = value - avg;

                    t2 += d * d;
                }
            }

            double variance = t2 / t0;

            double stdDev = Math.Sqrt(variance);

            float scale = sigma / (float)stdDev;

            foreach (float[] values in Map.Values)
            {
                for (int i = 0; i < values.Length; i++)
                {
                    values[i] *= scale;
                }
            }
        }
    }
}