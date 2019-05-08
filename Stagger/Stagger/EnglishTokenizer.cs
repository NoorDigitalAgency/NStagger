using System.IO;

namespace Stagger
{
    public class EnglishTokenizer : Tokenizer
    {
        public const int YYEOF = -1;

        private const int ZZ_BUFFERSIZE = 16384;

        public const int YYINITIAL = 0;

        private readonly int[] ZZ_LEXSTATE = { 0, 0 };

        private const string ZZ_CMAP_PACKED = "";

        private readonly char[] ZZ_CMAP = zzUnpackCMap(ZZ_CMAP_PACKED);

        private readonly int[] ZZ_ACTION = zzUnpackAction();

        private const string ZZ_ACTION_PACKED_0 = "";

        private static int[] zzUnpackAction()
        {
            int[] result = new int[159];

            const int offset = 0;

            zzUnpackAction(ZZ_ACTION_PACKED_0, offset, result);

            return result;
        }

        private static int zzUnpackAction(string packed, int offset, int[] result)
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

            return j;
        }

        private readonly int[] ZZ_ROWMAP = zzUnpackRowMap();

        private static int[] zzUnpackRowMap()
        {
            int[] result = new int[159];

            const int offset = 0;

            zzUnpackRowMap(ZZ_ROWMAP_PACKED_0, offset, result);

            return result;
        }

        private static int zzUnpackRowMap(string packed, int offset, int[] result)
        {
            int i = 0;

            int j = offset;

            int l = packed.Length;

            while (i < l)
            {
                int high = packed[i++] << 16;

                result[j++] = high | packed[i++];
            }

            return j;
        }

        private readonly int[] ZZ_TRANS = zzUnpackTrans();

        private const string ZZ_TRANS_PACKED_0 = "";

        private static int[] zzUnpackTrans()
        {
            int[] result = new int[9170];

            const int offset = 0;

            zzUnpackTrans(ZZ_TRANS_PACKED_0, offset, result);

            return result;
        }

        private static int zzUnpackTrans(string packed, int offset, int[] result)
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
            return j;
        }

        private const int ZZ_UNKNOWN_ERROR = 0;

        private const int ZZ_NO_MATCH = 1;

        private const int ZZ_PUSHBACK_2BIG = 2;
        
        private readonly string[] ZZ_ERROR_MSG = {

            "Unknown internal scanner error",

            "Error: could not match input",

            "Error: pushback value was too large"
        };

        private readonly int[] ZZ_ATTRIBUTE = zzUnpackAttribute();

        private const string ZZ_ATTRIBUTE_PACKED_0 = "";

        private static int[] zzUnpackAttribute()
        {
            int[] result = new int[159];
            int offset = 0;
            offset = zzUnpackAttribute(ZZ_ATTRIBUTE_PACKED_0, offset, result);
            return result;
        }

        private static int zzUnpackAttribute(String packed, int offset, int[] result)
        {
            int i = 0;       /* index in packed string  */
            int j = offset;  /* index in unpacked array */
            int l = packed.length();
            while (i < l)
            {
                int count = packed.charAt(i++);
                int value = packed.charAt(i++);
                do result[j++] = value; while (--count > 0);
            }
            return j;
        }

        /** the input device */
        private java.io.Reader zzReader;

        /** the current state of the DFA */
        private int zzState;

        /** the current lexical state */
        private int zzLexicalState = YYINITIAL;

        /** this buffer contains the current text to be matched and is
            the source of the yytext() string */
        private char[] zzBuffer = new char[ZZ_BUFFERSIZE];

        /** the textposition at the last accepting state */
        private int zzMarkedPos;

        /** the current text position in the buffer */
        private int zzCurrentPos;

        /** startRead marks the beginning of the yytext() string in the buffer */
        private int zzStartRead;

        /** endRead marks the last character in the buffer, that has been read
            from input */
        private int zzEndRead;

        /** number of newlines encountered up to the start of the matched text */
        private int yyline;

        /** the number of characters up to the start of the matched text */
        private int yychar;

        /**
         * the number of characters from the last newline up to the start of the 
         * matched text
         */
        private int yycolumn;

        /** 
         * zzAtBOL == true iff the scanner is currently at the beginning of a line
         */
        private boolean zzAtBOL = true;

        /** zzAtEOF == true iff the scanner is at the EOF */
        private boolean zzAtEOF;

        /** denotes if the user-EOF-code has already been executed */
        private boolean zzEOFDone;

        /** 
         * The number of occupied positions in zzBuffer beyond zzEndRead.
         * When a lead/high surrogate has been read from the input stream
         * into the final zzBuffer position, this will have a value of 1;
         * otherwise, it will have a value of 0.
         */
        private int zzFinalHighSurrogate = 0;

        /* user code: */
        public ArrayList<Token> readSentence()
        {
            ArrayList<Token> sentence = new ArrayList<Token>();
    Token token, lastNonSpace = null, lastSpace = null;

    while((token = yylex()) != null) {
        if(token.isSpace()) {
            if(token.type == Token.TOK_NEWLINES) {
                if(!sentence.isEmpty()) return sentence;
            }
    lastSpace = token;
        } else {
            if(!sentence.isEmpty()) {
                if(lastNonSpace != null &&
                   lastNonSpace.value.endsWith(".") &&
                   lastNonSpace.value.length() > 1 &&
                   token.isCapitalized())
                {
                    yypushback(yylength());
                    return sentence;
                } else if(token.type == Token.TOK_SENT_FINAL) {
                    if(lastNonSpace != null &&
                       lastNonSpace.value.length() == 1 &&
                       lastNonSpace.isCapitalized())
                    {
                    } else {
                        sentence.add(token);
                        return sentence;
                    }
                }
            }
            // I admit this is not pretty.
            if(token.type == Token.TOK_LATIN) {
                String textLower = token.value.toLowerCase();
int length = token.value.length();
                if(textLower.endsWith("n't")) {
                    if(textLower.equals("can't")) {
                        sentence.add(new Token(
                            Token.TOK_LATIN, token.value.substring(0,3),
                            token.offset));
                        sentence.add(new Token(
                            Token.TOK_LATIN, token.value.substring(2),
                            token.offset+2));
                    } else {
                        sentence.add(new Token(
                            Token.TOK_LATIN,
                            token.value.substring(0, length-3),
                            token.offset));
                        sentence.add(new Token(
                            Token.TOK_LATIN,
                            token.value.substring(length-3),
                            token.offset+length-3));
                    }
                } else {
                    sentence.add(token);
                }
            } else {
                sentence.add(token);
            }
            lastNonSpace = token;
        }
    }
    if(sentence.isEmpty()) return null;
    return sentence;
}


  /**
   * Creates a new scanner
   *
   * @param   in  the java.io.Reader to read input from.
   */
  public EnglishTokenizer(java.io.Reader in)
{
    this.zzReader = in;
}


/** 
 * Unpacks the compressed character translation table.
 *
 * @param packed   the packed character translation table
 * @return         the unpacked character translation table
 */
private static char[] zzUnpackCMap(String packed)
{
    char[] map = new char[0x110000];
    int i = 0;  /* index in packed string  */
    int j = 0;  /* index in unpacked array */
    while (i < 548)
    {
        int count = packed.charAt(i++);
        char value = packed.charAt(i++);
        do map[j++] = value; while (--count > 0);
    }
    return map;
}


/**
 * Refills the input buffer.
 *
 * @return      <code>false</code>, iff there was new input.
 * 
 * @exception   java.io.IOException  if any I/O-Error occurs
 */
private boolean zzRefill()
{

    /* first: make room (if you can) */
    if (zzStartRead > 0) {
        zzEndRead += zzFinalHighSurrogate;
        zzFinalHighSurrogate = 0;
        System.arraycopy(zzBuffer, zzStartRead,
                         zzBuffer, 0,
                         zzEndRead - zzStartRead);

        /* translate stored positions */
        zzEndRead -= zzStartRead;
        zzCurrentPos -= zzStartRead;
        zzMarkedPos -= zzStartRead;
        zzStartRead = 0;
    }

    /* is the buffer big enough? */
    if (zzCurrentPos >= zzBuffer.length - zzFinalHighSurrogate) {
        /* if not: blow it up */
        char newBuffer[] = new char[zzBuffer.length * 2];
        System.arraycopy(zzBuffer, 0, newBuffer, 0, zzBuffer.length);
        zzBuffer = newBuffer;
        zzEndRead += zzFinalHighSurrogate;
        zzFinalHighSurrogate = 0;
    }

    /* fill the buffer with new input */
    int requested = zzBuffer.length - zzEndRead;
    int numRead = zzReader.read(zzBuffer, zzEndRead, requested);

    /* not supposed to occur according to specification of java.io.Reader */
    if (numRead == 0) {
        throw new java.io.IOException("Reader returned 0 characters. See JFlex examples for workaround.");
    }
    if (numRead > 0) {
        zzEndRead += numRead;
        /* If numRead == requested, we might have requested to few chars to
           encode a full Unicode character. We assume that a Reader would
           otherwise never return half characters. */
        if (numRead == requested)
        {
            if (Character.isHighSurrogate(zzBuffer[zzEndRead - 1]))
            {
                --zzEndRead;
                zzFinalHighSurrogate = 1;
            }
        }
        /* potentially more input available */
        return false;
    }

    /* numRead < 0 ==> end of stream */
    return true;
}


/**
 * Closes the input stream.
 */
public final void yyclose()
{
    zzAtEOF = true;            /* indicate end of file */
    zzEndRead = zzStartRead;  /* invalidate buffer    */

    if (zzReader != null)
      zzReader.close();
}


/**
 * Resets the scanner to read from a new input stream.
 * Does not close the old reader.
 *
 * All internal variables are reset, the old input stream 
 * <b>cannot</b> be reused (internal buffer is discarded and lost).
 * Lexical state is set to <tt>ZZ_INITIAL</tt>.
 *
 * Internal scan buffer is resized down to its initial length, if it has grown.
 *
 * @param reader   the new input stream 
 */
public final void yyreset(java.io.Reader reader)
{
    zzReader = reader;
    zzAtBOL = true;
    zzAtEOF = false;
    zzEOFDone = false;
    zzEndRead = zzStartRead = 0;
    zzCurrentPos = zzMarkedPos = 0;
    zzFinalHighSurrogate = 0;
    yyline = yychar = yycolumn = 0;
    zzLexicalState = YYINITIAL;
    if (zzBuffer.length > ZZ_BUFFERSIZE)
        zzBuffer = new char[ZZ_BUFFERSIZE];
}


/**
 * Returns the current lexical state.
 */
public final int yystate()
{
    return zzLexicalState;
}


/**
 * Enters a new lexical state
 *
 * @param newState the new lexical state
 */
public final void yybegin(int newState)
{
    zzLexicalState = newState;
}


/**
 * Returns the text matched by the current regular expression.
 */
public final String yytext()
{
    return new String(zzBuffer, zzStartRead, zzMarkedPos - zzStartRead);
}


/**
 * Returns the character at position <tt>pos</tt> from the 
 * matched text. 
 * 
 * It is equivalent to yytext().charAt(pos), but faster
 *
 * @param pos the position of the character to fetch. 
 *            A value from 0 to yylength()-1.
 *
 * @return the character at position pos
 */
public final char yycharat(int pos)
{
    return zzBuffer[zzStartRead + pos];
}


/**
 * Returns the length of the matched text region.
 */
public final int yylength()
{
    return zzMarkedPos - zzStartRead;
}


/**
 * Reports an error that occured while scanning.
 *
 * In a wellformed scanner (no or only correct usage of 
 * yypushback(int) and a match-all fallback rule) this method 
 * will only be called with things that "Can't Possibly Happen".
 * If this method is called, something is seriously wrong
 * (e.g. a JFlex bug producing a faulty scanner etc.).
 *
 * Usual syntax/scanner level error handling should be done
 * in error fallback rules.
 *
 * @param   errorCode  the code of the errormessage to display
 */
private void zzScanError(int errorCode)
{
    String message;
    try
    {
        message = ZZ_ERROR_MSG[errorCode];
    }
    catch (ArrayIndexOutOfBoundsException e)
    {
        message = ZZ_ERROR_MSG[ZZ_UNKNOWN_ERROR];
    }

    throw new Error(message);
}


/**
 * Pushes the specified amount of characters back into the input stream.
 *
 * They will be read again by then next call of the scanning method
 *
 * @param number  the number of characters to be read again.
 *                This number must not be greater than yylength()!
 */
public void yypushback(int number)
{
    if (number > yylength())
        zzScanError(ZZ_PUSHBACK_2BIG);

    zzMarkedPos -= number;
}


/**
 * Resumes scanning until the next regular expression is matched,
 * the end of input is encountered or an I/O-Error occurs.
 *
 * @return      the next token
 * @exception   java.io.IOException  if any I/O-Error occurs
 */
public Token yylex() 
{
    int zzInput;
    int zzAction;

    // cached fields:
    int zzCurrentPosL;
    int zzMarkedPosL;
    int zzEndReadL = zzEndRead;
    char []
    zzBufferL = zzBuffer;
    char []
    zzCMapL = ZZ_CMAP;

    int []
    zzTransL = ZZ_TRANS;
    int []
    zzRowMapL = ZZ_ROWMAP;
    int []
    zzAttrL = ZZ_ATTRIBUTE;

    while (true) {
        zzMarkedPosL = zzMarkedPos;

        yychar += zzMarkedPosL - zzStartRead;

        zzAction = -1;

        zzCurrentPosL = zzCurrentPos = zzStartRead = zzMarkedPosL;

        zzState = ZZ_LEXSTATE[zzLexicalState];

        // set up zzAction for empty match case:
        int zzAttributes = zzAttrL[zzState];
        if ((zzAttributes & 1) == 1)
        {
            zzAction = zzState;
        }


        zzForAction:
        {
            while (true)
            {

                if (zzCurrentPosL < zzEndReadL)
                {
                    zzInput = Character.codePointAt(zzBufferL, zzCurrentPosL, zzEndReadL);
                    zzCurrentPosL += Character.charCount(zzInput);
                }
                else if (zzAtEOF)
                {
                    zzInput = YYEOF;
                    break zzForAction;
                }
                else
                {
                    // store back cached positions
                    zzCurrentPos = zzCurrentPosL;
                    zzMarkedPos = zzMarkedPosL;
                    boolean eof = zzRefill();
                    // get translated positions and possibly new buffer
                    zzCurrentPosL = zzCurrentPos;
                    zzMarkedPosL = zzMarkedPos;
                    zzBufferL = zzBuffer;
                    zzEndReadL = zzEndRead;
                    if (eof)
                    {
                        zzInput = YYEOF;
                        break zzForAction;
                    }
                    else
                    {
                        zzInput = Character.codePointAt(zzBufferL, zzCurrentPosL, zzEndReadL);
                        zzCurrentPosL += Character.charCount(zzInput);
                    }
                }
                int zzNext = zzTransL[zzRowMapL[zzState] + zzCMapL[zzInput]];
                if (zzNext == -1) break zzForAction;
                zzState = zzNext;

                zzAttributes = zzAttrL[zzState];
                if ((zzAttributes & 1) == 1)
                {
                    zzAction = zzState;
                    zzMarkedPosL = zzCurrentPosL;
                    if ((zzAttributes & 8) == 8) break zzForAction;
                }

            }
        }

        // store back cached position
        zzMarkedPos = zzMarkedPosL;

        if (zzInput == YYEOF && zzStartRead == zzCurrentPos)
        {
            zzAtEOF = true;
            return null;
        }
        else
        {
            switch (zzAction < 0 ? zzAction : ZZ_ACTION[zzAction])
            {
                case 1:
                    {
                        return (new Token(Token.TOK_UNKNOWN, yytext(), yychar));
                    }
                // fall through
                case 21: break;
                case 2:
                    {
                        return (new Token(Token.TOK_SYMBOL, yytext(), yychar));
                    }
                // fall through
                case 22: break;
                case 3:
                    {
                        return (new Token(Token.TOK_LATIN, yytext(), yychar));
                    }
                // fall through
                case 23: break;
                case 4:
                    {
                        return (new Token(Token.TOK_SPACE, yytext(), yychar));
                    }
                // fall through
                case 24: break;
                case 5:
                    {
                        return (new Token(Token.TOK_NEWLINE, yytext(), yychar));
                    }
                // fall through
                case 25: break;
                case 6:
                    {
                        return (new Token(Token.TOK_NUMBER, yytext(), yychar));
                    }
                // fall through
                case 26: break;
                case 7:
                    {
                        return (new Token(Token.TOK_SENT_FINAL, yytext(), yychar));
                    }
                // fall through
                case 27: break;
                case 8:
                    {
                        return (new Token(Token.TOK_DASH, yytext(), yychar));
                    }
                // fall through
                case 28: break;
                case 9:
                    {
                        return (new Token(Token.TOK_GREEK, yytext(), yychar));
                    }
                // fall through
                case 29: break;
                case 10:
                    {
                        return (new Token(Token.TOK_ARABIC, yytext(), yychar));
                    }
                // fall through
                case 30: break;
                case 11:
                    {
                        return (new Token(Token.TOK_NAGARI, yytext(), yychar));
                    }
                // fall through
                case 31: break;
                case 12:
                    {
                        return (new Token(Token.TOK_KANA, yytext(), yychar));
                    }
                // fall through
                case 32: break;
                case 13:
                    {
                        return (new Token(Token.TOK_HANGUL, yytext(), yychar));
                    }
                // fall through
                case 33: break;
                case 14:
                    {
                        return (new Token(Token.TOK_HANZI, yytext(), yychar));
                    }
                // fall through
                case 34: break;
                case 15:
                    {
                        return (new Token(Token.TOK_SMILEY, yytext(), yychar));
                    }
                // fall through
                case 35: break;
                case 16:
                    {
                        return (new Token(Token.TOK_SPACES, yytext(), yychar));
                    }
                // fall through
                case 36: break;
                case 17:
                    {
                        return (new Token(Token.TOK_NEWLINES, yytext(), yychar));
                    }
                // fall through
                case 37: break;
                case 18:
                    {
                        return (new Token(Token.TOK_URL, yytext(), yychar));
                    }
                // fall through
                case 38: break;
                case 19:
                    {
                        return (new Token(Token.TOK_EMAIL, yytext(), yychar));
                    }
                // fall through
                case 39: break;
                case 20:
                    {
                        sentID = yytext().substring(6, yylength() - 1);
                    }
                // fall through
                case 40: break;
                default:
                    zzScanError(ZZ_NO_MATCH);
            }
        }
    }
}

public EnglishTokenizer(StringReader reader)
        {
        }

        public override Token Tokenize()
        {
            throw new System.NotImplementedException();
        }

        public override void Close()
        {
            throw new System.NotImplementedException();
        }

        public override Token[] ReadSentence()
        {
            throw new System.NotImplementedException();
        }

        public override void Reset(StringReader reader)
        {
            throw new System.NotImplementedException();
        }

        public override int CharactersCount { get; protected set; }

        public override string SentenceId { get; protected set; }
    }
}