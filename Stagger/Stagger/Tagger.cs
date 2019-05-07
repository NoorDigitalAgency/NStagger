using System;
using System.Collections.Generic;
using System.Linq;

namespace Stagger
{
    public class Tagger
    {
        // During training, words with fewer occurences than this in the training
        // set are considered unknown. Empirically, 3 seems to be a good value.
        protected const int countLimit = 3;
        // If true, the countLimit value is used to create "unknown" words.
        protected bool trainingMode;
        // A list of POS tags each word form can take.
        protected Lexicon posLexicon;
        // Lists of possible POS tags for each token type
        protected int[][] tokenTypeTags;
        // Perceptrons for the two tasks: POS and NER.
        protected Perceptron posPerceptron;
        protected Perceptron nePerceptron;
        // Reading the POS and NER tag formats.
        protected TaggedData taggedData;
        // Beam sizes for the tagging algorithms.
        protected int posBeamSize;
        protected int neBeamSize;
        // Word representations for the two tasks.
        protected List<Dictionary> posDictionaries;
        protected List<Embedding> posEmbeddings;
        protected List<Dictionary> neDictionaries;
        protected List<Embedding> neEmbeddings;
        // Array of POS tags that unknown words can have.
        protected int[] openTags;
        // Whether or not this tagger performs the given task.
        protected bool hasPos, hasNe;
        // Should known words be extended?
        protected bool extendLexicon = true;

        // Maximum number of training iterations.
        protected int maximumPosIterations = 16;
        protected int maximumNeIterations = 16;
        // Maximum number of features per decision, don't be cheap here.
        protected const int maxFeats = 0x80;
        // Interval (in tokens) at which to accumulate weight vector.
        protected const int accumulateLimit = 0x1000;

        protected HashSet<string> allowedPrefixes = null;
        protected HashSet<string> allowedSuffixes = null;

        protected void setMaxPosIters(int n)
        {
            maximumPosIterations = n;
        }

        protected void setMaxNEIters(int n)
        {
            maximumNeIterations = n;
        }

        public void setExtendLexicon(bool x)
        {
            extendLexicon = x;
        }

        public void setHasNE(bool x)
        {
            hasNe = x;
        }

        /**
         * Creates a new tagger.
         *
         * train() will initialize the tagger further.
         */
        public Tagger(TaggedData taggedData, int posBeamSize, int neBeamSize)
        {
            trainingMode = false;
            hasPos = false;
            hasNe = false;
            posPerceptron = null;
            nePerceptron = null;
            this.taggedData = taggedData;
            this.posBeamSize = posBeamSize;
            this.neBeamSize = neBeamSize;
            posDictionaries = new List<Dictionary>();
            posEmbeddings = new List<Embedding>();
            neDictionaries = new List<Dictionary>();
            neEmbeddings = new List<Embedding>();
            posLexicon = new Lexicon();
            computeOpenTags();
        }

        public Lexicon getPosLexicon()
        {
            return posLexicon;
        }

        public void setPosDictionaries(List<Dictionary> dictionaries)
        {
            posDictionaries = dictionaries;
        }

        public void setNEDictionaries(List<Dictionary> dictionaries)
        {
            neDictionaries = dictionaries;
        }

        public void setPosEmbeddings(List<Embedding> embeddings)
        {
            posEmbeddings = embeddings;
        }

        public void setNEEmbeddings(List<Embedding> embeddings)
        {
            neEmbeddings = embeddings;
        }

        /**
         * Returns the TaggedData instance used by this tagger.
         *
         * @return  TaggedData instance
         */
        public TaggedData getTaggedData()
        {
            return taggedData;
        }

        /**
         * Annotates a token with its lemma form, given its POS tag.
         *
         * The default is to not use lemmas at all, but inflectional languages
         * should override this method.
         */
        protected string getLemma(TaggedToken token)
        {
            return null;
        }

        /**
         * Constructs POS tag lexicon, and generalized token lexicon.
         */
        public void buildLexicons(TaggedToken[][] sentences)
        {
            const int types = (int)TokenType.Types;

            bool[,] hasTag = new bool[types, taggedData.PosTagSet.Size];

            foreach (TaggedToken[] sentence in sentences)
            {
                foreach (TaggedToken token in sentence)
                {
                    if (token.PosTag >= 0)
                    {
                        hasTag[(int)token.Token.Type, token.PosTag] = true;

                        posLexicon.AddEntry(token.Token.Value, token.Lemma, token.PosTag, 1);
                    }
                }
            }

            tokenTypeTags = new int[types][];

            for (int tokenType = 0; tokenType < types; tokenType++)
            {
                int tagsCount = openTags.Count(openTag => hasTag[tokenType, openTag]);

                if (tagsCount == 0)
                {
                    tokenTypeTags[tokenType] = openTags;
                }
                else
                {
                    tokenTypeTags[tokenType] = new int[tagsCount];

                    int j = 0;

                    foreach (int openTag in openTags)
                    {
                        if (hasTag[tokenType, openTag])
                        {
                            tokenTypeTags[tokenType][j++] = openTag;
                        }
                    }

                    if (j != tagsCount)
                    {
                        throw new Exception("Incorrect number of Tags.");
                    }

                    for (int k = 0; k < j - 1; k++)
                    {
                        if (!(tokenTypeTags[tokenType][k] < tokenTypeTags[tokenType][k + 1]))
                        {
                            throw new Exception("Incorrect Tags order.");
                        }
                    }
                }
            }
        }

        protected void computeOpenTags()
        {
            openTags = new int[taggedData.PosTagSet.Size];

            for (int i = 0; i < openTags.Length; i++)
            {
                openTags[i] = i;
            }
        }

        public void train(TaggedToken[][] trainSentences, TaggedToken[][] testSentences)
        {
            hasPos = false;

            hasNe = false;

            foreach (TaggedToken[] sentence in trainSentences)
            {
                foreach (TaggedToken token in sentence)
                {
                    if (token.PosTag >= 0 && maximumPosIterations > 0)
                    {
                        hasPos = true;
                    }

                    if (token.NeTag >= 0 && maximumNeIterations > 0)
                    {
                        hasNe = true;
                    }
                }
            }

            trainingMode = true;

            if (hasPos)
            {
                posPerceptron = new Perceptron();

                trainPos(trainSentences, testSentences);
            }

            if (hasNe)
            {
                nePerceptron = new Perceptron();

                trainNE(trainSentences, testSentences);
            }

            trainingMode = false;
        }

        protected void trainPos(TaggedToken[][] trainSentences, TaggedToken[][] testSentences)
        {
            posPerceptron.StartTraining();

            List<int> trainOrder = new List<int>(trainSentences.Length);

            for (int i = 0; i < trainSentences.Length; i++)
            {
                trainOrder.Add(i);
            }

            int bestIterations = 0;

            double bestAccuracy = 0.0;

            for (int iterations = 0; iterations < maximumPosIterations; iterations++)
            {
                Console.WriteLine($"Starting POS iteration {iterations}");

                int tokensCount = 0;

                Evaluation trainEvaluation = new Evaluation();

                for (int sentIdx : trainOrder)
                {
                    TaggedToken[] trainSent = trainSentences[sentIdx];
                    // If the sentence is not POS tagged, skip it
                    if (trainSent.Length == 0 || trainSent[0].PosTag < 0)
                        continue;
                    TaggedToken[] taggedSent = new TaggedToken[trainSent.Length];
                    for (int i = 0; i < trainSent.Length; i++)
                        taggedSent[i] = new TaggedToken(trainSent[i]);
                    tagPos(taggedSent, false);
                    int oldPosCorrect = trainEvaluation.PosCorrect;
                    trainEvaluation.Evaluate(taggedSent, trainSent);
                    // Only perform weight updates if the sentence was incorrectly
                    // tagged
                    if (trainEvaluation.PosCorrect !=
                       oldPosCorrect + trainSent.Length)
                    {
                        posUpdateWeights(taggedSent, trainSent);
                    }
                    // Check if it is time to accumulate perceptron weights.
                    tokensCount += trainSent.Length;
                    if (tokensCount > accumulateLimit)
                    {
                        posPerceptron.AccumulateWeights();
                        tokensCount = 0;
                    }
                }
                System.err.println("Training set accuracy: " +
                    trainEvaluation.GetPosAccuracy());

                if (testSentences == null)
                {
                    if (iterations == maximumPosIterations - 1) posPerceptron.MakeBestWeight();
                    continue;
                }

                Evaluation devEvaluation = new Evaluation();
                for (int sentIdx = 0; sentIdx < testSentences.Length; sentIdx++)
                {
                    TaggedToken[] devSent = testSentences[sentIdx];
                    TaggedToken[] taggedSent = new TaggedToken[devSent.Length];
                    for (int i = 0; i < devSent.Length; i++)
                    {
                        taggedSent[i] = new TaggedToken(devSent[i]);
                    }
                    trainingMode = false;
                    tagPos(taggedSent, true);
                    trainingMode = true;
                    devEvaluation.Evaluate(taggedSent, devSent);
                }
                double devAccuracy = devEvaluation.GetPosAccuracy();
                System.err.println("Development set accuracy: " + devAccuracy);
                if ((devAccuracy - bestAccuracy) / devAccuracy > 0.00025)
                {
                    bestAccuracy = devAccuracy;
                    bestIterations = iterations;
                    posPerceptron.MakeBestWeight();
                }
                else if (devAccuracy > bestAccuracy)
                {
                    posPerceptron.MakeBestWeight();
                }
                else if (bestIterations <= iterations - 3)
                {
                    System.err.println("Accuracy not increasing, we are done.");
                    break;
                }

            }
            posPerceptron.EndTraining();
        }

        /**
         * Update the weights according to the target and model taggings.
         */
        private void posUpdateWeights(
        TaggedToken[] taggedSent, TaggedToken[] trainSent)
        {
            assert(taggedSent.Length == trainSent.Length);
            History[] taggedHistory = sentToHistory(taggedSent);
            History[] trainHistory = sentToHistory(trainSent);
            int[] feats = new int[maxFeats];
            double[] values = new double[maxFeats];
            for (int i = 0; i < taggedSent.Length; i++)
            {
                int nFeats;
                History tagged = taggedHistory[i];
                History train = trainHistory[i];
                // Compute feature values for negative example.
                nFeats = getPosFeats(
                    taggedSent, i, feats, values, 0, tagged.PosTag,
                    tagged.NeTag, tagged.NeTypeTag, false, null, true);
                nFeats = getPosFeats(
                    taggedSent, i, feats, values, nFeats, tagged.PosTag,
                    tagged.NeTag, tagged.NeTypeTag, true, tagged.Last, true);
                posPerceptron.UpdateWeights(feats, values, nFeats, false);

                // TODO: consider caching this
                // Compute feature values for positive example.
                nFeats = getPosFeats(
                    trainSent, i, feats, values, 0, train.PosTag,
                    train.NeTag, train.NeTypeTag, false, null, true);
                nFeats = getPosFeats(
                    trainSent, i, feats, values, nFeats, train.PosTag,
                    train.NeTag, train.NeTypeTag, true, train.Last, true);
                posPerceptron.UpdateWeights(feats, values, nFeats, true);
            }
        }

        /**
         * Creates a linked History vector from a sentence.
         *
         * @param sent      input sentence
         * @return          History array representing the same contents
         */
        public History[] sentToHistory(TaggedToken[] sent)
        {
            History[] history = new History[sent.Length];
            for (int i = 0; i < sent.Length; i++)
            {
                TaggedToken tok = sent[i];
                history[i] = new History(
                    tok.Token.value, tok.LowerCaseText,
                    tok.Lemma, tok.PosTag, tok.NeTag, tok.NeTypeTag, 0.0,
                    (i == 0) ? null : history[i - 1]);
            }
            return history;
        }

        /**
         * Tag a sentence with POS and NE information.
         *
         * @param sentence      input sentence, will not be modified
         * @param average       if true, use the averaged perceptron
         * @param preserve      if true, do not overwrite tags
         * @return              tagged version of input sentence
         */
        public TaggedToken[] tagSentence(
        TaggedToken[] sentence, bool average, bool preserve)
        {
            TaggedToken[] taggedSentence = new TaggedToken[sentence.Length];
            for (int i = 0; i < sentence.Length; i++)
            {
                taggedSentence[i] = new TaggedToken(sentence[i]);
                taggedSentence[i].PosTag = -1;
            }
            if (hasPos) tagPos(taggedSentence, average);
            for (int i = 0; i < sentence.Length; i++)
            {
                if (preserve && sentence[i].PosTag >= 0)
                    taggedSentence[i].PosTag = sentence[i].PosTag;
            }
            if (hasNe) tagNE(taggedSentence, average);
            for (int i = 0; i < sentence.Length; i++)
            {
                if (preserve && sentence[i].NeTag >= 0)
                {
                    taggedSentence[i].NeTag = sentence[i].NeTag;
                    taggedSentence[i].NeTypeTag = sentence[i].NeTypeTag;
                }
                if ((!preserve) || taggedSentence[i].Lemma == null)
                {
                    taggedSentence[i].Lemma = getLemma(taggedSentence[i]);
                }
            }
            return taggedSentence;
        }

        protected void tagPos(TaggedToken[] sentence, bool average)
        {
            History[] beam = new History[posBeamSize];
            History[] nextBeam = new History[posBeamSize];
            int[] feats = new int[maxFeats];
            double[] values = new double[maxFeats];
            beam[0] = null;
            int beamUsed = 1, nextBeamUsed;
            for (int i = 0; i < sentence.Length; i++)
            {
                TaggedToken ttok = sentence[i];
                string text = ttok.Token.value;
                string textLower = ttok.LowerCaseText;
                nextBeamUsed = 0;
                int[] possibleTags = possiblePosTags(sentence, i);
                int neTag = sentence[i].NeTag;
                int neTypeTag = sentence[i].NeTypeTag;
                assert possibleTags.Length > 0;
                /*
                if(!trainingMode) {
                System.out.print(textLower + "   ");
                for(int l=0; l<possibleTags.Length; l++)
                    System.out.print(" " + possibleTags[l]);
                System.out.println("");
                }
                */
                // First, go through all possible tags.
                for (int posTag : possibleTags)
                {
                    // Get history-independent features.
                    int nLocalFeats = getPosFeats(
                        sentence, i, feats, values, 0, posTag, neTag, neTypeTag,
                        false, null, false);
                    // Then go through the available histories.
                    for (int j = 0; j < beamUsed; j++)
                    {
                        History history = beam[j];
                        // Get history-dependent features.
                        int nFeats = getPosFeats(
                            sentence, i, feats, values, nLocalFeats, posTag,
                            neTag, neTypeTag, true, history, false);
                        // Get the score of all features for the local decision.
                        double score = posPerceptron.Score(
                            feats, values, nFeats, average);
                        // Compute the local + history score.
                        if (history != null) score += history.Score;
                        /*
                        if(!trainingMode) {
                            for(int q=0; q<nFeats; q++)
                                System.err.print(feats[q]+"="+values[q]+" ");
                            System.err.println(score);
                        }
                        */
                        // If the beam is empty, always add this decision.
                        if (nextBeamUsed == 0)
                        {
                            nextBeam[0] = new History(
                                text, textLower,
                                ttok.Lemma, posTag, neTag, neTypeTag, score,
                                history);
                            nextBeamUsed = 1;
                        }
                        else
                        {
                            // Otherwise, only add it if the score is higher than
                            // the lowest score currently in the beam.
                            if (score > nextBeam[nextBeamUsed - 1].Score)
                            {
                                int l = nextBeamUsed - 1;
                                // If the beam has space left, make an extra copy
                                // of the smallest element. In the following step
                                // the smallest element will be deleted.
                                if (nextBeamUsed < posBeamSize)
                                {
                                    nextBeam[l + 1] = nextBeam[l];
                                    nextBeamUsed++;
                                }
                                l--;
                                // Move histories with lower scores than the
                                // current one step to the right, until we find
                                // the right place to insert the current history.
                                while (l >= 0 && score > nextBeam[l].Score)
                                {
                                    nextBeam[l + 1] = nextBeam[l];
                                    l--;
                                }
                                // Create and insert the new history.
                                nextBeam[l + 1] = new History(
                                    text, textLower, ttok.Lemma,
                                    posTag, neTag, neTypeTag, score, history);
                            }
                            else if (nextBeamUsed < posBeamSize)
                            {
                                nextBeam[nextBeamUsed++] = new History(
                                    text, textLower, ttok.Lemma,
                                    posTag, neTag, neTypeTag, score, history);
                            }
                        }
                    }
                }
                System.arraycopy(nextBeam, 0, beam, 0, nextBeamUsed);
                beamUsed = nextBeamUsed;
            }
            /*
            if(!trainingMode) {
                for(int i=0; i<beamUsed; i++) {
                    System.out.println("Score of "+i+": "+beam[i].score);
                }
            }
            */
            // Copy the annotation of the best history to the sentence.
            History history = beam[0];
            for (int i = 0; i < sentence.Length; i++)
            {
                sentence[sentence.Length - (i + 1)].PosTag = history.PosTag;
                history = history.Last;
            }
            assert(history == null);
        }

        // May be implemented by a subclass
        protected void guessTags(string wordForm, bool firstWord)
        {
            return;
        }

        /** Returns an array of possible POS tags for a given token.
         *
         * @param tokens        we will look at tokens[idx]
         * @param idx           see above
         * @return              array of possible POS tags
         */
        protected int[] possiblePosTags(TaggedToken[] sent, int idx)
        {
            string textLower = sent[idx].LowerCaseText;
            if (!trainingMode)
                guessTags(sent[idx].Token.value, (idx == 0));

            Lexicon.Entry[] entries = posLexicon.getEntries(textLower);
            if (entries == null)
            {
                return tokenTypeTags[sent[idx].Token.type];
            }
            int[] tags = new int[entries.Length];
            int nTags = 0;
            int nSeen = 0;
            int lastTag = -1;
            for (Lexicon.Entry entry : entries)
                nSeen += entry.n;
            // Go through the list of entries (sorted by tag ID), and put its
            // unique elements into the tag array.
            for (Lexicon.Entry entry : entries)
            {
                // If extendLexicon is false, and this is a known word (as
                // estimated by its non-zero count), then skip any entry that does
                // not occur in the training data.
                if (nSeen > 0 && !extendLexicon && entry.n == 0) continue;
                // Add any tag that has not already been added.
                if (entry.tag != lastTag)
                {
                    tags[nTags++] = entry.tag;
                    lastTag = entry.tag;
                }
            }
            for (int t = 0; t < nTags - 1; t++)
                assert(tags[t] < tags[t + 1]);
            // If the word form is frequent enough (or we are not in training
            // mode), return the lexicon tags directly.
            if (!trainingMode || nSeen >= countLimit)
            {
                if (nTags != tags.Length) return Arrays.copyOf(tags, nTags);
                else return tags;
            }
            // Otherwise, merge the lexicon tags with the array of open tags
            int[] possibleTags = tokenTypeTags[sent[idx].Token.type];
            int[] lexiconTags = tags;
            int i = 0, j = 0, k = 0;
            tags = new int[nTags + possibleTags.Length];
            for (; j < possibleTags.Length && k < nTags; i++)
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
                for (; k < nTags; k++) tags[i++] = lexiconTags[k];
            }
            nTags = i;
            for (int t = 0; t < nTags - 1; t++)
                assert(tags[t] < tags[t + 1]);
            if (nTags != tags.Length) return Arrays.copyOf(tags, nTags);
            else return tags;
        }

        protected void trainNE(
        TaggedToken[][] trainSents, TaggedToken[][] devSents)
        {
            nePerceptron.StartTraining();

            // Create a list of ints 0 to trainSents.Length-1 (inclusive),
            // which will be the order than sentences are processed during a
            // training iteration. This may be permuted at each iteration.
            List<int> trainOrder =
                new List<int>(trainSents.Length);
            for (int i = 0; i < trainSents.Length; i++) trainOrder.Add(new int(i));

            // The peak accuracy on the development set.
            int bestIter = 0;
            double bestAccuracy = 0.0;

            for (int iter = 0; iter < maximumNeIterations; iter++)
            {
                // Randomly reorder the sequence of training sentences.
                // Collections.shuffle(trainOrder);
                System.err.println("Starting NE iteration " + iter);
                // Number of tokens since last weight accumulation.
                int tokenCount = 0;
                Evaluation trainEvaluation = new Evaluation();
                for (int sentIdx : trainOrder)
                {
                    TaggedToken[] trainSent = trainSents[sentIdx];
                    // If this sentence does not contain NE tags, skip it.
                    if (trainSent.Length == 0 || trainSent[0].NeTag < 0)
                        continue;
                    TaggedToken[] taggedSent = new TaggedToken[trainSent.Length];
                    for (int i = 0; i < trainSent.Length; i++)
                        taggedSent[i] = new TaggedToken(trainSent[i]);
                    tagNE(taggedSent, false);
                    trainEvaluation.Evaluate(taggedSent, trainSent);
                    // Only perform weight updates if the sentence was incorrectly
                    // tagged
                    if (!trainEvaluation.AreNesEqual(taggedSent, trainSent))
                    {
                        neUpdateWeights(taggedSent, trainSent);
                    }
                    // Check if it is time to accumulate perceptron weights.
                    tokenCount += trainSent.Length;
                    if (tokenCount > accumulateLimit)
                    {
                        nePerceptron.AccumulateWeights();
                        tokenCount = 0;
                    }
                }
                System.err.println("Training set F-score: " +
                    trainEvaluation.GetNeScore());

                if (devSents == null)
                {
                    if (iter == maximumNeIterations - 1) nePerceptron.MakeBestWeight();
                    continue;
                }

                Evaluation devEvaluation = new Evaluation();
                for (int sentIdx = 0; sentIdx < devSents.Length; sentIdx++)
                {
                    TaggedToken[] devSent = devSents[sentIdx];
                    TaggedToken[] taggedSent = new TaggedToken[devSent.Length];
                    for (int i = 0; i < devSent.Length; i++)
                    {
                        taggedSent[i] = new TaggedToken(devSent[i]);
                    }
                    trainingMode = false;
                    tagNE(taggedSent, true);
                    trainingMode = true;
                    devEvaluation.Evaluate(taggedSent, devSent);
                }
                double devAccuracy = devEvaluation.GetNeScore();
                System.err.println("Development set F-score: " + devAccuracy);
                if ((devAccuracy - bestAccuracy) / devAccuracy > 0.00025)
                {
                    bestAccuracy = devAccuracy;
                    bestIter = iter;
                    nePerceptron.MakeBestWeight();
                }
                else if (bestIter <= iter - 3)
                {
                    System.err.println("F-score not increasing, we are done.");
                    break;
                }

            }
            nePerceptron.EndTraining();
        }

        /**
         * Update the weights according to the target and model taggings.
         *
         * TODO: this could be merged with posUpdateWeights
         */
        private void neUpdateWeights(
        TaggedToken[] taggedSent, TaggedToken[] trainSent)
        {
            assert(taggedSent.Length == trainSent.Length);
            History[] taggedHistory = sentToHistory(taggedSent);
            History[] trainHistory = sentToHistory(trainSent);
            int[] feats = new int[maxFeats];
            double[] values = new double[maxFeats];
            for (int i = 0; i < taggedSent.Length; i++)
            {
                int nFeats;
                History tagged = taggedHistory[i];
                History train = trainHistory[i];
                // Compute feature values for negative example.
                nFeats = getNEFeats(
                    taggedSent, i, feats, values, 0, tagged.PosTag,
                    tagged.NeTag, tagged.NeTypeTag, tagged.Last, true);
                nePerceptron.UpdateWeights(feats, values, nFeats, false);

                // TODO: consider caching this
                // Compute feature values for positive example.
                nFeats = getNEFeats(
                    trainSent, i, feats, values, 0, train.PosTag,
                    train.NeTag, train.NeTypeTag, train.Last, true);
                nePerceptron.UpdateWeights(feats, values, nFeats, true);
            }
        }

        void tagNE(TaggedToken[] sentence, bool average)
        {
            History[] beam = new History[neBeamSize];
            History[] nextBeam = new History[neBeamSize];
            int[] feats = new int[maxFeats];
            double[] values = new double[maxFeats];
            beam[0] = null;
            int beamUsed = 1, nextBeamUsed;
            for (int i = 0; i < sentence.Length; i++)
            {
                TaggedToken ttok = sentence[i];
                string text = ttok.Token.value;
                string textLower = ttok.LowerCaseText;
                nextBeamUsed = 0;
                int posTag = sentence[i].PosTag;
                for (int neTag = 0; neTag < TaggedData.NeTags; neTag++)
                {
                    // Can not start with an I tag.
                    if (i == 0 && neTag == TaggedData.NeI) continue;
                    // Go through the available histories.
                    for (int j = 0; j < beamUsed; j++)
                    {
                        History history = beam[j];
                        // O -> I transitions not allowed
                        if ((history == null ||
                            history.NeTag == TaggedData.NeO) &&
                           neTag == TaggedData.NeI)
                            continue;
                        int minType = -1, maxType = -1;
                        if (neTag == TaggedData.NeI)
                        {
                            minType = history.NeTypeTag;
                            maxType = history.NeTypeTag;
                        }
                        else if (neTag == TaggedData.NeB)
                        {
                            minType = 0;
                            maxType = taggedData.getNETypeTagSet().size() - 1;
                        }
                        for (int neTypeTag = minType; neTypeTag <= maxType; neTypeTag++)
                        {
                            int nFeats = getNEFeats(
                                sentence, i, feats, values, 0,
                                posTag, neTag, neTypeTag, history, false);
                            // Get the score of all features for the local
                            // decision.
                            double score = nePerceptron.Score(
                                feats, values, nFeats, average);
                            // Compute the local + history score.
                            if (history != null) score += history.Score;
                            // If the beam is empty, always add this decision.
                            if (nextBeamUsed == 0)
                            {
                                nextBeam[0] = new History(
                                    text, textLower,
                                    ttok.Lemma, posTag, neTag, neTypeTag, score,
                                    history);
                                nextBeamUsed = 1;
                            }
                            else
                            {
                                // Otherwise, only add it if the score is higher
                                // than the lowest score currently in the beam.
                                if (score > nextBeam[nextBeamUsed - 1].Score)
                                {
                                    int l = nextBeamUsed - 1;
                                    // If the beam has space left, make an extra
                                    // copy of the smallest element. In the
                                    // following step the smallest element will
                                    // be deleted.
                                    if (nextBeamUsed < neBeamSize)
                                    {
                                        nextBeam[l + 1] = nextBeam[l];
                                        nextBeamUsed++;
                                    }
                                    l--;
                                    // Move histories with lower scores than the
                                    // current one step to the right, until we find
                                    // the right place to insert the current
                                    // history.
                                    while (l >= 0 && score > nextBeam[l].Score)
                                    {
                                        nextBeam[l + 1] = nextBeam[l];
                                        l--;
                                    }
                                    // Create and insert the new history.
                                    nextBeam[l + 1] = new History(
                                        text, textLower, ttok.Lemma,
                                        posTag, neTag, neTypeTag, score, history);
                                }
                                else if (nextBeamUsed < neBeamSize)
                                {
                                    nextBeam[nextBeamUsed++] = new History(
                                        text, textLower, ttok.Lemma,
                                        posTag, neTag, neTypeTag, score, history);
                                }
                            }
                        }
                    }
                }
                System.arraycopy(nextBeam, 0, beam, 0, nextBeamUsed);
                beamUsed = nextBeamUsed;
            }
            /*
            if(!trainingMode) {
                for(int i=0; i<beamUsed; i++) {
                    System.out.println("Score of "+i+": "+beam[i].score);
                }
            }
            */
            // Copy the annotation of the best history to the sentence.
            History history = beam[0];
            for (int i = 0; i < sentence.Length; i++)
            {
                sentence[sentence.Length - (i + 1)].NeTag = history.NeTag;
                sentence[sentence.Length - (i + 1)].NeTypeTag = history.NeTypeTag;
                history = history.Last;
            }
            assert(history == null);
        }

        /**
         * Computes feature values given a certain POS tag and context.
         */
        protected int getPosFeats(
        TaggedToken[] sentence, int idx, int[] feats, double[] values, int nFeats,
        int posTag, int neTag, int neTypeTag, bool hasLast, History last,
        bool extend)
        {
            char[] head = new char[8];
            int f;
            TaggedToken ttok = sentence[idx];
            char isInitial = (idx == 0) ? (char)1 : (char)0;
            char isFinal = (idx == sentence.Length - 1) ? (char)1 : (char)0;
            char capitalization = ttok.Token.isCapitalized() ? (char)1 : (char)0;
            char tokType = (char)ttok.Token.type;
            char tokType1a = (idx == sentence.Length - 1) ? 0xffff :
                             (char)sentence[idx + 1].Token.type;
            string text = ttok.Token.value;
            string textLower = ttok.LowerCaseText;
            string nextText =
                (idx == sentence.Length - 1) ? "" : sentence[idx + 1].Token.value;
            string nextText2 =
                (idx >= sentence.Length - 2) ? "" : sentence[idx + 2].Token.value;
            string lastLower = (idx == 0) ? "" : sentence[idx - 1].LowerCaseText;
            string lastLower2 = (idx < 2) ? "" : sentence[idx - 2].LowerCaseText;
            string nextLower =
                (idx == sentence.Length - 1) ? "" : sentence[idx + 1].LowerCaseText;
            string nextLower2 =
                (idx >= sentence.Length - 2) ? "" : sentence[idx + 2].LowerCaseText;

            if (!hasLast)
            {
                // POS + textLower + final?
                head[0] = 0x00;
                head[1] = (char)posTag;
                head[2] = isFinal;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 3) + textLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + textLower + capitalization + initial?
                head[0] = 0x01;
                head[1] = (char)posTag;
                head[2] = capitalization;
                head[3] = isInitial;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 4) + textLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + textLower + lastLower
                head[0] = 0x02;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + lastLower + "\n" + textLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + textLower + nextLower
                head[0] = 0x03;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + textLower + "\n" + nextLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + textLower + nextLower + nextLower2
                head[0] = 0x04;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(new string(head, 0, 2) +
                    textLower + "\n" + nextLower + "\n" + nextLower2, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + lastLower + textLower + nextLower
                head[0] = 0x05;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(new string(head, 0, 2) +
                    lastLower + "\n" + textLower + "\n" + nextLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + lastLower2 + lastLower + textLower
                head[0] = 0x06;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(new string(head, 0, 2) +
                    lastLower2 + "\n" + lastLower + "\n" + textLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + lastLower
                head[0] = 0x07;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + lastLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + lastLower2
                head[0] = 0x08;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + lastLower2, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + nextLower
                head[0] = 0x09;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + nextLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + nextLower2
                head[0] = 0x0a;
                head[1] = (char)posTag;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 2) + nextLower2, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + prefixes + capitalization + initial?
                head[0] = 0x10;
                head[1] = (char)posTag;
                head[2] = capitalization;
                head[3] = isInitial;
                for (int i = 1; i <= 4 && i < textLower.Length(); i++)
                {
                    string prefix = textLower.substring(0, i);
                    if (allowedPrefixes == null ||
                       allowedPrefixes.contains(prefix))
                    {
                        f = posPerceptron.getFeatureID(new string(head, 0, 4) +
                            prefix, extend);
                        if (f >= 0)
                        {
                            feats[nFeats] = f; values[nFeats] = 1.0; nFeats++;
                        }
                    }
                }

                // POS + suffixes + capitalization + initial?
                head[0] = 0x11;
                head[1] = (char)posTag;
                head[2] = capitalization;
                head[3] = isInitial;
                for (int i = textLower.Length() - 5; i < textLower.Length(); i++)
                {
                    if (i < 2) continue;
                    string suffix = textLower.substring(i);
                    if (allowedSuffixes == null ||
                       allowedSuffixes.contains(suffix))
                    {
                        f = posPerceptron.getFeatureID(new string(head, 0, 4) +
                            suffix, extend);
                        if (f >= 0)
                        {
                            feats[nFeats] = f; values[nFeats] = 1.0; nFeats++;
                        }
                    }
                }

                // POS + dictionary
                head[0] = 0x12;
                head[1] = (char)posTag;
                for (int i = 0; i < posDictionaries.size(); i++)
                {
                    Dictionary dict = posDictionaries.get(i);
                    string value = dict.map.get(text);
                    string nextValue =
                        (i == sentence.Length - 1) ? "" :
                        dict.map.get(nextText);
                    string nextValue2 =
                        (i >= sentence.Length - 2) ? "" :
                        dict.map.get(nextText2);
                    head[2] = (char)i;
                    string[] combinations = {
                    value,
                    (value == null || nextValue == null)? null :
                        value + "\n" + nextValue,
                    nextValue,
                    (nextValue == null || nextValue2 == null)? null :
                        nextValue + "\n" + nextValue2,
                    nextValue2
                };
                    for (int j = 0; j < combinations.Length; j++)
                    {
                        if (combinations[j] == null) continue;
                        head[3] = (char)j;
                        f = posPerceptron.getFeatureID(
                            new string(head, 0, 4) + combinations[j], extend);
                        if (f >= 0)
                        {
                            feats[nFeats] = f; values[nFeats] = 1.0; nFeats++;
                        }
                    }
                }

                // POS + embedding
                head[0] = 0x13;
                head[1] = (char)posTag;
                for (int i = 0; i < posEmbeddings.size(); i++)
                {
                    float[] value = posEmbeddings.get(i).map.get(textLower);
                    if (value == null) continue;
                    head[2] = (char)i;
                    for (int j = 0; j < value.Length; j++)
                    {
                        head[3] = (char)j;
                        f = posPerceptron.getFeatureID(
                            new string(head, 0, 4), extend);
                        if (f >= 0)
                        {
                            feats[nFeats] = f; values[nFeats] = value[j]; nFeats++;
                        }
                    }
                }

                // POS + token type + contains dash?
                head[0] = 0x20;
                head[1] = (char)posTag;
                head[2] = tokType;
                head[3] = (char)(textLower.contains("-") ? 1 : 0);
                f = posPerceptron.getFeatureID(new string(head, 0, 4), extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // POS + (current, next) token type
                head[0] = 0x21;
                head[1] = (char)posTag;
                head[2] = tokType;
                head[3] = tokType1a;
                f = posPerceptron.getFeatureID(new string(head, 0, 4), extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }
            }
            else
            {
                char posTag1b = 0xffff;
                char posTag2b = 0xffff;
                if (last != null)
                {
                    posTag1b = (char)last.PosTag;
                    if (last.Last != null) posTag2b = (char)last.Last.PosTag;
                }

                // (previous, current) POS
                head[0] = 0x80;
                head[1] = (char)posTag;
                head[2] = posTag1b;
                f = posPerceptron.getFeatureID(new string(head, 0, 3), extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // (previous2, previous, current) POS
                head[0] = 0x81;
                head[1] = (char)posTag;
                head[2] = posTag1b;
                head[3] = posTag2b;
                f = posPerceptron.getFeatureID(new string(head, 0, 4), extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // (previous, current) POS + textLower
                head[0] = 0x82;
                head[1] = (char)posTag;
                head[2] = posTag1b;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 3) + textLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // (previous, current) POS + textLower + nextLower
                head[0] = 0x83;
                head[1] = (char)posTag;
                head[2] = posTag1b;
                f = posPerceptron.getFeatureID(
                    new string(head, 0, 3) + textLower + "\n" + nextLower, extend);
                if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

                // (previous, current) POS + dictionary
                head[0] = 0x84;
                head[1] = (char)posTag;
                head[2] = posTag1b;
                for (int i = 0; i < posDictionaries.size(); i++)
                {
                    Dictionary dict = posDictionaries.get(i);
                    string nextValue =
                        (i == sentence.Length - 1) ? null :
                        dict.map.get(nextText);
                    if (nextValue == null) continue;
                    head[3] = (char)i;
                    f = posPerceptron.getFeatureID(
                        new string(head, 0, 4) + nextValue, extend);
                    if (f >= 0)
                    {
                        feats[nFeats] = f; values[nFeats] = 1.0; nFeats++;
                    }
                }
            }

            return nFeats;
        }

        /**
         * Computes feature values given a certain NE tag and context.
         */
        protected int getNEFeats(
        TaggedToken[] sentence, int idx, int[] feats, double[] values, int nFeats,
        int posTag, int neTag, int neTypeTag, History last, bool extend)
        {
            char[] head = new char[8];
            int f;
            char isInitial = (idx == 0) ? (char)1 : (char)0;
            char isFinal = (idx == sentence.Length - 1) ? (char)1 : (char)0;
            TaggedToken ttok = sentence[idx];
            char tokType = (char)ttok.Token.type;
            char capitalization = ttok.Token.isCapitalized() ? (char)1 : (char)0;
            int posTag1b = (idx == 0) ? 0xffff : sentence[idx - 1].PosTag;
            int posTag2b = (idx < 2) ? 0xffff : sentence[idx - 2].PosTag;
            int posTag1a =
                (idx == sentence.Length - 1) ? 0xffff : sentence[idx + 1].PosTag;
            string text = ttok.Token.value;
            string textLower = ttok.LowerCaseText;
            string lastLower = (idx == 0) ? "" : sentence[idx - 1].LowerCaseText;
            string lastLower2 = (idx < 2) ? "" : sentence[idx - 2].LowerCaseText;
            string nextLower =
                (idx == sentence.Length - 1) ? "" : sentence[idx + 1].LowerCaseText;
            string nextLower2 =
                (idx >= sentence.Length - 2) ? "" : sentence[idx + 2].LowerCaseText;

            // tag + type + POS
            head[0] = 0x00;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            head[3] = (char)posTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 4), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // tag + type + (previous, current) POS
            head[0] = 0x01;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            head[3] = (char)posTag;
            head[4] = (char)posTag1b;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 5), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // tag + type + (current, next) POS
            head[0] = 0x02;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            head[3] = (char)posTag;
            head[4] = (char)posTag1a;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 5), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // tag + type + textLower
            head[0] = 0x03;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 3) + textLower, extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // tag + type + textLower + nextLower
            head[0] = 0x04;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 3) + textLower + "\n" + nextLower, extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // tag + type + lastLower + textLower
            head[0] = 0x04;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 3) + lastLower + "\n" + textLower, extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // dictionaries
            head[0] = 0x08;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            for (int i = 0; i < neDictionaries.size(); i++)
            {
                Dictionary dict = neDictionaries.get(i);
                string value = dict.map.get(textLower);
                string lastValue = (i == 0) ? "" : dict.map.get(lastLower);
                string nextValue =
                    (i == sentence.Length - 1) ? "" :
                    dict.map.get(nextLower);
                head[3] = (char)i;
                string[] combinations = {
                value,
                (value == null || lastValue == null)? null :
                    lastValue + "\n" + value,
                (value == null || nextValue == null)? null :
                    value + "\n" + nextValue,
                nextValue
            };
                for (int j = 0; j < combinations.Length; j++)
                {
                    if (combinations[j] == null) continue;
                    head[4] = (char)j;
                    f = nePerceptron.getFeatureID(
                        new string(head, 0, 5) + combinations[j], extend);
                    if (f >= 0)
                    {
                        feats[nFeats] = f; values[nFeats] = 1.0; nFeats++;
                    }
                }
            }

            // embeddings
            head[0] = 0x09;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            for (int i = 0; i < neEmbeddings.size(); i++)
            {
                float[] value = neEmbeddings.get(i).map.get(textLower);
                if (value == null) continue;
                head[3] = (char)i;
                for (int j = 0; j < value.Length; j++)
                {
                    head[4] = (char)j;
                    f = nePerceptron.getFeatureID(
                        new string(head, 0, 5), extend);
                    if (f >= 0)
                    {
                        feats[nFeats] = f; values[nFeats] = value[j]; nFeats++;
                    }
                }
            }

            // tag + type + token type
            head[0] = 0x0a;
            head[1] = (char)neTag;
            head[2] = (char)neTypeTag;
            head[3] = tokType;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 4), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            char neTag1b = 0xffff;
            char neTag2b = 0xffff;
            if (last != null)
            {
                neTag1b = (char)last.NeTag;
                if (last.Last != null) neTag2b = (char)last.Last.NeTag;
            }

            // (previous, current) tag + type
            head[0] = 0x80;
            head[1] = (char)neTag;
            head[2] = (char)neTag1b;
            head[3] = (char)neTypeTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 4), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            // (previous, current) tag + type
            head[0] = 0x81;
            head[1] = (char)neTag;
            head[2] = (char)neTag1b;
            head[3] = (char)neTag2b;
            head[4] = (char)neTypeTag;
            f = nePerceptron.getFeatureID(
                new string(head, 0, 5), extend);
            if (f >= 0) { feats[nFeats] = f; values[nFeats] = 1.0; nFeats++; }

            return nFeats;
        }
    }
}