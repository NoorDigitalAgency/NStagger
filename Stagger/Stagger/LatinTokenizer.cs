using System;
using System.Collections.Generic;
using System.IO;

namespace Stagger
{
    [Serializable]
    public partial class LatinTokenizer : Tokenizer
    {
        public const int Eof = -1;

        private const int bufferSize = 16384;

        public const int Initial = 0;

        private readonly int[] lexicalStates = { 0, 0 };

        private readonly char[] cMap = UnpackCMap(CMapPacked);

        private readonly int[] action = UnpackAction();

        private static int[] UnpackAction()
        {
            int[] result = new int[107];

            const int offset = 0;

            UnpackAction(ActionPacked, offset, result);

            return result;
        }

        private static void UnpackAction(string packed, int offset, IList<int> result)
        {
            int i = 0;

            int j = offset;

            int l = packed.Length;

            while (i < l)
            {
                int count = packed[i++];

                int value = packed[i++];

                do
                {
                    result[j++] = value;

                } while (--count > 0);
            }
        }


        private readonly int[] rowMap = UnpackRowMap();

        private static int[] UnpackRowMap()
        {
            int[] result = new int[107];

            const int offset = 0;

            UnpackRowMap(RowMapPacked, offset, result);

            return result;
        }

        private static void UnpackRowMap(string packed, int offset, IList<int> result)
        {
            int i = 0;

            int j = offset;

            int l = packed.Length;

            while (i < l)
            {
                int high = packed[i++] << 16;

                result[j++] = high | packed[i++];
            }
        }

        private readonly int[] transitions = UnpackTransitions();

        private static int[] UnpackTransitions()
        {
            int[] result = new int[5676];

            const int offset = 0;

            UnpackTransitions(TransitionPacked, offset, result);

            return result;
        }

        private static void UnpackTransitions(string packed, int offset, IList<int> result)
        {
            int i = 0;

            int j = offset;

            int l = packed.Length;

            while (i < l)
            {
                int count = packed[i++];

                int value = packed[i++];

                value--;

                do
                {
                    result[j++] = value;

                } while (--count > 0);
            }
        }

        private const int unknownError = 0;

        private const int noMatch = 1;

        private const int pushbackTooBig = 2;

        private readonly string[] errorMessages = { "Unknown internal scanner error", "Error: could not match input", "Error: pushback value was too large" };

        private readonly int[] attribute = UnpackAttribute();

        private static int[] UnpackAttribute()
        {
            int[] result = new int[107];

            const int offset = 0;

            UnpackAttribute(AttributePacked, offset, result);

            return result;
        }

        private static void UnpackAttribute(string packed, int offset, IList<int> result)
        {
            int i = 0;

            int j = offset;

            int l = packed.Length;

            while (i < l)
            {
                int count = packed[i++];

                int value = packed[i++];

                do
                {
                    result[j++] = value;

                } while (--count > 0);
            }
        }

        private TextReader reader;

        private int state;

        private char[] buffer = new char[bufferSize];

        private int markedPosition;

        private int currentPosition;

        private int startRead;

        private int endRead;

        private bool atEof;

        private int finalHighSurrogate;

        public override List<Token> ReadSentence()
        {
            List<Token> sentence = new List<Token>();

            Token token, lastNonSpace = null, lastSpace = null;

            while ((token = Tokenize()) != null)
            {
                if (token.IsSpace)
                {
                    if (token.Type == TokenType.Newlines)
                    {
                        if (!sentence.IsEmpty()) return sentence;
                    }

                    lastSpace = token;
                }
                else
                {
                    if (!sentence.IsEmpty())
                    {
                        if (lastSpace != null && lastSpace.Type != TokenType.Space && token.IsCapitalized)
                        {
                            Pushback(Length);

                            return sentence;
                        }
                        else if (lastNonSpace != null && lastNonSpace.Value.EndsWith(".") && token.IsCapitalized)
                        {
                            Pushback(Length);

                            return sentence;
                        }
                        else if (token.Type == TokenType.SentenceFinal)
                        {
                            sentence.Add(token);

                            return sentence;
                        }
                    }

                    sentence.Add(token);

                    lastNonSpace = token;
                }
            }
            return sentence.IsEmpty() ? null : sentence;
        }

        public LatinTokenizer(TextReader reader)
        {
            this.reader = reader;
        }

        private static char[] UnpackCMap(string packed)
        {
            char[] map = new char[0x110000];

            int i = 0;

            int j = 0;

            while (i < 540)
            {
                int count = packed[i++];

                char value = packed[i++];

                do
                {
                    map[j++] = value;

                } while (--count > 0);
            }

            return map;
        }


        private bool Refill()
        {
            if (startRead > 0)
            {
                endRead += finalHighSurrogate;

                finalHighSurrogate = 0;

                Array.Copy(buffer, startRead, buffer, 0, endRead - startRead);

                endRead -= startRead;

                currentPosition -= startRead;

                markedPosition -= startRead;

                startRead = 0;
            }

            if (currentPosition >= buffer.Length - finalHighSurrogate)
            {
                char[] newBuffer = new char[buffer.Length * 2];

                Array.Copy(buffer, 0, newBuffer, 0, buffer.Length);

                buffer = newBuffer;

                endRead += finalHighSurrogate;

                finalHighSurrogate = 0;
            }

            int requested = buffer.Length - endRead;

            int numRead = reader.Read(buffer, endRead, requested);

            if (numRead > 0)
            {
                endRead += numRead;

                if (numRead == requested)
                {
                    if (Character.IsHighSurrogate(buffer[endRead - 1]))
                    {
                        --endRead;

                        finalHighSurrogate = 1;
                    }
                }

                return false;
            }

            return true;
        }

        public override void Close()
        {
            atEof = true;

            endRead = startRead;

            reader?.Close();
        }


        public override void Reset(TextReader newReader)
        {
            reader = newReader;

            atEof = false;

            endRead = startRead = 0;

            currentPosition = markedPosition = 0;

            finalHighSurrogate = 0;

            CharactersCount = 0;

            LexicalState = Initial;

            if (buffer.Length > bufferSize)
            {
                buffer = new char[bufferSize];
            }
        }

        public int LexicalState { get; set; } = Initial;

        public string Text => new string(buffer, startRead, markedPosition - startRead);

        public char this[int index] => buffer[startRead + index];

        public int Length => markedPosition - startRead;

        private void ScanError(int errorCode)
        {
            string message;

            try
            {
                message = errorMessages[errorCode];
            }
            catch (IndexOutOfRangeException)
            {
                message = errorMessages[unknownError];
            }

            throw new Exception(message);
        }


        public void Pushback(int number)
        {
            if (number > Length)
            {
                ScanError(pushbackTooBig);
            }

            markedPosition -= number;
        }

        public override Token Tokenize()
        {
            int endReadLocal = endRead;

            char[] bufferLocal = buffer;

            char[] cMapLocal = cMap;

            int[] transitionLocal = transitions;

            int[] rowMapLocal = rowMap;

            int[] attributeLocal = attribute;

            while (true)
            {
                var markedPosLocal = markedPosition;

                CharactersCount += markedPosLocal - startRead;

                var actionLocal = -1;

                var currentPosLocal = currentPosition = startRead = markedPosLocal;

                state = lexicalStates[LexicalState];

                int attributes = attributeLocal[state];

                if ((attributes & 1) == 1)
                {
                    actionLocal = state;
                }

                int input;

                while (true)
                {
                    if (currentPosLocal < endReadLocal)
                    {
                        input = Character.CodePointAt(bufferLocal, currentPosLocal, endReadLocal);

                        currentPosLocal += Character.CharCount(input);
                    }
                    else if (atEof)
                    {
                        input = Eof;

                        break;
                    }
                    else
                    {
                        currentPosition = currentPosLocal;

                        markedPosition = markedPosLocal;

                        bool eof = Refill();

                        currentPosLocal = currentPosition;

                        markedPosLocal = markedPosition;

                        bufferLocal = buffer;

                        endReadLocal = endRead;

                        if (eof)
                        {
                            input = Eof;

                            break;
                        }
                        else
                        {
                            input = Character.CodePointAt(bufferLocal, currentPosLocal, endReadLocal);

                            currentPosLocal += Character.CharCount(input);
                        }
                    }

                    int next = transitionLocal[rowMapLocal[state] + cMapLocal[input]];

                    if (next == -1)
                    {
                        break;
                    }

                    state = next;

                    attributes = attributeLocal[state];

                    if ((attributes & 1) == 1)
                    {
                        actionLocal = state;

                        markedPosLocal = currentPosLocal;

                        if ((attributes & 8) == 8)
                        {
                            break;
                        }
                    }
                }

                markedPosition = markedPosLocal;

                if (input == Eof && startRead == currentPosition)
                {
                    atEof = true;

                    return null;
                }
                else
                {
                    switch (actionLocal < 0 ? actionLocal : action[actionLocal])
                    {
                        case 1: return new Token(TokenType.Unknown, Text, CharactersCount);

                        case 21: break;

                        case 2: return new Token(TokenType.Symbol, Text, CharactersCount);

                        case 22: break;

                        case 3: return new Token(TokenType.Latin, Text, CharactersCount);

                        case 23: break;

                        case 4: return new Token(TokenType.Space, Text, CharactersCount);

                        case 24: break;

                        case 5: return new Token(TokenType.Newline, Text, CharactersCount);

                        case 25: break;

                        case 6: return new Token(TokenType.Number, Text, CharactersCount);

                        case 26: break;

                        case 7: return new Token(TokenType.SentenceFinal, Text, CharactersCount);

                        case 27: break;

                        case 8: return new Token(TokenType.Dash, Text, CharactersCount);

                        case 28: break;

                        case 9: return new Token(TokenType.Greek, Text, CharactersCount);

                        case 29: break;

                        case 10: return new Token(TokenType.Arabic, Text, CharactersCount);

                        case 30: break;

                        case 11: return new Token(TokenType.Nagari, Text, CharactersCount);

                        case 31: break;

                        case 12: return new Token(TokenType.Kana, Text, CharactersCount);

                        case 32: break;

                        case 13: return new Token(TokenType.Hangul, Text, CharactersCount);

                        case 33: break;

                        case 14: return new Token(TokenType.Hanzi, Text, CharactersCount);

                        case 34: break;

                        case 15: return new Token(TokenType.Smiley, Text, CharactersCount);

                        case 35: break;

                        case 16: return new Token(TokenType.Spaces, Text, CharactersCount);

                        case 36: break;

                        case 17: return new Token(TokenType.Newlines, Text, CharactersCount);

                        case 37: break;

                        case 18: return new Token(TokenType.Url, Text, CharactersCount);

                        case 38: break;

                        case 19: return new Token(TokenType.Email, Text, CharactersCount);

                        case 39: break;

                        case 20: SentenceId = Text.Substring(6, Length - 1); break;

                        case 40: break;

                        default: ScanError(noMatch); break;
                    }
                }
            }
        }

        public override int CharactersCount { get; protected set; }

        public override string SentenceId { get; protected set; }
    }
}