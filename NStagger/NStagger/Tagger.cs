using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NStagger
{
    [Serializable]
    public class Tagger
    {
        protected const int CountLimit = 3;

        protected const int MaximumFeatures = 0x80;

        protected const int AccumulateLimit = 0x1000;

        protected bool TrainingMode;

        protected int[][] TokenTypeTags;

        protected Perceptron PosPerceptron;

        protected Perceptron NePerceptron;

        protected int PosBeamSize;

        protected int NeBeamSize;

        protected int[] OpenTags;

        protected bool HasPos;

        protected HashSet<string> AllowedPrefixes = null;

        protected HashSet<string> AllowedSuffixes = null;

        public TaggedData TaggedData { get; }

        public bool HasNe { get; set; }

        public Lexicon PosLexicon { get; }

        public List<Dictionary> PosDictionaries { get; set; }

        public List<Embedding> PosEmbeddings { get; set; }

        public List<Dictionary> NeDictionaries { get; set; }

        public List<Embedding> NeEmbeddings { get; set; }

        public bool ExtendLexicon { get; set; } = true;

        public int MaximumPosIterations { get; set; } = 16;

        public int MaximumNeIterations { get; set; } = 16;

        public Tagger(TaggedData taggedData, int posBeamSize, int neBeamSize)
        {
            TrainingMode = false;

            HasPos = false;

            HasNe = false;

            PosPerceptron = null;

            NePerceptron = null;

            TaggedData = taggedData;

            PosBeamSize = posBeamSize;

            NeBeamSize = neBeamSize;

            PosDictionaries = new List<Dictionary>();

            PosEmbeddings = new List<Embedding>();

            NeDictionaries = new List<Dictionary>();

            NeEmbeddings = new List<Embedding>();

            PosLexicon = new Lexicon();

            // ReSharper disable once VirtualMemberCallInConstructor
            ComputeOpenTags();
        }

        protected virtual string GetLemma(TaggedToken token)
        {
            return null;
        }

        public virtual void BuildLexicons(TaggedToken[][] sentences)
        {
            const int types = (int)TokenType.Types;

            bool[,] hasTag = new bool[types, TaggedData.PosTagSet.Size];

            foreach (TaggedToken[] sentence in sentences)
            {
                foreach (TaggedToken token in sentence)
                {
                    if (token.PosTag >= 0)
                    {
                        hasTag[(int)token.Token.Type, token.PosTag] = true;

                        PosLexicon.AddEntry(token.Token.Value, token.Lemma, token.PosTag, 1);
                    }
                }
            }

            TokenTypeTags = new int[types][];

            for (int tokenType = 0; tokenType < types; tokenType++)
            {
                int tagsCount = OpenTags.Count(openTag => hasTag[tokenType, openTag]);

                if (tagsCount == 0)
                {
                    TokenTypeTags[tokenType] = OpenTags;
                }
                else
                {
                    TokenTypeTags[tokenType] = new int[tagsCount];

                    int j = 0;

                    foreach (int openTag in OpenTags)
                    {
                        if (hasTag[tokenType, openTag])
                        {
                            TokenTypeTags[tokenType][j++] = openTag;
                        }
                    }

                    Debug.Assert(j == tagsCount);

                    for (int k = 0; k < j - 1; k++)
                    {
                        Debug.Assert(TokenTypeTags[tokenType][k] < TokenTypeTags[tokenType][k + 1]);
                    }
                }
            }
        }

        protected virtual void ComputeOpenTags()
        {
            OpenTags = new int[TaggedData.PosTagSet.Size];

            for (int i = 0; i < OpenTags.Length; i++)
            {
                OpenTags[i] = i;
            }
        }

        public virtual void Train(TaggedToken[][] trainSentences, TaggedToken[][] developmentSentences)
        {
            HasPos = false;

            HasNe = false;

            foreach (TaggedToken[] sentence in trainSentences)
            {
                foreach (TaggedToken token in sentence)
                {
                    if (token.PosTag >= 0 && MaximumPosIterations > 0)
                    {
                        HasPos = true;
                    }

                    if (token.NeTag >= 0 && MaximumNeIterations > 0)
                    {
                        HasNe = true;
                    }
                }
            }

            TrainingMode = true;

            if (HasPos)
            {
                PosPerceptron = new Perceptron();

                TrainPos(trainSentences, developmentSentences);
            }

            if (HasNe)
            {
                NePerceptron = new Perceptron();

                TrainNe(trainSentences, developmentSentences);
            }

            TrainingMode = false;
        }

        protected void TrainPos(TaggedToken[][] trainSentences, TaggedToken[][] developmentSentences)
        {
            PosPerceptron.StartTraining();

            List<int> trainOrder = new List<int>(trainSentences.Length);

            for (int i = 0; i < trainSentences.Length; i++)
            {
                trainOrder.Add(i);
            }

            int bestIterations = 0;

            double bestAccuracy = 0.0;

            for (int iterations = 0; iterations < MaximumPosIterations; iterations++)
            {
                Console.WriteLine($"Starting POS iteration {iterations}");

                int tokensCount = 0;

                Evaluation trainEvaluation = new Evaluation();

                foreach (int sentenceIndex in trainOrder)
                {
                    TaggedToken[] trainSent = trainSentences[sentenceIndex];

                    if (trainSent.Length == 0 || trainSent[0].PosTag < 0)
                    {
                        continue;
                    }

                    TaggedToken[] taggedSent = new TaggedToken[trainSent.Length];

                    for (int i = 0; i < trainSent.Length; i++)
                    {
                        taggedSent[i] = new TaggedToken(trainSent[i]);
                    }

                    TagPos(taggedSent, false);

                    int oldPosCorrect = trainEvaluation.PosCorrect;

                    trainEvaluation.Evaluate(taggedSent, trainSent);

                    if (trainEvaluation.PosCorrect != oldPosCorrect + trainSent.Length)
                    {
                        PosUpdateWeights(taggedSent, trainSent);
                    }

                    tokensCount += trainSent.Length;

                    if (tokensCount > AccumulateLimit)
                    {
                        PosPerceptron.AccumulateWeights();

                        tokensCount = 0;
                    }
                }

                Console.WriteLine($"Training set accuracy: {trainEvaluation.GetPosAccuracy()}");

                if (developmentSentences == null)
                {
                    if (iterations == MaximumPosIterations - 1)
                    {
                        PosPerceptron.MakeBestWeight();
                    }

                    continue;
                }

                Evaluation developmentEvaluation = new Evaluation();

                foreach (TaggedToken[] developmentSentence in developmentSentences)
                {
                    TaggedToken[] taggedSentence = new TaggedToken[developmentSentence.Length];

                    for (int i = 0; i < developmentSentence.Length; i++)
                    {
                        taggedSentence[i] = new TaggedToken(developmentSentence[i]);
                    }

                    TrainingMode = false;

                    TagPos(taggedSentence, true);

                    TrainingMode = true;

                    developmentEvaluation.Evaluate(taggedSentence, developmentSentence);
                }

                double developmentAccuracy = developmentEvaluation.GetPosAccuracy();

                Console.WriteLine($"Development set accuracy: {developmentAccuracy}");

                if ((developmentAccuracy - bestAccuracy) / developmentAccuracy > 0.00025)
                {
                    bestAccuracy = developmentAccuracy;

                    bestIterations = iterations;

                    PosPerceptron.MakeBestWeight();
                }
                else if (developmentAccuracy > bestAccuracy)
                {
                    PosPerceptron.MakeBestWeight();
                }
                else if (bestIterations <= iterations - 3)
                {
                    Console.WriteLine("Accuracy not increasing, we are done.");

                    break;
                }

            }

            PosPerceptron.EndTraining();
        }

        private void PosUpdateWeights(TaggedToken[] taggedSentence, TaggedToken[] trainSentence)
        {
            Debug.Assert(taggedSentence.Length == trainSentence.Length);

            History[] taggedHistory = SentenceToHistory(taggedSentence);

            History[] trainHistory = SentenceToHistory(trainSentence);

            int[] features = new int[MaximumFeatures];

            double[] values = new double[MaximumFeatures];

            for (int i = 0; i < taggedSentence.Length; i++)
            {
                History tagged = taggedHistory[i];

                History train = trainHistory[i];

                int featuresCount = GetPosFeatures(taggedSentence, i, features, values, 0, tagged.PosTag, tagged.NeTag, tagged.NeTypeTag, false, null, true);

                featuresCount = GetPosFeatures(taggedSentence, i, features, values, featuresCount, tagged.PosTag, tagged.NeTag, tagged.NeTypeTag, true, tagged.Last, true);

                PosPerceptron.UpdateWeights(features, values, featuresCount, false);

                featuresCount = GetPosFeatures(trainSentence, i, features, values, 0, train.PosTag, train.NeTag, train.NeTypeTag, false, null, true);

                featuresCount = GetPosFeatures(trainSentence, i, features, values, featuresCount, train.PosTag, train.NeTag, train.NeTypeTag, true, train.Last, true);

                PosPerceptron.UpdateWeights(features, values, featuresCount, true);
            }
        }

        public History[] SentenceToHistory(TaggedToken[] sentence)
        {
            History[] history = new History[sentence.Length];

            for (int i = 0; i < sentence.Length; i++)
            {
                TaggedToken token = sentence[i];

                history[i] = new History(token.Token.Value, token.LowerCaseText, token.Lemma, token.PosTag, token.NeTag, token.NeTypeTag, 0.0, (i == 0) ? null : history[i - 1]);
            }

            return history;
        }

        public TaggedToken[] TagSentence(TaggedToken[] sentence, bool average, bool preserve)
        {
            TaggedToken[] taggedSentence = new TaggedToken[sentence.Length];

            for (int i = 0; i < sentence.Length; i++)
            {
                taggedSentence[i] = new TaggedToken(sentence[i]) { PosTag = -1 };
            }

            if (HasPos)
            {
                TagPos(taggedSentence, average);
            }

            for (int i = 0; i < sentence.Length; i++)
            {
                if (preserve && sentence[i].PosTag >= 0)
                {
                    taggedSentence[i].PosTag = sentence[i].PosTag;
                }
            }

            if (HasNe)
            {
                TagNe(taggedSentence, average);
            }

            for (int i = 0; i < sentence.Length; i++)
            {
                if (preserve && sentence[i].NeTag >= 0)
                {
                    taggedSentence[i].NeTag = sentence[i].NeTag;

                    taggedSentence[i].NeTypeTag = sentence[i].NeTypeTag;
                }

                if ((!preserve) || taggedSentence[i].Lemma == null)
                {
                    taggedSentence[i].Lemma = GetLemma(taggedSentence[i]);
                }
            }

            return taggedSentence;
        }

        protected void TagPos(TaggedToken[] sentence, bool average)
        {
            History[] beam = new History[PosBeamSize];

            History[] nextBeam = new History[PosBeamSize];

            int[] features = new int[MaximumFeatures];

            double[] values = new double[MaximumFeatures];

            beam[0] = null;

            int beamUsed = 1;

            for (int i = 0; i < sentence.Length; i++)
            {
                TaggedToken taggedToken = sentence[i];

                string text = taggedToken.Token.Value;

                string textLower = taggedToken.LowerCaseText;

                var nextBeamUsed = 0;

                int[] possibleTags = PossiblePosTags(sentence, i);

                int neTag = sentence[i].NeTag;

                int neTypeTag = sentence[i].NeTypeTag;

                Debug.Assert(possibleTags.Length > 0);

                foreach (int posTag in possibleTags)
                {
                    int localFeaturesCount = GetPosFeatures(sentence, i, features, values, 0, posTag, neTag, neTypeTag, false, null, false);

                    for (int j = 0; j < beamUsed; j++)
                    {
                        History beamHistory = beam[j];

                        int featuresCount = GetPosFeatures(sentence, i, features, values, localFeaturesCount, posTag, neTag, neTypeTag, true, beamHistory, false);

                        double score = PosPerceptron.Score(features, values, featuresCount, average);

                        if (beamHistory != null)
                        {
                            score += beamHistory.Score;
                        }

                        if (nextBeamUsed == 0)
                        {
                            nextBeam[0] = new History(text, textLower, taggedToken.Lemma, posTag, neTag, neTypeTag, score, beamHistory);

                            nextBeamUsed = 1;
                        }
                        else
                        {
                            if (score > nextBeam[nextBeamUsed - 1].Score)
                            {
                                int l = nextBeamUsed - 1;

                                if (nextBeamUsed < PosBeamSize)
                                {
                                    nextBeam[l + 1] = nextBeam[l];

                                    nextBeamUsed++;
                                }

                                l--;

                                while (l >= 0 && score > nextBeam[l].Score)
                                {
                                    nextBeam[l + 1] = nextBeam[l];

                                    l--;
                                }

                                nextBeam[l + 1] = new History(text, textLower, taggedToken.Lemma, posTag, neTag, neTypeTag, score, beamHistory);
                            }
                            else if (nextBeamUsed < PosBeamSize)
                            {
                                nextBeam[nextBeamUsed++] = new History(text, textLower, taggedToken.Lemma, posTag, neTag, neTypeTag, score, beamHistory);
                            }
                        }
                    }
                }

                Array.Copy(nextBeam, 0, beam, 0, nextBeamUsed);

                beamUsed = nextBeamUsed;
            }

            History history = beam[0];

            for (int i = 0; i < sentence.Length; i++)
            {
                Debug.Assert(history != null);

                sentence[sentence.Length - (i + 1)].PosTag = history.PosTag;

                history = history.Last;
            }

            Debug.Assert(history == null);
        }

        protected virtual void GuessTags(string wordForm, bool firstWord)
        {
        }

        protected int[] PossiblePosTags(TaggedToken[] sentence, int index)
        {
            string textLower = sentence[index].LowerCaseText;

            if (!TrainingMode)
            {
                GuessTags(sentence[index].Token.Value, (index == 0));
            }

            Entry[] entries = PosLexicon.GetEntries(textLower);

            if (entries == null)
            {
                return TokenTypeTags[(int)sentence[index].Token.Type];
            }

            int[] tags = new int[entries.Length];

            int tagsCount = 0;

            int lastTag = -1;

            int seenCount = entries.Sum(entry => entry.NumberOfOccurence);

            foreach (Entry entry in entries)
            {
                if (seenCount > 0 && !ExtendLexicon && entry.NumberOfOccurence == 0)
                {
                    continue;
                }

                if (entry.TagId != lastTag)
                {
                    tags[tagsCount++] = entry.TagId;

                    lastTag = entry.TagId;
                }
            }

            for (int t = 0; t < tagsCount - 1; t++)
            {
                Debug.Assert(tags[t] < tags[t + 1]);
            }

            if (!TrainingMode || seenCount >= CountLimit)
            {
                return tagsCount != tags.Length ? Arrays.CopyOf(tags, tagsCount) : tags;
            }

            int[] possibleTags = TokenTypeTags[(int)sentence[index].Token.Type];

            int[] lexiconTags = tags;

            int i = 0, j = 0, k = 0;

            tags = new int[tagsCount + possibleTags.Length];

            for (; j < possibleTags.Length && k < tagsCount; i++)
            {
                if (possibleTags[j] < lexiconTags[k])
                {
                    tags[i] = possibleTags[j++];
                }
                else if (possibleTags[j] == lexiconTags[k])
                {
                    tags[i] = possibleTags[j++]; k++;
                }
                else
                {
                    tags[i] = lexiconTags[k++];
                }
            }

            if (j < possibleTags.Length)
            {
                for (; j < possibleTags.Length; j++) tags[i++] = possibleTags[j];
            }
            else
            {
                for (; k < tagsCount; k++) tags[i++] = lexiconTags[k];
            }

            tagsCount = i;

            for (int t = 0; t < tagsCount - 1; t++)
            {
                Debug.Assert(tags[t] < tags[t + 1]);
            }

            return tagsCount != tags.Length ? Arrays.CopyOf(tags, tagsCount) : tags;
        }

        protected void TrainNe(TaggedToken[][] trainSentences, TaggedToken[][] developmentSentences)
        {
            NePerceptron.StartTraining();

            List<int> trainOrder = new List<int>(trainSentences.Length);

            for (int i = 0; i < trainSentences.Length; i++)
            {
                trainOrder.Add(i);
            }

            int bestIterations = 0;

            double bestAccuracy = 0.0;

            for (int iterations = 0; iterations < MaximumNeIterations; iterations++)
            {
                Console.WriteLine($"Starting NE iteration {iterations}");

                int tokenCount = 0;

                Evaluation trainEvaluation = new Evaluation();

                foreach (int sentenceIndex in trainOrder)
                {
                    TaggedToken[] trainSentence = trainSentences[sentenceIndex];

                    if (trainSentence.Length == 0 || trainSentence[0].NeTag < 0)
                    {
                        continue;
                    }

                    TaggedToken[] taggedSentence = new TaggedToken[trainSentence.Length];

                    for (int i = 0; i < trainSentence.Length; i++)
                    {
                        taggedSentence[i] = new TaggedToken(trainSentence[i]);
                    }

                    TagNe(taggedSentence, false);

                    trainEvaluation.Evaluate(taggedSentence, trainSentence);

                    if (!trainEvaluation.CheckNesEqual(taggedSentence, trainSentence))
                    {
                        NeUpdateWeights(taggedSentence, trainSentence);
                    }

                    tokenCount += trainSentence.Length;

                    if (tokenCount > AccumulateLimit)
                    {
                        NePerceptron.AccumulateWeights();

                        tokenCount = 0;
                    }
                }

                Console.WriteLine($"Training set F-score: {trainEvaluation.GetNeFScore()}");

                if (developmentSentences == null)
                {
                    if (iterations == MaximumNeIterations - 1)
                    {
                        NePerceptron.MakeBestWeight();
                    }

                    continue;
                }

                Evaluation developmentEvaluation = new Evaluation();

                foreach (TaggedToken[] developmentSent in developmentSentences)
                {
                    TaggedToken[] taggedSentence = new TaggedToken[developmentSent.Length];

                    for (int i = 0; i < developmentSent.Length; i++)
                    {
                        taggedSentence[i] = new TaggedToken(developmentSent[i]);
                    }

                    TrainingMode = false;

                    TagNe(taggedSentence, true);

                    TrainingMode = true;

                    developmentEvaluation.Evaluate(taggedSentence, developmentSent);
                }

                double developmentAccuracy = developmentEvaluation.GetNeFScore();

                Console.WriteLine($"Development set F-Score: {developmentAccuracy}");

                if ((developmentAccuracy - bestAccuracy) / developmentAccuracy > 0.00025)
                {
                    bestAccuracy = developmentAccuracy;

                    bestIterations = iterations;

                    NePerceptron.MakeBestWeight();
                }
                else if (bestIterations <= iterations - 3)
                {
                    Console.WriteLine("F-score not increasing, we are done.");

                    break;
                }

            }

            NePerceptron.EndTraining();
        }

        private void NeUpdateWeights(TaggedToken[] taggedSentence, TaggedToken[] trainSentence)
        {
            Debug.Assert(taggedSentence.Length == trainSentence.Length);

            History[] taggedHistory = SentenceToHistory(taggedSentence);

            History[] trainHistory = SentenceToHistory(trainSentence);

            int[] features = new int[MaximumFeatures];

            double[] values = new double[MaximumFeatures];

            for (int i = 0; i < taggedSentence.Length; i++)
            {
                History tagged = taggedHistory[i];

                History train = trainHistory[i];

                int featuresCount = GetNeFeatures(taggedSentence, i, features, values, 0, tagged.PosTag, tagged.NeTag, tagged.NeTypeTag, tagged.Last, true);

                NePerceptron.UpdateWeights(features, values, featuresCount, false);

                featuresCount = GetNeFeatures(trainSentence, i, features, values, 0, train.PosTag, train.NeTag, train.NeTypeTag, train.Last, true);

                NePerceptron.UpdateWeights(features, values, featuresCount, true);
            }
        }

        private void TagNe(TaggedToken[] sentence, bool average)
        {
            History[] beam = new History[NeBeamSize];

            History[] nextBeam = new History[NeBeamSize];

            int[] features = new int[MaximumFeatures];

            double[] values = new double[MaximumFeatures];

            beam[0] = null;

            int beamUsed = 1;

            for (int i = 0; i < sentence.Length; i++)
            {
                TaggedToken token = sentence[i];

                string text = token.Token.Value;

                string textLower = token.LowerCaseText;

                var nextBeamUsed = 0;

                int posTag = sentence[i].PosTag;

                for (int neTag = 0; neTag < TaggedData.NeTags; neTag++)
                {
                    if (i == 0 && neTag == TaggedData.NeI)
                    {
                        continue;
                    }

                    for (int j = 0; j < beamUsed; j++)
                    {
                        History beamHistory = beam[j];

                        if ((beamHistory == null || beamHistory.NeTag == TaggedData.NeO) && neTag == TaggedData.NeI)
                        {
                            continue;
                        }

                        int minType = -1, maxType = -1;

                        if (neTag == TaggedData.NeI)
                        {
                            Debug.Assert(beamHistory != null);

                            minType = beamHistory.NeTypeTag;

                            maxType = beamHistory.NeTypeTag;
                        }
                        else if (neTag == TaggedData.NeB)
                        {
                            minType = 0;

                            maxType = TaggedData.NeTypeTagSet.Size - 1;
                        }

                        for (int neTypeTag = minType; neTypeTag <= maxType; neTypeTag++)
                        {
                            int nFeats = GetNeFeatures(sentence, i, features, values, 0, posTag, neTag, neTypeTag, beamHistory, false);

                            double score = NePerceptron.Score(features, values, nFeats, average);

                            if (beamHistory != null)
                            {
                                score += beamHistory.Score;
                            }

                            if (nextBeamUsed == 0)
                            {
                                nextBeam[0] = new History(text, textLower, token.Lemma, posTag, neTag, neTypeTag, score, beamHistory);

                                nextBeamUsed = 1;
                            }
                            else
                            {
                                if (score > nextBeam[nextBeamUsed - 1].Score)
                                {
                                    int l = nextBeamUsed - 1;

                                    if (nextBeamUsed < NeBeamSize)
                                    {
                                        nextBeam[l + 1] = nextBeam[l];

                                        nextBeamUsed++;
                                    }

                                    l--;

                                    while (l >= 0 && score > nextBeam[l].Score)
                                    {
                                        nextBeam[l + 1] = nextBeam[l];

                                        l--;
                                    }

                                    nextBeam[l + 1] = new History(text, textLower, token.Lemma, posTag, neTag, neTypeTag, score, beamHistory);
                                }
                                else if (nextBeamUsed < NeBeamSize)
                                {
                                    nextBeam[nextBeamUsed++] = new History(text, textLower, token.Lemma, posTag, neTag, neTypeTag, score, beamHistory);
                                }
                            }
                        }
                    }
                }

                Array.Copy(nextBeam, 0, beam, 0, nextBeamUsed);

                beamUsed = nextBeamUsed;
            }

            History history = beam[0];

            for (int i = 0; i < sentence.Length; i++)
            {
                Debug.Assert(history != null);

                sentence[sentence.Length - (i + 1)].NeTag = history.NeTag;

                sentence[sentence.Length - (i + 1)].NeTypeTag = history.NeTypeTag;

                history = history.Last;
            }

            Debug.Assert(history == null);
        }

        protected virtual int GetPosFeatures(TaggedToken[] sentence, int index, int[] features, double[] values, int featuresCount, int posTag, int neTag, int neTypeTag, bool hasLast, History history, bool extend)
        {
            char[] head = new char[8];

            int id;

            TaggedToken token = sentence[index];

            char isInitial = (index == 0) ? (char)1 : (char)0;

            char isFinal = (index == sentence.Length - 1) ? (char)1 : (char)0;

            char capitalization = token.Token.IsCapitalized ? (char)1 : (char)0;

            char tokenType = (char)token.Token.Type;

            char tokenType1A = (index == sentence.Length - 1) ? (char)0xffff : (char)sentence[index + 1].Token.Type;

            string text = token.Token.Value;

            string textLower = token.LowerCaseText;

            string nextText = (index == sentence.Length - 1) ? "" : sentence[index + 1].Token.Value;

            string nextText2 = (index >= sentence.Length - 2) ? "" : sentence[index + 2].Token.Value;

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


                // POS + textLower + capitalization + initial?
                head[0] = (char)0x01;

                head[1] = (char)posTag;

                head[2] = capitalization;

                head[3] = isInitial;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 4) + textLower, extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + lastLower
                head[0] = (char)0x02;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower}\n{textLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + nextLower
                head[0] = (char)0x03;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{textLower}\n{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + textLower + nextLower + nextLower2
                head[0] = (char)0x04;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{textLower}\n{nextLower}\n{nextLower2}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower + textLower + nextLower
                head[0] = (char)0x05;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower}\n{textLower}\n{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower2 + lastLower + textLower
                head[0] = (char)0x06;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower2}\n{lastLower}\n{textLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower
                head[0] = (char)0x07;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + lastLower2
                head[0] = (char)0x08;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{lastLower2}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + nextLower
                head[0] = (char)0x09;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{nextLower}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + nextLower2
                head[0] = (char)0x0a;

                head[1] = (char)posTag;

                id = PosPerceptron.GetFeatureId($"{new string(head, 0, 2)}{nextLower2}", extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + prefixes + capitalization + initial?
                head[0] = (char)0x10;

                head[1] = (char)posTag;

                head[2] = capitalization;

                head[3] = isInitial;

                for (int i = 1; i <= 4 && i < textLower.Length; i++)
                {
                    string prefix = textLower.Substring(0, i);

                    if (AllowedPrefixes == null || AllowedPrefixes.Contains(prefix))
                    {
                        id = PosPerceptron.GetFeatureId(new string(head, 0, 4) + prefix, extend);

                        if (id >= 0)
                        {
                            features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                        }
                    }
                }


                // POS + suffixes + capitalization + initial?
                head[0] = (char)0x11;

                head[1] = (char)posTag;

                head[2] = capitalization;

                head[3] = isInitial;

                for (int i = textLower.Length - 5; i < textLower.Length; i++)
                {
                    if (i < 2) continue;

                    string suffix = textLower.Substring(i);

                    if (AllowedSuffixes == null || AllowedSuffixes.Contains(suffix))
                    {
                        id = PosPerceptron.GetFeatureId(new string(head, 0, 4) + suffix, extend);

                        if (id >= 0)
                        {
                            features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                        }
                    }
                }


                // POS + dictionary
                head[0] = (char)0x12;

                head[1] = (char)posTag;

                for (int i = 0; i < PosDictionaries.Count; i++)
                {
                    Dictionary dictionary = PosDictionaries[i];

                    string value = dictionary.Map[text];

                    string nextValue = (i == sentence.Length - 1) ? "" : dictionary.Map[nextText];

                    string nextValue2 = (i >= sentence.Length - 2) ? "" : dictionary.Map[nextText2];

                    head[2] = (char)i;

                    string[] combinations = { value, (value == null || nextValue == null) ? null : $"{value}\n{nextValue}", nextValue, (nextValue == null || nextValue2 == null) ? null : $"{nextValue}\n{nextValue2}", nextValue2 };

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
                head[0] = (char)0x13;

                head[1] = (char)posTag;

                for (int i = 0; i < PosEmbeddings.Count; i++)
                {
                    if (!PosEmbeddings[i].Map.ContainsKey(textLower))
                    {
                        continue;
                    }

                    float[] value = PosEmbeddings[i].Map[textLower];

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


                // POS + token type + contains dash?
                head[0] = (char)0x20;

                head[1] = (char)posTag;

                head[2] = tokenType;

                head[3] = (char)(textLower.Contains("-") ? 1 : 0);

                id = PosPerceptron.GetFeatureId(new string(head, 0, 4), extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


                // POS + (current, next) token type
                head[0] = (char)0x21;

                head[1] = (char)posTag;

                head[2] = tokenType;

                head[3] = tokenType1A;

                id = PosPerceptron.GetFeatureId(new string(head, 0, 4), extend);

                if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }
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


                // (previous, current) POS + dictionary
                head[0] = (char)0x84;

                head[1] = (char)posTag;

                head[2] = posTag1B;

                for (int i = 0; i < PosDictionaries.Count; i++)
                {
                    Dictionary dictionary = PosDictionaries[i];

                    string nextValue = (i == sentence.Length - 1) ? null : (dictionary.Map.ContainsKey(nextText) ? dictionary.Map[nextText] : null);

                    if (nextValue == null) continue;

                    head[3] = (char)i;

                    id = PosPerceptron.GetFeatureId(new string(head, 0, 4) + nextValue, extend);

                    if (id >= 0)
                    {
                        features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                    }
                }
            }

            return featuresCount;
        }

        protected int GetNeFeatures(TaggedToken[] sentence, int index, int[] features, double[] values, int featuresCount, int posTag, int neTag, int neTypeTag, History history, bool extend)
        {
            char[] head = new char[8];

            TaggedToken token = sentence[index];

            char tokenType = (char)token.Token.Type;

            int posTag1B = (index == 0) ? 0xffff : sentence[index - 1].PosTag;

            int posTag1A = (index == sentence.Length - 1) ? 0xffff : sentence[index + 1].PosTag;

            string textLower = token.LowerCaseText;

            string lastLower = (index == 0) ? "" : sentence[index - 1].LowerCaseText;

            string nextLower = (index == sentence.Length - 1) ? "" : sentence[index + 1].LowerCaseText;


            // tag + type + POS
            head[0] = (char)0x00;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            head[3] = (char)posTag;

            int id = NePerceptron.GetFeatureId(new string(head, 0, 4), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // tag + type + (previous, current) POS
            head[0] = (char)0x01;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            head[3] = (char)posTag;

            head[4] = (char)posTag1B;

            id = NePerceptron.GetFeatureId(new string(head, 0, 5), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // tag + type + (current, next) POS
            head[0] = (char)0x02;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            head[3] = (char)posTag;

            head[4] = (char)posTag1A;

            id = NePerceptron.GetFeatureId(new string(head, 0, 5), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // tag + type + textLower
            head[0] = (char)0x03;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            id = NePerceptron.GetFeatureId(new string(head, 0, 3) + textLower, extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // tag + type + textLower + nextLower
            head[0] = (char)0x04;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            id = NePerceptron.GetFeatureId($"{new string(head, 0, 3)}{textLower}\n{nextLower}", extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // tag + type + lastLower + textLower
            head[0] = (char)0x04;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            id = NePerceptron.GetFeatureId($"{new string(head, 0, 3)}{lastLower}\n{textLower}", extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // dictionaries
            head[0] = (char)0x08;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            for (int i = 0; i < NeDictionaries.Count; i++)
            {
                Dictionary dictionary = NeDictionaries[i];

                string value = dictionary.Map[textLower];

                string lastValue = (i == 0) ? "" : dictionary.Map[lastLower];

                string nextValue = (i == sentence.Length - 1) ? "" : dictionary.Map[nextLower];

                head[3] = (char)i;

                string[] combinations = { value, (value == null || lastValue == null) ? null : $"{lastValue}\n{value}", (value == null || nextValue == null) ? null : $"{value}\n{nextValue}", nextValue };

                for (int j = 0; j < combinations.Length; j++)
                {
                    if (combinations[j] == null) continue;

                    head[4] = (char)j;

                    id = NePerceptron.GetFeatureId(new string(head, 0, 5) + combinations[j], extend);

                    if (id >= 0)
                    {
                        features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++;
                    }
                }
            }


            // embeddings
            head[0] = (char)0x09;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            for (int i = 0; i < NeEmbeddings.Count; i++)
            {
                if (!NeEmbeddings[i].Map.ContainsKey(textLower))
                {
                    continue;
                }

                float[] value = NeEmbeddings[i].Map[textLower];

                head[3] = (char)i;

                for (int j = 0; j < value.Length; j++)
                {
                    head[4] = (char)j;

                    id = NePerceptron.GetFeatureId(new string(head, 0, 5), extend);

                    if (id >= 0)
                    {
                        features[featuresCount] = id; values[featuresCount] = value[j]; featuresCount++;
                    }
                }
            }


            // tag + type + token type
            head[0] = (char)0x0a;

            head[1] = (char)neTag;

            head[2] = (char)neTypeTag;

            head[3] = tokenType;

            id = NePerceptron.GetFeatureId(new string(head, 0, 4), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }

            char neTag1B = (char)0xffff;

            char neTag2B = (char)0xffff;

            if (history != null)
            {
                neTag1B = (char)history.NeTag;

                if (history.Last != null) neTag2B = (char)history.Last.NeTag;
            }


            // (previous, current) tag + type
            head[0] = (char)0x80;

            head[1] = (char)neTag;

            head[2] = neTag1B;

            head[3] = (char)neTypeTag;

            id = NePerceptron.GetFeatureId(new string(head, 0, 4), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            // (previous, current) tag + type
            head[0] = (char)0x81;

            head[1] = (char)neTag;

            head[2] = neTag1B;

            head[3] = neTag2B;

            head[4] = (char)neTypeTag;

            id = NePerceptron.GetFeatureId(new string(head, 0, 5), extend);

            if (id >= 0) { features[featuresCount] = id; values[featuresCount] = 1.0; featuresCount++; }


            return featuresCount;
        }
    }
}