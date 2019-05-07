using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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

        public void WriteConll(StreamWriter writer, TaggedToken[][] sentences, bool plain)
        {
            foreach (TaggedToken[] sentence in sentences)
            {
                for (int i = 0; i < sentences.Length; i++)
                {
                    TaggedToken token = sentence[i];

                    writer.Write($"{TokenToString(token, i, plain)}{Environment.NewLine}");
                }

                writer.Write(Environment.NewLine);
            }
        }

        public void WriteConllSentence(StreamWriter writer, TaggedToken[] sentence, bool plain)
        {
            TaggedToken[][] sentences = new TaggedToken[1][];

            sentences[0] = sentence;

            WriteConll(writer, sentences, plain);
        }

        public void WriteConllGold(StreamWriter writer, TaggedToken[] tokens, TaggedToken[] goldTokens, bool plain)
        {
            if (tokens.Length != goldTokens.Length)
            {
                throw new Exception("Size of the Tokens Array and the Gold Token Array are not the same.");
            }

            for (int i = 0; i < tokens.Length; i++)
            {
                TaggedToken token = tokens[i];

                TaggedToken gold = goldTokens[i];

                writer.Write($"{TokenToString(token, i, plain)}{Environment.NewLine}");

                if (!token.ConsistentWith(gold))
                {
                    writer.Write($"#{TokenToString(gold, i, plain)}{Environment.NewLine}");
                }
            }

            writer.Write(Environment.NewLine);
        }

        private string TokenToString(TaggedToken token, int index, bool plain)
        {
            if (plain)
            {
                return $"{token.Token.Value}\t{PosTagSet.GetTagName(token.PosTag)}";
            }

            string[] pos = null;

            string neTag = null;

            string neType = null;

            if (token.PosTag >= 0)
            {
                pos = PosTagSet.GetTagName(token.PosTag).Split(new[] { '\\', '|' }, 2);
            }

            if (token.NeTag >= 0)
            {
                neTag = NeTagSet.GetTagName(token.NeTag);
            }

            if (token.NeTypeTag >= 0)
            {
                neType = NeTypeTagSet.GetTagName(token.NeTypeTag);
            }

            return $"{index + 1}\t{token.Token.Value}\t{token.Lemma ?? ""}\t{((pos == null) ? "_" : pos[0])}\t{((pos == null) ? "_" : pos[0])}\t{((pos == null || pos.Length < 2) ? "_" : pos[1])}\t_\t_\t_\t_\t{neTag ?? "_"}\t{neType ?? "_"}\t{token.Id ?? "_"}";
        }

        public TaggedToken[][][] ReadConllFiles(string[] filePaths, bool extend, bool plain)
        {
            TaggedToken[][][] files = new TaggedToken[filePaths.Length][][];

            int fileIndex = 0;

            foreach (string filePath in filePaths)
            {
                string id = Path.GetFileNameWithoutExtension(filePath);

                files[fileIndex++] = ReadConll(filePath, id, extend, plain);
            }

            return files;
        }

        public TaggedToken[][] ReadConll(string filePath, string fileId, bool extend, bool plain)
        {
            if (fileId == null)
            {
                fileId = Path.GetFileNameWithoutExtension(filePath);
            }

            TaggedToken[][] data;

            using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
            {
                data = ReadConll(reader, fileId, extend, plain);

                reader.Close();
            }

            return data;
        }

        public TaggedToken[][] ReadConll(StreamReader reader, string fileId, bool extend, bool plain)
        {
            List<TaggedToken[]> sentences = new List<TaggedToken[]>();

            List<TaggedToken> sentence = new List<TaggedToken>();

            Tokenizer tokenizer;

            if (Language.Equals("sv"))
            {
                tokenizer = new SwedishTokenizer(new StringReader(""));
            }
            else if (Language.Equals("en"))
            {
                tokenizer = new EnglishTokenizer(new StringReader(""));
            }
            else if (Language.Equals("zh"))
            {
                tokenizer = null;
            }
            else
            {
                tokenizer = new LatinTokenizer(new StringReader(""));
            }

            string line;

            int sentenceIndex = 0;

            int tokenIndex = 0;

            while ((line = reader.ReadLine()) != null)
            {
                if (line.Equals(""))
                {
                    if (sentence.Count > 0)
                    {
                        sentences.Add(sentence.ToArray());

                        sentence = new List<TaggedToken>();

                        sentenceIndex++;

                        tokenIndex = 0;
                    }

                    continue;
                }

                if (line.StartsWith("#"))
                {
                    continue;
                }

                string[] fields = plain ? Regex.Split(line, "\\s+") : line.Split('\t');

                string posString = null;

                string neString = null;

                string neTypeString = null;

                string tokenId = null;

                string text;

                string lemma = null;

                int fieldsLength = fields.Length;

                if (plain)
                {
                    if (fieldsLength < 1 || fieldsLength > 2)
                    {
                        throw new FormatException($"Expected 1 or 2 fields, found {fields.Length} in: {line}");
                    }

                    text = fields[0];

                    if (fieldsLength == 2)
                    {
                        posString = fields[1];
                    }
                }
                else
                {
                    if (fieldsLength < 6)
                    {
                        throw new FormatException($"Expected at least 6 fields, found {fields.Length} in: {line}");
                    }

                    text = fields[1];

                    lemma = fields[2];

                    if (lemma.Equals("") || (lemma.Equals("_") && !text.Equals("_")))
                    {
                        lemma = null;
                    }

                    if (!fields[3].Equals("_"))
                    {
                        if (!(fields[5].Equals("") || fields[5].Equals("_")))
                        {
                            posString = fields[3] + "|" + fields[5];
                        }
                        else
                        {
                            posString = fields[3];
                        }
                    }

                    if (fieldsLength >= 12 && !fields[10].Equals("_"))
                    {
                        neString = fields[10];
                    }

                    if (fieldsLength >= 12 && !fields[11].Equals("_"))
                    {
                        neTypeString = fields[11];
                    }

                    if (fieldsLength >= 13 && !fields[12].Equals("_"))
                    {
                        tokenId = fields[12];
                    }
                }

                if (text.Equals(""))
                {
                    throw new FormatException($"Text field empty in: {line}");
                }

                if (tokenId == null)
                {
                    tokenId = $"{fileId}:{sentenceIndex}:{tokenIndex}";
                }

                TaggedToken token;

                if (tokenizer == null)
                {
                    token = new TaggedToken(new Token(TokenType.Unknown, text, 0), tokenId);
                }
                else
                {
                    tokenizer.Reset(new StringReader(text));

                    Token subToken = tokenizer.Tokenize();

                    token = new TaggedToken(new Token(subToken.Type, text, 0), tokenId);
                }

                int posTag = -1, neTag = -1, neTypeTag = -1;

                try
                {
                    if (posString != null)
                    {
                        posTag = PosTagSet.GetTagId(posString, extend);
                    }

                    if (neString != null)
                    {
                        if (neString.Equals("U"))
                        {
                            neString = "B";
                        }
                        else if (neString.Equals("L"))
                        {
                            neString = "I";
                        }

                        neTag = NeTagSet.GetTagId(neString, false);
                    }
                }
                catch (TagNameException e)
                {
                    Console.WriteLine(e);

                    throw;
                }

                if (neTypeString != null)
                {
                    neTypeTag = NeTypeTagSet.GetTagId(neTypeString, extend);
                }

                token.Lemma = lemma;

                token.PosTag = posTag;

                token.NeTag = neTag;

                token.NeTypeTag = neTypeTag;

                sentence.Add(token);

                tokenIndex++;
            }

            if (sentence.Count > 0)
            {
                sentences.Add(sentence.ToArray());
            }

            return sentences.Count == 0 ? null : sentences.ToArray();
        }
    }
}