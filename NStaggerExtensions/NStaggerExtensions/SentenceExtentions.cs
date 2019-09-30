using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NStagger;

namespace NStaggerExtensions
{
    public static class SentenceExtensions
    {
        private static readonly Regex[] regexList =
        {
            new Regex(@"([?!]) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), // 0

            new Regex(@"(\.[\.]+) +(['""([\u00bf\u00A1\p{Pi}]*[\p{Lu}])"), // 1

            new Regex(@"([?!\.][\ ]*['"")\]\p{Pf}]+) +(['""([\u00bf\u00A1\p{Pi}]*[\ ]*[\p{Lu}])"), // 2

            new Regex(@"([?!\.]) +(['""([\u00bf\u00A1\p{Pi}]+[\ ]*[\p{Lu}])"), // 3

            new Regex(@"([\p{L}\p{Nl}\p{Nd}\.\-]*)([\'\""\)\]\%\p{Pf}]*)(\.+)$"), // 4

            new Regex(@"(?:\.)[\p{Lu}\-]+(?:\.+)$"), // 5

            new Regex(@"^(?:[ ]*['""([\u00bf\u00A1\p{Pi}]*[ ]*[\p{Lu}0-9])", RegexOptions.Multiline), // 6

            new Regex(" +"), // 7

            new Regex(@"(\w+['""\)\]\%\p{Pf}]*[\u00bf\u00A1?!]+)(\p{Lu}[\w]*[^\.])"), // 8

            new Regex(@"((?:\w*\.\w+)+(?:\.(?![\n]))?)"), // 9

            new Regex(@"(?<!^)(\. *|: +| +)([-*•]) *(\p{L}\w+|\d\p{L}\w*)", RegexOptions.Multiline), // 10

            new Regex(@"(\S\.+)(\p{Lu})"), // 11

            new Regex(@"\b((?:18|19|20)\d{2})\.([0-1]?[0-9])\.([0-1]?[0-9])\b"), // 12

            new Regex(@"\b(?:(?:\d*\.)?(?:\w+\.)+(?:\w+(?:\.\d*)?))"), // 13

            new Regex(@"(?:www\..+?\.\p{L}{2,}|(?:[\w-]*[\w]\.)+(?:com|org|net|se|nu|da|no|fi))"), // 14 

            new Regex(@"\b((?i)(?<!www\.)\w+\.(?-i)N(?i)ET|\w+\.JS)\b"), // 15,

            new Regex(@"\b(?:A|C|J|R|J|X|XBase|Z)\+{1,2}"), // 16,
            
            new Regex(@"\b(?:A|C|F|J|M|Q)#"), // 17,
            
            new Regex(@"([^ ][\!?])(\p{Lu})"), // 18

            new Regex(@"([^?!\.: ])(?: {0,1})(?:\r\n|\n|\r)(?: *)(\p{Ll})"), // 19

            new Regex(@"^ *(\p{Lu}[\p{Lu} ]+\p{Lu})(?: *)(\p{Lu}\p{Ll}|\d\w)", RegexOptions.Multiline), // 20

            new Regex(@"\[{2,}"), // 21

            new Regex(@"(?: {3,})(\p{Lu} \w|\p{Lu}\w+)"), // 22
            
            new Regex(@"(\w\p{P})\[( |$)"), // 23
            
            new Regex(@"(?:^ *)(\p{Lu}[\w ,\-/\\]+:)(?: *)((?:\. *)?[\p{Lu}\d])", RegexOptions.Multiline), // 24
            
            new Regex(@"(\p{Lu}[\w ,\-/\\]+:)(?: *)((?:\. *)?[\p{Lu}\d])"), // 25
            
            new Regex(@"^ *[^\p{L}\s] *(?:\r\n|\n|\r)", RegexOptions.Multiline), // 26
            
            new Regex(@"^(?:\s*\. +)(\w+)", RegexOptions.Multiline), // 27
            
            new Regex(@"[\u00a0 ]{2,}"), // 28
            
            new Regex(@"(?:[^\w]\u00a0|\u00a0[^\w])"), // 29
            
            new Regex(@"(?:\n\s*){3,}"), // 30

            new Regex(@"(?<!^)(\. *|: +| +)([+]) *(\p{Lu}\w+|\d\p{L}\w*)", RegexOptions.Multiline), // 31

            new Regex(@" +(\d{1,2}[\.])(?: *(?:\r\n|\n|\r)+ *)(\w)", RegexOptions.Multiline), // 32
        };

        private static readonly string[] exceptions =
        {
            @"d", @"v", @"ex", @"t", @"A", @"B", @"C", @"D", @"E", @"F", @"G", @"H", @"I", @"J", @"K", @"L", @"M", @"N", @"O", @"P", @"Q", @"R", @"S", @"T", @"U", @"V", @"W", @"X", @"Y", @"Z", @"Ö", @"Ä", @"Å", @"Ø", @"Æ", @"bla", @"fKr",
            @"tex", @"sk", @"etc", @"eKr", @"mm", @"dvs", @"mfl", @"dä", @"dy", @"resp", @"tom", @"kl", @"osv", @"ff", @"eg", @"from", @"Bla", @"gr", @"Tex", @"pga", @"eo", @"ev", @"omkr", @"dr", @"fn", @"Ev", @"From", @"ns", @"alt",
            @"fkr", @"Div", @"e", @"Kl", @"odyl", @"od", @"JM", @"ang", @"ä", @"enl", @"iom", @"Dvs", @"jur", @"tv", @"Sk", @"kk", @"fra", @"MM", @"ca", @"Ex", @"AA", @"w", @"leg", @"k", @"tf", @"Etc", @"ed", @"utg", @"FN", @"ekr", @"kuk",
            @"sp", @"edyl", @"SM", @"em", @"th", @"fl", @"gm", @"Tom", @"mag", @"am", @"sas", @"fö", @"teol", @"mao", @"as", @"sek", @"EM", @"dd", @"m", @"upa", @"Fd", @"FM", @"tr", @"rf", @"SEK", @"dys", @"aka", @"pers", @"blaa", @"Fn",
            @"rok", @"trol", @"farm", @"OD", @"dyl", @"fvt", @"Pga", @"DM", @"tekn", @"SK", @"oa", @"fk", @"ref", @"nhov", @"sa", @"filkand", @"urspr", @"OA", @"frv", @"aa", @"sign", @"AKA", @"stud", @"pol", @"gs", @"vs", @"fom", @"pm",
            @"vd", @"spec", @"med", @"spa", @"mha", @"RAMeissn", @"EKr", @"kv", @"stf", @"Iom", @"krigsv", @"ss", @"SA", @"Pol", @"ua", @"km", @"NWA", @"ba", @"jr", @"fm", @"dv", @"doo", @"den", @"ok", @"ED", @"dsv", @"co", @"starkt",
            @"efterKr", @"dikter", @"phil", @"stundar/", @"polmaster", @"ekon", @"csp", @"civing", @"pgra", @"forts", @"mbpa", @"uvs", @"dreadlocks", @"Aa", @"gatunamn", @"Enl", @"signaturmelodin", @"markeratsa", @"septembergs", @"man",
            @"Farm", @"TOM", @"tsm", @"uta", @"LEGION", @"sdd", @"ie", @"REM", @"syfte", @"pizz", @"jurkand", @"ledarposition", @"fiolmm", @"dag", @"septemberns", @"civek", @"eldyl", @"TDMacfarl", @"sockena", @"polkand", @"sgs", @"philos",
            @"nv", @"glomerulonefrit,sk", @"day", @"igen", @"Guern", @"í-tron", @"libs", @"srl", @"sekr", @"EX", @"om", @"hellom", @"kronan", @"Ekon", @"JParn", @"modemm", @"ffKr", @"ek", @"sjukhus", @"os", @"skk", @"HJ", @"rs", @"pastex",
            @"nshm", @"Polen", @"Tim", @"zinkkarbonata", @"stv", @"berättar/", @"a", @"sm", @"rp", @"solanin", @"ry", @"ism", @"d,vs", @"sv", @"teolkand", @"sta", @"spp", @"nb", @"tillsm", @"DOA", @"intervjuasmm", @"osa", @"rc",
            @"dancehall", @"fv", @"hia", @"alvv", @"betr", @"sn", @"tillhöra"
        };

        private static readonly HashSet<string> hashSet;

        static SentenceExtensions()
        {
            hashSet = new HashSet<string>(exceptions);
        }

        public static IEnumerable<string> ToLines(this string text, bool code = false, bool doubleLineBreak = true, bool pointDoubleLineBreak = true)
        {
            string lineBreak = doubleLineBreak ? "\n\n" : "\n";
            
            string pointLineBreak = pointDoubleLineBreak ? "\n\n" : "\n";

            text = text.Replace('\t', ' ').Replace('\v', ' ');
            
            text = regexList[28].Replace(text, match => Enumerable.Range(0, match.Length).Select(_ => " ").Aggregate((s, s1) => $"{s}{s1}"));
            
            text = regexList[29].Replace(text, match => match.Value.Replace('\u00A0', ' '));
            
            text = regexList[27].Replace(text, "• $1");
            
            text = regexList[21].Replace(text, "").Replace('[', ' ');
            
            text = regexList[23].Replace(text, "$1$2").Replace('[', ' ');
            
            text = regexList[10].Replace(text, $"$1{pointLineBreak}$2 $3");

            text = regexList[31].Replace(text, $"$1{pointLineBreak}$2 $3");

            text = regexList[12].Replace(text, "$1-$2-$3");

            text = regexList[0].Replace(text, $"$1{lineBreak}$2");

            text = regexList[1].Replace(text, $"$1{lineBreak}$2");

            text = regexList[2].Replace(text, $"$1{lineBreak}$2");

            text = regexList[3].Replace(text, $"$1{lineBreak}$2");

            text = regexList[8].Replace(text, $"$1{lineBreak}$2");

            string[] words = regexList[7].Split(text);

            text = "";

            int i;

            for (i = 0; i < words.Length - 1; i++)
            {
                Match match = regexList[4].Match(words[i]);

                if (match.Success)
                {
                    string prefix = match.Groups[0].Success ? match.Groups[0].Value : null;

                    string startingPunctuation = match.Groups[1].Success ? match.Groups[1].Value : null;

                    if (prefix != null && hashSet.Contains(prefix) && startingPunctuation == null)
                    {
                    }
                    else if (regexList[5].IsMatch(words[i]))
                    {
                    }
                    else if (regexList[6].IsMatch(words[i + 1]))
                    {
                        words[i] += lineBreak;
                    }
                }
                else if (regexList[13].IsMatch(words[i]))
                {
                    string word = words[i].Replace(".", "").Trim('0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', ' ', '\t', '\r', '\n');

                    if (!hashSet.Contains(word))
                    {
                        if (regexList[11].IsMatch(words[i]) && !regexList[14].IsMatch(words[i]) && !regexList[15].IsMatch(words[i]))
                        {
                            words[i] = regexList[11].Replace(words[i], $"$1{lineBreak}$2");
                        }
                    }
                }
                else if (regexList[18].IsMatch(words[i]))
                {
                    words[i] = regexList[18].Replace(words[i], $"$1{lineBreak}$2");
                }

                words[i] = code && regexList[9].IsMatch(words[i]) ? regexList[9].Replace(words[i], m => m.Value.Hex()) : words[i];

                words[i] = code && regexList[16].IsMatch(words[i]) ? regexList[16].Replace(words[i], m => m.Value.Hex()) : words[i];

                words[i] = code && regexList[17].IsMatch(words[i]) ? regexList[17].Replace(words[i], m => m.Value.Hex()) : words[i];

                text += $"{words[i]} ";
            }

            words[i] = code && regexList[16].IsMatch(words[i]) ? regexList[16].Replace(words[i], m => m.Value.Hex()) : words[i];

            words[i] = code && regexList[17].IsMatch(words[i]) ? regexList[17].Replace(words[i], m => m.Value.Hex()) : words[i];

            text += $"{words[i]}";

            text = regexList[7].Replace(text, " ");

            text = regexList[19].Replace(text, "$1 $2");

            text = regexList[32].Replace(text, $"{pointLineBreak}$1 $2");
            
            text = regexList[20].Replace(text, $"$1{lineBreak}$2");
            
            text = regexList[22].Replace(text, $"{lineBreak}$1{lineBreak}$2");
            
            text = regexList[24].Replace(text, $"{lineBreak}$1{lineBreak}$2");
            
            text = regexList[25].Replace(text, $"{lineBreak}$1{lineBreak}$2");
            
            text = regexList[26].Replace(text, "");

            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            text = text.Trim('\n');
            
            text = regexList[30].Replace(text, "\n\n");
            
            foreach (string line in text.Split('\n'))
            {
                yield return line.Trim(' ');
            }
        }

        private static readonly Regex unHexRegex = new Regex(@"\bhexstring[A-F0-9]+x");

        private static IEnumerable<string> UnHex(this IEnumerable<string> lines)
        {
            foreach (string line in lines)
            {
                string l = line;

                if (unHexRegex.IsMatch(l))
                {
                    l = unHexRegex.Replace(l, match =>
                    {
                        string hex = match.Value.Substring(9, match.Length - 10);

                        return Encoding.UTF8.GetString(Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray());
                    });
                }

                yield return l;
            }
        }

        private static string UnHex(this string text)
        {
            return UnHex(new[] { text }).First();
        }

        private static string Hex(this string text)
        {
            return $"hexstring{string.Join("", Encoding.UTF8.GetBytes(text).Select(b => b.ToString("X2")))}x";
        }

        public static List<List<Token>> TokenizeSentences(this string text)
        {
            List<List<Token>> output = new List<List<Token>>();

            using (StringReader reader = new StringReader(string.Join("\n", text.ToLines(true))))
            {
                SwedishTokenizer tokenizer = new SwedishTokenizer(reader);

                List<Token> tokens;

                while ((tokens = tokenizer.ReadSentence()) != null)
                {
                    output.Add(tokens.Select(token => new Token(token.Type, token.Value.UnHex(), token.Offset) { IsSpace = token.IsSpace, IsCapitalized = token.IsCapitalized }).ToList());
                }

                return output;
            }
        }
    }
}
