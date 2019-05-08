using System;

namespace Stagger
{
    public class CTBTagger : Tagger
    {
        private readonly string[] openTagArray = { "VA", "VC", "VE", "VV", "NR", "NT", "NN", "AD", "FW", "CD", "OD", "IJ", "JJ" };

        public CTBTagger(TaggedData taggedData, int posBeamSize, int neBeamSize) : base(taggedData, posBeamSize, neBeamSize)
        {
        }

        protected override void ComputeOpenTags()
        {
            OpenTags = new int[openTagArray.Length];

            TagSet tagSet = TaggedData.PosTagSet;

            for (int i = 0; i < OpenTags.Length; i++)
            {
                try
                {
                    OpenTags[i] = tagSet.GetTagId(openTagArray[i]);
                }
                catch (TagNameException)
                {
                    Console.WriteLine($"Open tag not in tag set '{openTagArray[i]}'.");

                    Environment.Exit(1);
                }
            }
        }

        protected override int GetPosFeatures(TaggedToken[] sentence, int index, int[] features, double[] values, int featuresCount, int posTag, int neTag, int neTypeTag, bool hasLast, History history, bool extend)
        {
            char[] head = new char[8];

            int id;

            TaggedToken token = sentence[index];

            char isFinal = (index == sentence.Length - 1) ? (char)1 : (char)0;

            string textLower = token.LowerCaseText;

            string lastLower = (index == 0) ? "" : sentence[index - 1].LowerCaseText;

            string lastLower2 = (index < 2) ? "" : sentence[index - 2].LowerCaseText;

            string nextLower = (index == sentence.Length - 1) ? "" : sentence[index + 1].LowerCaseText;

            string nextLower2 = (index >= sentence.Length - 2) ? "" : sentence[index + 2].LowerCaseText;

            if (!hasLast)
            {
                // POS + textLower + final?
                head[0] = (char)0x00;

                head[1] = (char)posTag;

                head[2] = isFinal;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 3) + textLower, extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + lastLower
                head[0] = (char)0x01;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower}\n{textLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + nextLower
                head[0] = (char)0x02;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{textLower}\n{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + nextLower + nextLower2
                head[0] = (char)0x03;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{textLower}\n{nextLower}\n{nextLower2}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower + textLower + nextLower
                head[0] = (char)0x04;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower}\n{textLower}\n{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower2 + lastLower + textLower
                head[0] = (char)0x05;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower2}\n{lastLower}\n{textLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + suffixes
                head[0] = (char)0x06;

                head[1] = (char)posTag;

                for (int i = textLower.Length - 4; i < textLower.Length; i++)
                {
                    if (i < 1) continue;

                    id = PosPerceptron.GetFeatureId(new string(head, 0, 2) + textLower.Substring(i), extend);

                    if (id >= 0)
                    {
                        features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                    }
                }


                // POS + characters
                head[0] = (char)0x07;

                head[1] = (char)posTag;

                head[2] = (char)textLower.Length;

                for (int i = 0; i < textLower.Length; i++)
                {
                    head[3] = (char)i;

                    head[4] = textLower[i];

                    id = PosPerceptron.GetFeatureId(new string(head, 0, 5) + textLower.Substring(i), extend);

                    if (id >= 0)
                    {
                        features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                    }
                }


                // POS + dictionary
                head[0] = (char)0x08;

                head[1] = (char)posTag;

                for (int i = 0; i < PosDictionaries.Count; i++)
                {
                    Dictionary dict = PosDictionaries[i];

                    string value = dict.Map[textLower];

                    string nextValue = (i == sentence.Length - 1) ? "" : dict.Map[nextLower];

                    head[2] = (char)i;

                    string[] combinations = { value, (value == null || nextValue == null) ? null : $"{value}\n{nextValue}", nextValue };

                    for (int j = 0; j < combinations.Length; j++)
                    {
                        if (combinations[j] == null) continue;

                        head[3] = (char)j;

                        id = PosPerceptron.GetFeatureId(new string(head, 0, 4) + combinations[j], extend);

                        if (id >= 0)
                        {
                            features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                        }
                    }
                }


                // POS + embedding
                head[0] = (char)0x09;

                head[1] = (char)posTag;

                for (int i = 0; i < PosEmbeddings.Count; i++)
                {
                    float[] value = PosEmbeddings[i].Map[textLower];

                    if (value == null) continue;

                    head[2] = (char)i;

                    for (int j = 0; j < value.Length; j++)
                    {
                        head[3] = (char)j;

                        id = PosPerceptron.GetFeatureId(new string(head, 0, 4), extend);

                        if (id >= 0)
                        {
                            features[featuresCount] = id; values[featuresCount] = value[j]; featuresCount++;
                        }
                    }
                }
            }
            else
            {
                char posTag1B = (char)0xffff;

                char posTag2B = (char)0xffff;

                if (history != null)
                {
                    posTag1B = (char)history.PosTag;

                    if (history.Last != null) posTag2B = (char)history.Last.PosTag;
                }


                // (previous, current) POS
                head[0] = (char)0x80;

                head[1] = (char)posTag;

                head[2] = posTag1B;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 3), extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // (previous2, previous, current) POS
                head[0] = (char)0x81;

                head[1] = (char)posTag;

                head[2] = posTag1B;

                head[3] = posTag2B;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 4), extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // (previous, current) POS + textLower
                head[0] = (char)0x82;

                head[1] = (char)posTag;

                head[2] = posTag1B;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 3) + textLower, extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // (previous, current) POS + textLower + nextLower
                head[0] = (char)0x83;

                head[1] = (char)posTag;

                head[2] = posTag1B;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 3)}{textLower}\n{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }
            }

            return featuresCount;
        }
    }
}