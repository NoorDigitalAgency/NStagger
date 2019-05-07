using System;

namespace Stagger
{
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

        public void Evaluate(TaggedToken[] sent, TaggedToken[] goldSent)
        {
            if (sent.Length != goldSent.Length)
            {
                throw new Exception("Length of the Sentence and Gold Sentence are different.");
            }

            for (int i = 0; i < sent.Length; i++)
            {
                if (sent[i].PosTag >= 0 && goldSent[i].PosTag >= 0)
                {
                    PosTotal++;

                    if (sent[i].PosTag == goldSent[i].PosTag)
                    {
                        PosCorrect++;
                    }
                }

                if (goldSent[i].NeTag == TaggedData.NeB)
                {
                    NeTotal++;
                }

                if (sent[i].NeTag == TaggedData.NeB)
                {
                    NeFound++;

                    if (goldSent[i].NeTag == TaggedData.NeB && goldSent[i].NeTypeTag == sent[i].NeTypeTag)
                    {
                        for (int j = i + 1; j < sent.Length; j++)
                        {
                            if (goldSent[i].NeTag != TaggedData.NeI)
                            {
                                if (sent[i].NeTag != TaggedData.NeI)
                                {
                                    NeCorrect++;
                                }

                                break;
                            }
                            else if (sent[i].NeTag != TaggedData.NeI)
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

        public double GetNeScore()
        {
            double precision = GetNePrecision();

            double recall = GetNeRecall();

            return Math.Abs(precision) < double.Epsilon && Math.Abs(recall) < double.Epsilon ? 0.0 : 2.0 * precision * recall / (precision + recall);
        }

        public bool AreNesEqual(TaggedToken[] sent, TaggedToken[] goldSent)
        {
            if (sent.Length != goldSent.Length)
            {
                throw new Exception("Length of the Sentence and Gold Sentence are different.");
            }

            for (int i = 0; i < sent.Length; i++)
            {
                if (sent[i].NeTag != goldSent[i].NeTag)
                {
                    return false;
                }

                if (sent[i].NeTypeTag != goldSent[i].NeTypeTag)
                {
                    return false;
                }
            }

            return true;
        }
    }
}