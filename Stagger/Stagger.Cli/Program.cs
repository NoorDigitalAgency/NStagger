using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Stagger.Cli
{
    internal class Program
    {
        /**
     * Creates and returns a tokenizer for the given language.
     */
        private static Tokenizer getTokenizer(TextReader reader, string lang)
        {
            Tokenizer tokenizer;
            if (lang.Equals("sv"))
            {
                tokenizer = new SwedishTokenizer(reader);
            }
            else if (lang.Equals("en"))
            {
                tokenizer = new EnglishTokenizer(reader);
            }
            else if (lang.Equals("any"))
            {
                tokenizer = new LatinTokenizer(reader);
            }
            else
            {
                throw new ArgumentException();
            }
            return tokenizer;
        }

        /**
         * Creates and returns a tagger for the given language.
         */
        private static Tagger getTagger(
        string lang, TaggedData td, int posBeamSize, int neBeamSize)
        {
            Tagger tagger = null;
            if (lang.Equals("sv"))
            {
                tagger = new SUCTagger(
                    td, posBeamSize, neBeamSize);
            }
            else if (lang.Equals("en"))
            {
                tagger = new PTBTagger(
                    td, posBeamSize, neBeamSize);
            }
            else if (lang.Equals("any"))
            {
                tagger = new GenericTagger(
                    td, posBeamSize, neBeamSize);
            }
            else if (lang.Equals("zh"))
            {
                tagger = new CTBTagger(
                    td, posBeamSize, neBeamSize);
            }
            else
            {
                Console.WriteLine("Invalid language: " + lang);
                Environment.Exit(1);
            }
            return tagger;
        }

        private static TaggedToken[][][] getSUCFold(
            TaggedToken[][] sents, int fold)
        {
            TaggedToken[][][] parts = new TaggedToken[3][][];
            HashSet<string> fileSet = new HashSet<string>();
            foreach (TaggedToken[] sent in sents)
            {
                string fileID = sent[0].Id.Substring(0, 4);
                fileSet.Add(fileID);
            }
            List<string> files = new List<string>(fileSet);
            Collections.Sort(files);
            Debug.Assert(files.Count == 500);
            Dictionary<string, int> fileNr = new Dictionary<string, int>();
            for (int i = 0; i < files.Count; i++)
            {
                string fileID = files[i];
                fileNr[fileID] = i;
            }
            int nDev = 0, nTest = 0, nTrain = 0;
            foreach (TaggedToken[] sent in sents)
            {
                string fileID = sent[0].Id.Substring(0, 4);
                int nr = fileNr[fileID];
                if ((nr % 10) == fold) nTest++;
                else if ((((nr + 1) % 10) == fold) && (((nr / 10)) % 5 == 0)) nDev++;
                else nTrain++;
            }
            parts[0] = new TaggedToken[nTrain][];
            parts[1] = new TaggedToken[nDev][];
            parts[2] = new TaggedToken[nTest][];
            int iDev = 0, iTest = 0, iTrain = 0;
            foreach (TaggedToken[] sent in sents)
            {
                string fileID = sent[0].Id.Substring(0, 4);
                int nr = (int)fileNr[fileID];
                if ((nr % 10) == fold) parts[2][iTest++] = sent;
                else if ((((nr + 1) % 10) == fold) && (((nr / 10)) % 5 == 0))
                    parts[1][iDev++] = sent;
                else parts[0][iTrain++] = sent;
            }
            return parts;
        }

        /**
         * Splits the sentences into training/development/test data sets.
         *
         * @param sents         array of sentences
         * @param nFolds        number of folds in the experiment
         * @param devPercent    size of development set in 1/10 percent
         * @param testPercent   size of test set in 1/10 percent
         * @param i             fold number (between 0 and nFolds-1, inclusive)
         * @return              array with 3 TaggedToken[][] objects, containing
         *                      the training, development and test sets
         */
        private static TaggedToken[][][] getFold(
        TaggedToken[][] sents, int nFolds, int devPercent, int testPercent, int i)
        {
            int j, k;
            TaggedToken[][][] parts = new TaggedToken[3][][];
            List<int> order = new List<int>(sents.Length);
            for (j = 0; j < sents.Length; j++) order.Add(j);
            Collections.Shuffle(order, new Random(1));
            int nDev = (devPercent * sents.Length) / 1000;
            int nTest = (testPercent * sents.Length) / 1000;
            int nTrain = sents.Length - (nDev + nTest);
            parts[0] = new TaggedToken[nTrain][];
            parts[1] = new TaggedToken[nDev][];
            parts[2] = new TaggedToken[nTest][];
            int a = (sents.Length * i) / nFolds;
            for (j = 0, k = 0; j < a; j++) parts[0][k++] = sents[j];
            for (j = a + nDev + nTest; j < sents.Length; j++) parts[0][k++] = sents[j];
            for (j = 0; j < nDev; j++) parts[1][j] = sents[a + j];
            for (j = 0; j < nTest; j++) parts[2][j] = sents[a + nDev + j];
            return parts;
        }

        private static TextReader openUTF8File(string name)
        {
            if (name.Equals("-"))
                return new StreamReader(
                    new InputStreamReader(System.in, "UTF-8"));
            else if (name.EndsWith(".gz"))
                return new StreamReader(new InputStreamReader(
                            new GZIPInputStream(
                                new FileInputStream(name)), "UTF-8"));
            return new StreamReader(new InputStreamReader(
                        new FileInputStream(name), "UTF-8"));
        }

        public static void Main(string[] args)
        {
            string lexiconFile = null;
            string trainFile = null;
            string devFile = null;
            string modelFile = null;
            List<Dictionary> posDictionaries = new List<Dictionary>();
            List<Embedding> posEmbeddings = new List<Embedding>();
            List<Dictionary> neDictionaries = new List<Dictionary>();
            List<Embedding> neEmbeddings = new List<Embedding>();
            int posBeamSize = 8;
            int neBeamSize = 4;
            string lang = null;
            bool preserve = false;
            float embeddingSigma = 0.1f;
            bool plainOutput = false;
            string fold = null;
            int maxPosIters = 16;
            int maxNEIters = 16;
            bool extendLexicon = true;
            bool hasNE = true;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("-lexicon"))
                {
                    lexiconFile = args[++i];
                }
                else if (args[i].Equals("-dict"))
                {
                    string dest = args[++i];
                    Dictionary dict = new Dictionary();
                    try
                    {
                        dict.FromFile(args[++i]);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Can not load dictionary file.");
                        Console.WriteLine(e.StackTrace);
                        Environment.Exit(1);
                    }
                    if (dest.Equals("pos"))
                    {
                        posDictionaries.Add(dict);
                    }
                    else if (dest.Equals("ne"))
                    {
                        neDictionaries.Add(dict);
                    }
                    else if (dest.Equals("all"))
                    {
                        posDictionaries.Add(dict);
                        neDictionaries.Add(dict);
                    }
                    else
                    {
                        Console.WriteLine("Expected pos/ne/all.");
                        Environment.Exit(1);
                    }
                }
                else if (args[i].Equals("-lang"))
                {
                    lang = args[++i];
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
                    hasNE = false;
                }
                else if (args[i].Equals("-positers"))
                {
                    maxPosIters = int.Parse(args[++i]);
                }
                else if (args[i].Equals("-neiters"))
                {
                    maxNEIters = int.Parse(args[++i]);
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
                    string dest = args[++i];
                    Embedding embedding = new Embedding();
                    try
                    {
                        embedding.FromFile(args[++i]);
                        // This gives a very slight decrease in accuracy
                        // embedding.rescale(embeddingSigma);
                    }
                    catch (IOException e)
                    {
                        Console.WriteLine("Can not load embedding file.");
                        Console.WriteLine(e.StackTrace);
                        Environment.Exit(1);
                    }
                    if (dest.Equals("pos"))
                    {
                        posEmbeddings.Add(embedding);
                    }
                    else if (dest.Equals("ne"))
                    {
                        neEmbeddings.Add(embedding);
                    }
                    else if (dest.Equals("all"))
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
                    devFile = args[++i];
                }
                else if (args[i].Equals("-modelfile"))
                {
                    modelFile = args[++i];
                }
                else if (args[i].Equals("-train"))
                {
                    TaggedToken[][] trainSents = null;
                    TaggedToken[][] devSents = null;
                    if (trainFile == null ||
                       modelFile == null || lang == null)
                    {
                        Console.WriteLine("Insufficient data.");
                        Environment.Exit(1);
                    }
                    TaggedData td = new TaggedData(lang);
                    trainSents = td.ReadConll(
                        trainFile, null, true, !trainFile.EndsWith(".conll"));
                    if (devFile != null)
                        devSents = td.ReadConll(
                            devFile, null, true, !devFile.EndsWith(".conll"));
                    Console.WriteLine(
                        "Read " + trainSents.Length +
                        " training sentences and " +
                        ((devSents == null) ? 0 : devSents.Length) +
                        " development sentences.");
                    Tagger tagger = getTagger(
                        lang, td, posBeamSize, neBeamSize);
                    tagger.BuildLexicons(trainSents);
                    Lexicon lexicon = tagger.PosLexicon;
                    Console.WriteLine("POS lexicon size (corpus): " +
                                                       lexicon.Size);
                    if (lexiconFile != null)
                    {
                        if (extendLexicon)
                        {
                            Console.WriteLine(
                                "Reading lexicon: " + lexiconFile);
                        }
                        else
                        {
                            Console.WriteLine(
                                "Reading lexicon (not extending profiles): " +
                                lexiconFile);
                        }
                        lexicon.FromFile(lexiconFile, td.PosTagSet,
                                         extendLexicon);
                        Console.WriteLine("POS lexicon size (external): " +
                                           lexicon.Size);
                    }
                    tagger.PosDictionaries = posDictionaries;
                    tagger.PosEmbeddings = posEmbeddings;
                    tagger.NeDictionaries = neDictionaries;
                    tagger.NeEmbeddings = neEmbeddings;
                    tagger.MaximumPosIterations = maxPosIters;
                    tagger.MaximumNeIterations = maxNEIters;
                    tagger.Train(trainSents, devSents);
                    ObjectOutputStream writer = new ObjectOutputStream(
                        new FileOutputStream(modelFile));
                    writer.writeObject(tagger);
                    writer.close();
                }
                else if (args[i].Equals("-cross"))
                {
                    TaggedData td = new TaggedData(lang);
                    TaggedToken[][] allSents =
                        td.ReadConll(
                            trainFile, null, true,
                            !trainFile.EndsWith(".conll"));
                    Tagger tagger = getTagger(
                        lang, td, posBeamSize, neBeamSize);
                    tagger.PosDictionaries = posDictionaries;
                    tagger.PosEmbeddings = posEmbeddings;
                    tagger.NeDictionaries = neDictionaries;
                    tagger.NeEmbeddings = neEmbeddings;
                    const int nFolds = 10;
                    /*
                    int devPercent = 25;
                    int testPercent = 25;
                    */
                    Evaluation eval = new Evaluation();
                    for (int j = 0; j < nFolds; j++)
                    {
                        Evaluation localEval = new Evaluation();
                        TaggedToken[][][] parts = getSUCFold(allSents, j);
                        /*
                        TaggedToken[][][] parts = getFold(
                            allSents, nFolds, devPercent, testPercent, j);
                        */
                        Console.WriteLine(
                                                "Fold " + j + ", train (" + parts[0].Length +
                                                "), dev (" + parts[1].Length + "), test (" +
                                                parts[2].Length + ")");
                        Lexicon lexicon = tagger.PosLexicon;
                        lexicon.Clear();
                        tagger.BuildLexicons(parts[0]);
                        if (lexiconFile != null)
                            lexicon.FromFile(
                                lexiconFile, td.PosTagSet, extendLexicon);
                        tagger.Train(parts[0], parts[1]);
                        foreach (TaggedToken[] sent in parts[2])
                        {
                            TaggedToken[] taggedSent = tagger.TagSentence(
                                sent, true, false);
                            eval.Evaluate(taggedSent, sent);
                            localEval.Evaluate(taggedSent, sent);
                            //tagger.TaggedData.writeConllSentence(
                            //    System.out, taggedSent);
                            tagger.TaggedData.WriteConllGold(new StreamWriter(Console.OpenStandardOutput()), taggedSent, sent, plainOutput);
                        }
                        Console.WriteLine("Local POS accuracy: " +
                            localEval.GetPosAccuracy() + " (" +
                            localEval.PosCorrect + " / " +
                            localEval.PosTotal + ")");
                    }
                    Console.WriteLine("POS accuracy: " + eval.GetPosAccuracy() +
                                       " (" + eval.PosCorrect + " / " +
                                       eval.PosTotal + ")");
                    Console.WriteLine("NE precision: " + eval.GetNePrecision());
                    Console.WriteLine("NE recall:    " + eval.GetNeRecall());
                    Console.WriteLine("NE F-score:   " + eval.GetNeFScore());
                    Console.WriteLine("NE total:     " + eval.NeTotal);
                    Console.WriteLine("NE correct:   " + eval.NeCorrect);
                    Console.WriteLine("NE found:     " + eval.NeFound);
                }
                else if (args[i].Equals("-server"))
                {
                    if (modelFile == null || i >= args.Length - 1)
                    {
                        Console.WriteLine("Insufficient data.");
                        Environment.Exit(1);
                    }

                    IPAddress serverIP = Dns.GetHostAddresses(args[++i]).FirstOrDefault();
                    int serverPort = int.Parse(args[++i]);

                    ObjectInputStream modelReader = new ObjectInputStream(
                        new FileInputStream(modelFile));
                    Console.WriteLine("Loading Stagger model ...");
                    Tagger tagger = (Tagger)modelReader.readObject();
                    lang = tagger.TaggedData.Language;
                    modelReader.close();

                    TcpListener ss = new TcpListener(serverIP, serverPort);
                    ss.Start(4);
                    while (true)
                    {
                        Socket sock = null;
                        try
                        {
                            sock = ss.AcceptSocket();
                            Console.WriteLine("Connected to " +
                                sock.RemoteEndPoint);
                            NetworkStream ins = new NetworkStream(sock);
                            byte[] lenBuf = new byte[4];
                            if (ins.Read(lenBuf) != 4)
                            {
                                throw new IOException("Can not read length.");
                            }
                            int len = BitConverter.ToInt32(lenBuf);
                            if (len < 1 || len > 100000)
                            {
                                throw new IOException("Invalid data size: " + len);
                            }
                            byte[] dataBuf = new byte[len];
                            if (ins.Read(dataBuf) != len)
                            {
                                throw new IOException("Can not read data.");
                            }
                            StringReader reader = new StringReader(Encoding.UTF8.GetString(dataBuf));

                            StreamWriter writer = new StreamWriter(
                                new OutputStreamWriter(
                                    sock.getOutputStream(), "UTF-8"));

                            Tokenizer tokenizer = getTokenizer(reader, lang);
                            List<Token> sentence;
                            int sentIdx = 0;
                            string fileID = "net";
                            while ((sentence = tokenizer.ReadSentence()) != null)
                            {
                                TaggedToken[] sent =
                                    new TaggedToken[sentence.Count];
                                if (tokenizer.SentenceId != null)
                                {
                                    if (!fileID.Equals(tokenizer.SentenceId))
                                    {
                                        fileID = tokenizer.SentenceId;
                                        sentIdx = 0;
                                    }
                                }
                                for (int j = 0; j < sentence.Count; j++)
                                {
                                    Token tok = sentence[j];
                                    string id;
                                    id = fileID + ":" + sentIdx + ":" + tok.Offset;
                                    sent[j] = new TaggedToken(tok, id);
                                }
                                TaggedToken[] taggedSent =
                                    tagger.TagSentence(sent, true, false);
                                tagger.TaggedData.WriteConllSentence(
                                    (writer == null) ? new StreamWriter(Console.OpenStandardOutput()) : writer,
                                    taggedSent, plainOutput);
                                sentIdx++;
                            }
                            tokenizer.Close();
                            if (sock.Connected)
                            {
                                Console.WriteLine("Closing connection to " +
                                    sock.RemoteEndPoint);
                                writer.Close();
                            }
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e.StackTrace);
                            if (sock != null)
                            {
                                Console.WriteLine("Connection failed to " +
                                    sock.RemoteEndPoint);
                                if (sock.Connected) sock.Close();
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
                        inputFiles.Add(args[i]);
                    if (inputFiles.Count < 1)
                    {
                        Console.WriteLine("No files to tag.");
                        Environment.Exit(1);
                    }
                    TaggedToken[][] inputSents = null;

                    ObjectInputStream modelReader = new ObjectInputStream(
                        new FileInputStream(modelFile));
                    Console.WriteLine("Loading Stagger model ...");
                    Tagger tagger = (Tagger)modelReader.readObject();
                    lang = tagger.TaggedData.Language;
                    modelReader.close();

                    // TODO: experimental feature, might remove later
                    tagger.ExtendLexicon = extendLexicon;
                    if (!hasNE) tagger.HasNe = false;

                    foreach (string inputFile in inputFiles)
                    {
                        if (!(inputFile.EndsWith(".txt") ||
                             inputFile.EndsWith(".txt.gz")))
                        {
                            inputSents = tagger.TaggedData.ReadConll(
                                inputFile, null, true,
                                !inputFile.EndsWith(".conll"));
                            Evaluation eval = new Evaluation();
                            int count = 0;
                            StreamWriter writer = new StreamWriter(
                                new OutputStreamWriter(Console.OpenStandardOutput(), "UTF-8"));
                            foreach (TaggedToken[] sent in inputSents)
                            {
                                if (count % 100 == 0)
                                    Console.WriteLine("Tagging sentence nr: " +
                                                     count + "\r");
                                count++;
                                TaggedToken[] taggedSent =
                                    tagger.TagSentence(sent, true, preserve);

                                eval.Evaluate(taggedSent, sent);
                                tagger.TaggedData.WriteConllGold(
                                    writer, taggedSent, sent, plainOutput);
                            }
                            writer.Close();
                            Console.WriteLine("Tagging sentence nr: " + count);
                            Console.WriteLine(
                                "POS accuracy: " + eval.GetPosAccuracy() +
                                " (" + eval.PosCorrect + " / " +
                                eval.PosTotal + ")");
                            Console.WriteLine(
                                "NE precision: " + eval.GetNePrecision());
                            Console.WriteLine(
                                "NE recall:    " + eval.GetNeRecall());
                            Console.WriteLine(
                                "NE F-score:   " + eval.GetNeFScore());
                        }
                        else
                        {
                            string fileID = Path.GetFileNameWithoutExtension(inputFile);
                            TextReader reader = openUTF8File(inputFile);
                            StreamWriter writer;
                            if (inputFiles.Count > 1)
                            {
                                string outputFile = inputFile +
                                    (plainOutput ? ".plain" : ".conll");
                                writer = new StreamWriter(
                                                                new OutputStreamWriter(
                                                                    new FileOutputStream(
                                                                        outputFile), "UTF-8"));
                            }
                            else
                            {
                                writer = new StreamWriter(
                                    new OutputStreamWriter(Console.OpenStandardOutput(), "UTF-8"));
                            }
                            Tokenizer tokenizer = getTokenizer(reader, lang);
                            List<Token> sentence;
                            int sentIdx = 0;
                            while ((sentence = tokenizer.ReadSentence()) != null)
                            {
                                TaggedToken[] sent =
                                    new TaggedToken[sentence.Count];
                                if (tokenizer.SentenceId != null)
                                {
                                    if (!fileID.Equals(tokenizer.SentenceId))
                                    {
                                        fileID = tokenizer.SentenceId;
                                        sentIdx = 0;
                                    }
                                }
                                for (int j = 0; j < sentence.Count; j++)
                                {
                                    Token tok = sentence[j];
                                    var id = fileID + ":" + sentIdx + ":" +
                                                tok.Offset;
                                    sent[j] = new TaggedToken(tok, id);
                                }
                                TaggedToken[] taggedSent =
                                    tagger.TagSentence(sent, true, false);
                                tagger.TaggedData.WriteConllSentence(
                                    (writer == null) ? new StreamWriter(Console.OpenStandardOutput()) : writer,
                                    taggedSent, plainOutput);
                                sentIdx++;
                            }
                            tokenizer.Close();
                            if (writer != null) writer.Close();
                        }
                    }
                }
                else if (args[i].Equals("-tokenize"))
                {
                    string inputFile = args[++i];
                    TextReader reader = openUTF8File(inputFile);
                    Tokenizer tokenizer = getTokenizer(reader, lang);
                    List<Token> sentence;
                    while ((sentence = tokenizer.ReadSentence()) != null)
                    {
                        if (sentence.Count == 0) continue;
                        if (!plainOutput)
                        {
                            Console.Write(
                                sentence[0].Value.Replace(' ', '_'));
                            for (int j = 1; j < sentence.Count; j++)
                            {
                                Console.Write(
                                    " " +
                                    sentence[j].Value.Replace(' ', '_'));
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
