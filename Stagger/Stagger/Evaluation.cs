using System;
using System.Diagnostics;

namespace Stagger
{
    [Serializable]
    public class Evaluation
    {
        public int PosTotal { get; private set; }

        public int PosCorrect { get; private set; }

        public int NeTotal { get; private set; }

        public int NeCorrect { get; private set; }

        public int NeFound { get; private set; }

        public Evaluation()
        {
            PosTotal = 0;

            PosCorrect = 0;

            NeTotal = 0;

            NeCorrect = 0;

            NeFound = 0;
        }

        public void Evaluate(TaggedToken[] sentence, TaggedToken[] goldSentence)
        {
            Debug.Assert(sentence.Length == goldSentence.Length);

            for (int i = 0; i < sentence.Length; i++)
            {
                if (sentence[i].PosTag >= 0 && goldSentence[i].PosTag >= 0)
                {
                    PosTotal++;

                    if (sentence[i].PosTag == goldSentence[i].PosTag)
                    {
                        PosCorrect++;
                    }
                }

                if (goldSentence[i].NeTag == TaggedData.NeB)
                {
                    NeTotal++;
                }

                if (sentence[i].NeTag == TaggedData.NeB)
                {
                    NeFound++;

                    if (goldSentence[i].NeTag == TaggedData.NeB && goldSentence[i].NeTypeTag == sentence[i].NeTypeTag)
                    {
                        for (int j = i + 1; j < sentence.Length; j++)
                        {
                            if (goldSentence[i].NeTag != TaggedData.NeI)
                            {
                                if (sentence[i].NeTag != TaggedData.NeI)
                                {
                                    NeCorrect++;
                                }

                                break;
                            }
                            else if (sentence[i].NeTag != TaggedData.NeI)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        public double GetPosAccuracy()
        {
            return PosTotal == 0 ? 0.0 : PosCorrect / (double)PosTotal;
        }

        public double GetNePrecision()
        {
            return NeFound == 0 ? 0.0 : NeCorrect / (double)NeFound;
        }

        public double GetNeRecall()
        {
            return NeTotal == 0 ? 0.0 : NeCorrect / (double)NeTotal;
        }

        public double GetNeFScore()
        {
            double precision = GetNePrecision();

            double recall = GetNeRecall();

            return Math.Abs(precision) < double.Epsilon && Math.Abs(recall) < double.Epsilon ? 0.0 : 2.0 * precision * recall / (precision + recall);
        }

        public bool CheckNesEqual(TaggedToken[] sentence, TaggedToken[] goldSentence)
        {
            Debug.Assert(sentence.Length == goldSentence.Length);

            for (int i = 0; i < sentence.Length; i++)
            {
                if (sentence[i].NeTag != goldSentence[i].NeTag)
                {
                    return false;
                }

                if (sentence[i].NeTypeTag != goldSentence[i].NeTypeTag)
                {
                    return false;
                }
            }

            return true;
        }
    }
}