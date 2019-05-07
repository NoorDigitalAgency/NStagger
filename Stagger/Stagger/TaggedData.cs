using System;
using System.IO;

namespace Stagger
{
    [Serializable]
    public class TaggedData
    {
        public TagSet PosTagSet { get; }

        public TagSet NeTagSet { get; }

        public TagSet NeTypeTagSet { get; }

        public string Language { get; }

        public const int NeO = 0;

        public const int NeB = 1;

        public const int NeI = 2;

        public const int NeTags = 3;

        public TaggedData(string language)
        {
            Language = language;

            try
            {
                PosTagSet = new TagSet();

                NeTagSet = new TagSet();

                NeTagSet.AddTag("O");

                NeTagSet.AddTag("B");

                NeTagSet.AddTag("I");

                NeTypeTagSet = new TagSet();
            }
            catch (TagNameException)
            {
            }
        }

        public void writeConll(StreamWriter writer, TaggedToken[][] sentences, bool plain)
        {
            foreach (TaggedToken[] sentence in sentences)
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    TaggedToken token = sentence[i];

                    writer.Write($"{tokenToString(token, i, plain)}{Environment.NewLine}");
                }

                writer.Write(Environment.NewLine);
            }
        }

        public void writeConllSentence(StreamWriter writer, TaggedToken[] sentence, bool plain)
        {
            TaggedToken[][] sentences = new TaggedToken[1][];

            sentences[0] = sentence;

            writeConll(writer, sentences, plain);
        }

        public void writeConllGold(StreamWriter writer, TaggedToken[] tokens, TaggedToken[] goldTokens, bool plain)
        {
            if (tokens.Length != goldTokens.Length)
            {
                throw new Exception("Size of the Tokens Array and the Gold Token Array are not the same.");
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                TaggedToken token = tokens[i];

                TaggedToken gold = goldTokens[i];

                writer.Write($"{tokenToString(token, i, plain)}{Environment.NewLine}");

                if (!token.ConsistentWith(gold))
                {
                    writer.Write($"#{tokenToString(gold, i, plain)}{Environment.NewLine}");
                }
            }

            writer.Write(Environment.NewLine);
        }

        /**
         * Converts a single token to a line in a CoNLL file.
         *
         * @param token     tokens to convert
         * @param idx       0-based index within the sentence
         * @param plain     use plain output rather than CoNLL?
         * @throws TagNameException if any tag value is invalid
         */
        private string tokenToString(TaggedToken token, int idx, bool plain)
        {
            if (plain) return token.token.value + "\t" +
                              posTagSet.getTagName(token.posTag);
            string[]
            pos = null;
            string neTag = null;
            string neType = null;
            if (token.posTag >= 0)
                pos = posTagSet.getTagName(token.posTag).split("\\|", 2);
            if (token.neTag >= 0)
                neTag = NeTagSet.getTagName(token.neTag);
            if (token.neTypeTag >= 0)
                neType = NeTypeTagSet.getTagName(token.neTypeTag);
            return
                (idx + 1) + "\t" +
                token.token.value + "\t" +
                ((token.lf == null) ? "" : token.lf) + "\t" +
                ((pos == null) ? "_" : pos[0]) + "\t" +
                ((pos == null) ? "_" : pos[0]) + "\t" +
                ((pos == null || pos.Length < 2) ? "_" : pos[1]) + "\t" +
                "_\t" +
                "_\t" +
                "_\t" +
                "_\t" +
                ((neTag == null) ? "_" : neTag) + "\t" +
                ((neType == null) ? "_" : neType) + "\t" +
                ((token.id == null) ? "_" : token.id);
        }

        /**
         * Reads a number of .conll files.
         *
         * @param filenames names of files
         * @param extend    if true, unknown tags are created
         * @param plain     use plain format rather than CoNLL?
         * @return          array of sentences (arrays of TaggedToken)
         * @throws FormatException if the syntax is invalid
         * @throws TagNameException if any tag value is invalid
         * @throws IOException from the reader
         */
        public TaggedToken[][][] readConllFiles(
        string[] filenames, bool extend, bool plain)
        {
            int nFiles = filenames.Length;
            TaggedToken[][][] files = new TaggedToken[nFiles][][];
            int fileIdx = 0;
            for (string name : filenames)
            {
                string id = (new File(name)).getName().split("\\.")[0];
                files[fileIdx++] = readConll(name, id, extend, plain);
            }
            return files;
        }

        /**
         * Reads all sentences in the given file, until EOF.
         *
         * @param filename  name of the file to read
         * @param fileID    identifier of the file (used for token IDs)
         * @param extend    if true, unknown tags are created
         * @param plain     use plain format rather than CoNLL?
         * @return          null on EOF, otherwise array of tokens
         * @throws FormatException if the syntax is invalid
         * @throws TagNameException if any tag value is invalid
         * @throws IOException from the reader
         */
        public TaggedToken[][] readConll(
        string filename, string fileID, bool extend, bool plain)
        {
            if (fileID == null)
            {
                fileID = (new File(filename)).getName().split("\\.")[0];
            }
            BufferedReader reader = new BufferedReader(
                new InputStreamReader(
                    new FileInputStream(filename), "UTF-8"));
            TaggedToken[][] data = readConll(reader, fileID, extend, plain);
            reader.close();
            return data;
        }

        /**
         * Reads all sentences in the given file, until EOF.
         *
         * @param reader    BufferedReader to read from
         * @param fileID    identifier of the file (used for token IDs)
         * @param extend    if true, unknown tags are created
         * @param plain     use plain format rather than CoNLL?
         * @return          null on EOF, otherwise array of tokens
         * @throws FormatException if the syntax is invalid
         * @throws TagNameException if any tag value is invalid
         * @throws IOException from the reader
         */
        public TaggedToken[][] readConll(
        BufferedReader reader, string fileID, bool extend, bool plain)
        {
            ArrayList<TaggedToken[]> sentences = new ArrayList<TaggedToken[]>();
            ArrayList<TaggedToken> sentence = new ArrayList<TaggedToken>();
            Tokenizer tokenizer;
            if (Language.equals("sv"))
                tokenizer = new SwedishTokenizer(new StringReader(""));
            else if (Language.equals("en"))
                tokenizer = new EnglishTokenizer(new StringReader(""));
            else if (Language.equals("zh"))
                tokenizer = null;
            else
                tokenizer = new LatinTokenizer(new StringReader(""));
            string line;
            int sentIdx = 0;
            int tokIdx = 0;
            while ((line = reader.readLine()) != null)
            {
                if (line.equals(""))
                {
                    if (sentence.size() > 0)
                    {
                        TaggedToken[] tokensArray =
                            new TaggedToken[sentence.size()];
                        sentences.add(sentence.toArray(tokensArray));
                        sentence = new ArrayList<TaggedToken>();
                        sentIdx++;
                        tokIdx = 0;
                    }
                    continue;
                }
                if (line.startsWith("#")) continue;
                string[] fields = (plain) ? line.split("\\s+")
                                         : line.split("\t", -1);
                string posString = null;
                string neString = null;
                string neTypeString = null;
                string tokenID = null;
                string text = null;
                string lf = null;
                int nFields = fields.Length;
                if (plain)
                {
                    if (nFields < 1 || nFields > 2)
                    {
                        throw new FormatException(
                            "Expected 1 or 2 fields, found " + fields.Length +
                            " in: " + line);
                    }
                    text = fields[0];
                    if (nFields == 2) posString = fields[1];
                }
                else
                {
                    if (nFields < 6)
                        throw new FormatException(
                            "Expected at least 6 fields, found " + fields.Length +
                            " in: " + line);
                    text = fields[1];
                    lf = fields[2];
                    if (lf.equals("") || (lf.equals("_") && !text.equals("_")))
                        lf = null;
                    if (!fields[3].equals("_"))
                    {
                        if (!(fields[5].equals("") || fields[5].equals("_")))
                            posString = fields[3] + "|" + fields[5];
                        else
                            posString = fields[3];
                    }
                    if (nFields >= 12 && !fields[10].equals("_"))
                        neString = fields[10];
                    if (nFields >= 12 && !fields[11].equals("_"))
                        neTypeString = fields[11];
                    if (nFields >= 13 && !fields[12].equals("_"))
                        tokenID = fields[12];
                }
                if (text.equals(""))
                {
                    throw new FormatException("Text field empty in: " + line);
                }
                if (tokenID == null)
                {
                    tokenID = fileID + ":" + sentIdx + ":" + tokIdx;
                }
                TaggedToken token;
                // TODO: consider interpreting z as the offset if the ID is on the
                // form x:y:z
                if (tokenizer == null)
                {
                    token = new TaggedToken(
                        new Token(Token.TOK_UNKNOWN, text, 0), tokenID);
                }
                else
                {
                    // Use the tokenizer to find the token type of this file
                    tokenizer.yyreset(new StringReader(text));
                    Token subToken = tokenizer.yylex();
                    // Note that only the first subtoken is used, in case the
                    // token is complex.
                    token = new TaggedToken(
                                        new Token(subToken.type, text, 0), tokenID);
                }
                int posTag = -1, neTag = -1, neTypeTag = -1;
                try
                {
                    if (posString != null)
                        posTag = posTagSet.getTagID(posString, extend);
                    if (neString != null)
                    {
                        if (neString.equals("U")) neString = "B";
                        else if (neString.equals("L")) neString = "I";
                        neTag = NeTagSet.getTagID(neString, false);
                    }
                }
                catch (TagNameException e)
                {
                    System.err.println(line);
                    throw e;
                }
                if (neTypeString != null)
                    neTypeTag = NeTypeTagSet.getTagID(neTypeString, extend);
                token.lf = lf;
                token.posTag = posTag;
                token.neTag = neTag;
                token.neTypeTag = neTypeTag;
                sentence.add(token);

                tokIdx++;
            }
            // In case the last sentence was not followed by an empty line, make
            // sure to add it.
            if (sentence.size() > 0)
            {
                TaggedToken[] tokensArray =
                    new TaggedToken[sentence.size()];
                sentences.add(sentence.toArray(tokensArray));
            }
            if (sentences.size() == 0) return null;
            TaggedToken[][] sentenceArray = new TaggedToken[sentences.size()][];
            return sentences.toArray(sentenceArray);
        }
    }
}