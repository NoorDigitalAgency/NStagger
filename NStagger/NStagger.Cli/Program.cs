using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace NStagger.Cli
{
    internal class Program
    {
        private static Tokenizer GetTokenizer(TextReader reader, string language)
        {
            Tokenizer tokenizer;

            if (language.Equals("sv"))
            {
                tokenizer = new SwedishTokenizer(reader);
            }
            else if (language.Equals("en"))
            {
                tokenizer = new EnglishTokenizer(reader);
            }
            else if (language.Equals("any"))
            {
                tokenizer = new LatinTokenizer(reader);
            }
            else
            {
                throw new ArgumentException();
            }

            return tokenizer;
        }

        private static Tagger GetTagger(string language, TaggedData taggedData, int posBeamSize, int neBeamSize)
        {
            Tagger tagger = null;

            if (language.Equals("sv"))
            {
                tagger = new SUCTagger(taggedData, posBeamSize, neBeamSize);
            }
            else if (language.Equals("en"))
            {
                tagger = new PTBTagger(taggedData, posBeamSize, neBeamSize);
            }
            else if (language.Equals("any"))
            {
                tagger = new GenericTagger(taggedData, posBeamSize, neBeamSize);
            }
            else if (language.Equals("zh"))
            {
                tagger = new CTBTagger(taggedData, posBeamSize, neBeamSize);
            }
            else
            {
                Console.WriteLine($"Invalid language '{language}'.");

                Environment.Exit(1);
            }

            return tagger;
        }

        private static TaggedToken[][][] GetSUCFold(TaggedToken[][] sentences, int fold)
        {
            TaggedToken[][][] parts = new TaggedToken[3][][];

            HashSet<string> fileSet = new HashSet<string>();

            foreach (TaggedToken[] sentence in sentences)
            {
                string fileId = sentence[0].Id.Substring(0, 4);

                fileSet.Add(fileId);
            }

            List<string> files = new List<string>(fileSet);

            Collections.Sort(files);

            Debug.Assert(files.Count == 500);

            Dictionary<string, int> fileIndex = new Dictionary<string, int>();

            for (int i = 0; i < files.Count; i++)
            {
                string fileId = files[i];

                fileIndex[fileId] = i;
            }

            int developmentCount = 0, testCount = 0, trainCount = 0;

            foreach (TaggedToken[] sentence in sentences)
            {
                string fileId = sentence[0].Id.Substring(0, 4);

                int index = fileIndex[fileId];

                if ((index % 10) == fold)
                {
                    testCount++;
                }
                else if ((((index + 1) % 10) == fold) && (index / 10 % 5 == 0))
                {
                    developmentCount++;
                }

                else trainCount++;
            }

            parts[0] = new TaggedToken[trainCount][];

            parts[1] = new TaggedToken[developmentCount][];

            parts[2] = new TaggedToken[testCount][];

            int developmentIndex = 0, testIndex = 0, trainIndex = 0;

            foreach (TaggedToken[] sentence in sentences)
            {
                string fileId = sentence[0].Id.Substring(0, 4);

                int index = fileIndex[fileId];

                if ((index % 10) == fold)
                {
                    parts[2][testIndex++] = sentence;
                }
                else if ((((index + 1) % 10) == fold) && (index / 10 % 5 == 0))
                {
                    parts[1][developmentIndex++] = sentence;
                }
                else
                {
                    parts[0][trainIndex++] = sentence;
                }
            }

            return parts;
        }

        private static TaggedToken[][][] GetFold(TaggedToken[][] sentences, int foldsCount, int developmentPercentage, int testPercentage, int foldNumber)
        {
            int j, k;

            TaggedToken[][][] parts = new TaggedToken[3][][];

            List<int> order = new List<int>(sentences.Length);

            for (j = 0; j < sentences.Length; j++)
            {
                order.Add(j);
            }

            Collections.Shuffle(order, new Random(1));

            int developmentCount = developmentPercentage * sentences.Length / 1000;

            int testCount = testPercentage * sentences.Length / 1000;

            int trainCount = sentences.Length - (developmentCount + testCount);

            parts[0] = new TaggedToken[trainCount][];

            parts[1] = new TaggedToken[developmentCount][];

            parts[2] = new TaggedToken[testCount][];

            int factor = sentences.Length * foldNumber / foldsCount;

            for (j = 0, k = 0; j < factor; j++)
            {
                parts[0][k++] = sentences[j];
            }

            for (j = factor + developmentCount + testCount; j < sentences.Length; j++)
            {
                parts[0][k++] = sentences[j];
            }

            for (j = 0; j < developmentCount; j++)
            {
                parts[1][j] = sentences[factor + j];
            }

            for (j = 0; j < testCount; j++)
            {
                parts[2][j] = sentences[factor + developmentCount + j];
            }

            return parts;
        }

        private static TextReader OpenUtf8File(string name)
        {
            if (name.Equals("-"))
            {
                return new StreamReader(Console.OpenStandardInput(), Encoding.UTF8);
            }

            return name.EndsWith(".gz") ? new StreamReader(new GZipStream(new FileStream(name, FileMode.Open), CompressionMode.Decompress), Encoding.UTF8) : new StreamReader(new FileStream(name, FileMode.Open), Encoding.UTF8);
        }

        public static void Main(string[] args)
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            Stopwatch stopwatch = Stopwatch.StartNew();

            SUCTagger sucTagger = Mapper.Map<SUCTagger>();

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            stopwatch.Restart();

            SUCTagger t = (SUCTagger) binaryFormatter.Deserialize(File.OpenRead(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nmodel"));//Mapper.Map<SUCTagger>();

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();
            
            //binaryFormatter.Serialize(File.OpenWrite(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.nmodel"), sucTagger);

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            string lexiconFile = null;

            string trainFile = null;

            string developmentFile = null;

            string modelFile = null;

            List<Dictionary> posDictionaries = new List<Dictionary>();

            List<Embedding> posEmbeddings = new List<Embedding>();

            List<Dictionary> neDictionaries = new List<Dictionary>();

            List<Embedding> neEmbeddings = new List<Embedding>();

            int posBeamSize = 8;

            int neBeamSize = 4;

            string language = null;

            bool preserve = false;

            bool plainOutput = false;

            string fold = null;

            int maximumPosIterations = 16;

            int maximumNeIterations = 16;

            bool extendLexicon = true;

            bool hasNe = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-lexicon"))
                {
                    lexiconFile = args[++i];
                }
                else if (args[i].Equals("-dict"))
                {
                    string destination = args[++i];

                    Dictionary dictionary = new Dictionary();

                    try
                    {
                        dictionary.FromFile(args[++i]);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Can not load dictionary file.");

                        Console.WriteLine(e.StackTrace);

                        Environment.Exit(1);
                    }

                    if (destination.Equals("pos"))
                    {
                        posDictionaries.Add(dictionary);
                    }
                    else if (destination.Equals("ne"))
                    {
                        neDictionaries.Add(dictionary);
                    }
                    else if (destination.Equals("all"))
                    {
                        posDictionaries.Add(dictionary);

                        neDictionaries.Add(dictionary);
                    }
                    else
                    {
                        Console.WriteLine("Expected pos/ne/all.");

                        Environment.Exit(1);
                    }
                }
                else if (args[i].Equals("-lang"))
                {
                    language = args[++i];
                }
                else if (args[i].Equals("-extendlexicon"))
                {
                    extendLexicon = true;
                }
                else if (args[i].Equals("-noextendlexicon"))
                {
                    extendLexicon = false;
                }
                else if (args[i].Equals("-noner"))
                {
                    hasNe = false;
                }
                else if (args[i].Equals("-positers"))
                {
                    maximumPosIterations = int.Parse(args[++i]);
                }
                else if (args[i].Equals("-neiters"))
                {
                    maximumNeIterations = int.Parse(args[++i]);
                }
                else if (args[i].Equals("-posbeamsize"))
                {
                    posBeamSize = int.Parse(args[++i]);
                }
                else if (args[i].Equals("-nebeamsize"))
                {
                    neBeamSize = int.Parse(args[++i]);
                }
                else if (args[i].Equals("-preserve"))
                {
                    preserve = true;
                }
                else if (args[i].Equals("-plain"))
                {
                    plainOutput = true;
                }
                else if (args[i].Equals("-fold"))
                {
                    fold = args[++i];
                }
                else if (args[i].Equals("-embed"))
                {
                    string destination = args[++i];

                    Embedding embedding = new Embedding();

                    try
                    {
                        embedding.FromFile(args[++i]);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Can not load embedding file.");

                        Console.WriteLine(e.StackTrace);

                        Environment.Exit(1);
                    }

                    if (destination.Equals("pos"))
                    {
                        posEmbeddings.Add(embedding);
                    }
                    else if (destination.Equals("ne"))
                    {
                        neEmbeddings.Add(embedding);
                    }
                    else if (destination.Equals("all"))
                    {
                        posEmbeddings.Add(embedding);

                        neEmbeddings.Add(embedding);
                    }
                    else
                    {
                        Console.WriteLine("Expected pos/ne/all.");

                        Environment.Exit(1);
                    }
                }
                else if (args[i].Equals("-trainfile"))
                {
                    trainFile = args[++i];
                }
                else if (args[i].Equals("-devfile"))
                {
                    developmentFile = args[++i];
                }
                else if (args[i].Equals("-modelfile"))
                {
                    modelFile = args[++i];
                }
                else if (args[i].Equals("-train"))
                {
                    TaggedToken[][] developmentSentences = null;

                    if (trainFile == null || modelFile == null || language == null)
                    {
                        Console.WriteLine("Insufficient data.");

                        Environment.Exit(1);
                    }

                    TaggedData taggedData = new TaggedData(language);

                    TaggedToken[][] trainSentences = taggedData.ReadConll(trainFile, null, true, !trainFile.EndsWith(".conll"));

                    if (developmentFile != null)
                    {
                        developmentSentences = taggedData.ReadConll(developmentFile, null, true, !developmentFile.EndsWith(".conll"));
                    }

                    Console.WriteLine($"Read {trainSentences.Length} training sentences and {developmentSentences?.Length ?? 0} development sentences.");

                    Tagger tagger = GetTagger(language, taggedData, posBeamSize, neBeamSize);

                    tagger.BuildLexicons(trainSentences);

                    Lexicon lexicon = tagger.PosLexicon;

                    Console.WriteLine($"POS lexicon size (corpus) {lexicon.Size}.");

                    if (lexiconFile != null)
                    {
                        Console.WriteLine(extendLexicon ? $"Reading lexicon '{lexiconFile}'." : $"Reading lexicon (not extending profiles) '{lexiconFile}'.");

                        lexicon.FromFile(lexiconFile, taggedData.PosTagSet, extendLexicon);

                        Console.WriteLine($"POS lexicon size (external) {lexicon.Size}.");
                    }

                    tagger.PosDictionaries = posDictionaries;

                    tagger.PosEmbeddings = posEmbeddings;

                    tagger.NeDictionaries = neDictionaries;

                    tagger.NeEmbeddings = neEmbeddings;

                    tagger.MaximumPosIterations = maximumPosIterations;

                    tagger.MaximumNeIterations = maximumNeIterations;

                    tagger.Train(trainSentences, developmentSentences);

                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(new FileStream(modelFile, FileMode.Create), tagger);
                }
                else if (args[i].Equals("-cross"))
                {
                    TaggedData taggedData = new TaggedData(language);

                    TaggedToken[][] allSentences = taggedData.ReadConll(trainFile, null, true, !trainFile.EndsWith(".conll"));

                    Tagger tagger = GetTagger(language, taggedData, posBeamSize, neBeamSize);

                    tagger.PosDictionaries = posDictionaries;

                    tagger.PosEmbeddings = posEmbeddings;

                    tagger.NeDictionaries = neDictionaries;

                    tagger.NeEmbeddings = neEmbeddings;

                    const int foldsCount = 10;

                    Evaluation evaluation = new Evaluation();

                    for (int j = 0; j < foldsCount; j++)
                    {
                        Evaluation localEvaluation = new Evaluation();

                        TaggedToken[][][] parts = GetSUCFold(allSentences, j);

                        Console.WriteLine($"Fold {j}, train ({parts[0].Length}), development ({parts[1].Length}), test ({parts[2].Length})");

                        Lexicon lexicon = tagger.PosLexicon;

                        lexicon.Clear();

                        tagger.BuildLexicons(parts[0]);

                        if (lexiconFile != null)
                        {
                            lexicon.FromFile(lexiconFile, taggedData.PosTagSet, extendLexicon);
                        }

                        tagger.Train(parts[0], parts[1]);

                        foreach (TaggedToken[] sentence in parts[2])
                        {
                            TaggedToken[] taggedSentence = tagger.TagSentence(sentence, true, false);

                            evaluation.Evaluate(taggedSentence, sentence);

                            localEvaluation.Evaluate(taggedSentence, sentence);

                            tagger.TaggedData.WriteConllGold(new StreamWriter(Console.OpenStandardOutput()), taggedSentence, sentence, plainOutput);
                        }

                        Console.WriteLine($"Local POS accuracy: {localEvaluation.GetPosAccuracy()} ({localEvaluation.PosCorrect} / {localEvaluation.PosTotal})");
                    }

                    Console.WriteLine($"POS accuracy: {evaluation.GetPosAccuracy()} ({evaluation.PosCorrect} / {evaluation.PosTotal})");

                    Console.WriteLine($"NE precision: {evaluation.GetNePrecision()}");

                    Console.WriteLine($"NE recall:    {evaluation.GetNeRecall()}");

                    Console.WriteLine($"NE F-score:   {evaluation.GetNeFScore()}");

                    Console.WriteLine($"NE total:     {evaluation.NeTotal}");

                    Console.WriteLine($"NE correct:   {evaluation.NeCorrect}");

                    Console.WriteLine($"NE found:     {evaluation.NeFound}");
                }
                else if (args[i].Equals("-server"))
                {
                    if (modelFile == null || i >= args.Length - 1)
                    {
                        Console.WriteLine("Insufficient data.");

                        Environment.Exit(1);
                    }

                    IPAddress serverIp = Dns.GetHostAddresses(args[++i]).FirstOrDefault();

                    int serverPort = int.Parse(args[++i]);

                    BinaryFormatter formatter = new BinaryFormatter();

                    Console.WriteLine("Loading Stagger model ...");

                    Tagger tagger = (Tagger)formatter.Deserialize(new FileStream(modelFile, FileMode.Open));

                    language = tagger.TaggedData.Language;

                    TcpListener tcpListener = new TcpListener(serverIp, serverPort);

                    tcpListener.Start(4);

                    while (true)
                    {
                        Socket sock = null;

                        try
                        {
                            sock = tcpListener.AcceptSocket();

                            Console.WriteLine($"Connected to {sock.RemoteEndPoint}");

                            NetworkStream networkStream = new NetworkStream(sock);

                            byte[] lengthBuffer = new byte[4];

                            if (networkStream.Read(lengthBuffer) != 4)
                            {
                                throw new IOException("Can not read length.");
                            }

                            int length = BitConverter.ToInt32(lengthBuffer);

                            if (length < 1 || length > 100000)
                            {
                                throw new IOException($"Invalid data size {length}.");
                            }

                            byte[] dataBuf = new byte[length];
                            if (networkStream.Read(dataBuf) != length)
                            {
                                throw new IOException("Can not read data.");
                            }

                            StringReader reader = new StringReader(Encoding.UTF8.GetString(dataBuf));

                            StreamWriter writer = new StreamWriter(networkStream, Encoding.UTF8);

                            Tokenizer tokenizer = GetTokenizer(reader, language);

                            List<Token> sentence;

                            int sentenceIndex = 0;

                            string fileId = "net";

                            while ((sentence = tokenizer.ReadSentence()) != null)
                            {
                                TaggedToken[] taggedSentence = new TaggedToken[sentence.Count];

                                if (tokenizer.SentenceId != null)
                                {
                                    if (!fileId.Equals(tokenizer.SentenceId))
                                    {
                                        fileId = tokenizer.SentenceId;

                                        sentenceIndex = 0;
                                    }
                                }

                                for (int j = 0; j < sentence.Count; j++)
                                {
                                    Token token = sentence[j];

                                    var id = $"{fileId}:{sentenceIndex}:{token.Offset}";

                                    taggedSentence[j] = new TaggedToken(token, id);
                                }

                                TaggedToken[] taggedSent = tagger.TagSentence(taggedSentence, true, false);

                                tagger.TaggedData.WriteConllSentence(writer ?? new StreamWriter(Console.OpenStandardOutput()), taggedSent, plainOutput);

                                sentenceIndex++;
                            }

                            tokenizer.Close();

                            if (sock.Connected)
                            {
                                Console.WriteLine($"Closing connection to {sock.RemoteEndPoint}.");

                                writer.Close();
                            }
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e.StackTrace);

                            if (sock != null)
                            {
                                Console.WriteLine($"Connection failed to {sock.RemoteEndPoint}.");

                                if (sock.Connected)
                                {
                                    sock.Close();
                                }
                            }
                        }
                    }
                }
                else if (args[i].Equals("-tag"))
                {
                    if (modelFile == null || i >= args.Length - 1)
                    {
                        Console.WriteLine("Insufficient data.");

                        Environment.Exit(1);
                    }

                    List<string> inputFiles = new List<string>();

                    for (i++; i < args.Length && !args[i].StartsWith("-"); i++)
                    {
                        inputFiles.Add(args[i]);
                    }

                    if (inputFiles.Count < 1)
                    {
                        Console.WriteLine("No files to tag.");

                        Environment.Exit(1);
                    }

                    BinaryFormatter formatter = new BinaryFormatter();

                    Console.WriteLine("Loading Stagger model ...");

                    Tagger tagger = (Tagger)formatter.Deserialize(new FileStream(modelFile, FileMode.Open));

                    language = tagger.TaggedData.Language;

                    tagger.ExtendLexicon = extendLexicon;

                    if (!hasNe)
                    {
                        tagger.HasNe = false;
                    }

                    foreach (string inputFile in inputFiles)
                    {
                        if (!(inputFile.EndsWith(".txt") || inputFile.EndsWith(".txt.gz")))
                        {
                            TaggedToken[][] inputSentence = tagger.TaggedData.ReadConll(inputFile, null, true, !inputFile.EndsWith(".conll"));

                            Evaluation evaluation = new Evaluation();

                            int count = 0;

                            StreamWriter writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8);

                            foreach (TaggedToken[] sentence in inputSentence)
                            {
                                if (count % 100 == 0)
                                {
                                    Console.WriteLine($"Tagging sentence number {count}.\r");
                                }

                                count++;

                                TaggedToken[] taggedSentence = tagger.TagSentence(sentence, true, preserve);

                                evaluation.Evaluate(taggedSentence, sentence);

                                tagger.TaggedData.WriteConllGold(writer, taggedSentence, sentence, plainOutput);
                            }

                            writer.Close();

                            Console.WriteLine($"Tagging sentence number {count}.");

                            Console.WriteLine($"POS accuracy: {evaluation.GetPosAccuracy()} ({evaluation.PosCorrect} / {evaluation.PosTotal}).");

                            Console.WriteLine($"NE precision: {evaluation.GetNePrecision()}.");

                            Console.WriteLine($"NE recall:    {evaluation.GetNeRecall()}.");

                            Console.WriteLine($"NE F-score:   {evaluation.GetNeFScore()}.");
                        }
                        else
                        {
                            string fileId = Path.GetFileNameWithoutExtension(inputFile);

                            TextReader reader = OpenUtf8File(inputFile);

                            StreamWriter writer;

                            if (inputFiles.Count > 1)
                            {
                                string outputFile = $"{inputFile}{(plainOutput ? ".plain" : ".conll")}";

                                writer = new StreamWriter(new FileStream(outputFile, FileMode.Create), Encoding.UTF8);
                            }
                            else
                            {
                                writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8);
                            }

                            Tokenizer tokenizer = GetTokenizer(reader, language);

                            List<Token> sentence;

                            int sentenceIndex = 0;

                            while ((sentence = tokenizer.ReadSentence()) != null)
                            {
                                TaggedToken[] sent = new TaggedToken[sentence.Count];

                                if (tokenizer.SentenceId != null)
                                {
                                    if (!fileId.Equals(tokenizer.SentenceId))
                                    {
                                        fileId = tokenizer.SentenceId;

                                        sentenceIndex = 0;
                                    }
                                }

                                for (int j = 0; j < sentence.Count; j++)
                                {
                                    Token tok = sentence[j];

                                    var id = $"{fileId}:{sentenceIndex}:{tok.Offset}";

                                    sent[j] = new TaggedToken(tok, id);
                                }

                                TaggedToken[] taggedSent = tagger.TagSentence(sent, true, false);

                                tagger.TaggedData.WriteConllSentence(writer ?? new StreamWriter(Console.OpenStandardOutput()), taggedSent, plainOutput);

                                sentenceIndex++;
                            }

                            tokenizer.Close();

                            writer?.Close();
                        }
                    }
                }
                else if (args[i].Equals("-tokenize"))
                {
                    string inputFile = args[++i];

                    TextReader reader = OpenUtf8File(inputFile);

                    Tokenizer tokenizer = GetTokenizer(reader, language);

                    List<Token> sentence;

                    while ((sentence = tokenizer.ReadSentence()) != null)
                    {
                        if (sentence.Count == 0)
                        {
                            continue;
                        }

                        if (!plainOutput)
                        {
                            Console.Write(sentence[0].Value.Replace(' ', '_'));

                            for (int j = 1; j < sentence.Count; j++)
                            {
                                Console.Write($" {sentence[j].Value.Replace(' ', '_')}");
                            }

                            Console.WriteLine("");
                        }
                        else
                        {
                            foreach (Token token in sentence)
                            {
                                Console.WriteLine(token.Value);
                            }

                            Console.WriteLine();
                        }
                    }

                    tokenizer.Close();
                }
            }
        }
    }
}