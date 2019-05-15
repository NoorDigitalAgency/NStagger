using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using java.io;
using Console = System.Console;

namespace NStagger.ModelMapper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            ObjectInputStream modelReader = new ObjectInputStream(new FileInputStream(@"C:\Users\Rojan\Desktop\swedish.bin\swedish.bin"));

            se.su.ling.stagger.SUCTagger stagger = (se.su.ling.stagger.SUCTagger)modelReader.readObject();

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            stopwatch = Stopwatch.StartNew();

            Mapper.Map<SUCTagger>(stagger);

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            stopwatch = Stopwatch.StartNew();

            SUCTagger sucTagger = Mapper.Map<SUCTagger>();

            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Stop();

            Console.WriteLine();

            var readLine =
                @"Stockholm växer som aldrig förr. Våra ambitioner är höga. Nu söker vi dig som vill vara med och forma morgondagens Stockholm.

Norrmalms stadsdelsområde har ca 70 000 invånare. I stadsdelsområdet ingår förutom Norrmalm och Vasastan även den nya stadsdelen Hagastaden där bostäder, parkområden, handel, life science med världsledande forskning och avancerad vård växer fram. Norrmalms stadsdelsförvaltning erbjuder dig en attraktiv arbetsplats där vi tillsammans arbetar för att ge stockholmarna de bästa verksamheterna.



Arbetsplatsbeskrivning
Karlbergs förskolor består av sex förskolor på olika adresser i Birkastan och Vasastan.

Sammanlagt är vi ca 80 engagerade medarbetare och ca 350 barn. Karlbergs förskolor är inspirerade av de kommunala förskolornas pedagogiska filosofi i Reggio Emilia. Vi söker nu 3 barnskötare till tre av våra förskolor. Är du en driven barnskötare fylld med glädje och entusiasm? Tycker du om att lära nytt, inspirera andra och värdesätter allas delaktighet, barn som vuxnas? Då är det här jobbet för dig! Hos oss får du arbeta i en tydlig organisation med barnens bästa i fokus, där vi har en framtagen verksamhetsidé som beskriver våra ställningstaganden och det förhållningssätt vi eftersträvar hos oss som arbetar med barnen. På Karlbergs förskolor har vi vår pedagogiska utvecklingstid på schemat, tydligt separerat från barngruppstid. Varje barngrupp genererar en viss pott planeringstid. Avdelningsansvarig för i samråd med biträdande chef dialog om vem som använder tiden i ditt arbetslag och hur mycket tid var och en använder.

Arbetsbeskrivning
Som barnskötare bidrar du till att vardagen på förskolan fungerar väl och att omsorgen och det dagliga pedagogiska arbetet bygger på barns delaktighet och val. Tillsammans med övriga kollegor på förskolan medverkar du till att en positiv, meningsfull och utvecklande miljö skapas för varje barn och för hela barngruppen. Du deltar i det pedagogiska utvecklingsarbetet och den pedagogiska dokumentationen enligt förskolans läroplan och verksamhetsmål. Du är engagerad och deltar aktivt i planering och utvärdering av arbetet. Du bidrar även vid föräldramöten och utvecklingssamtal. I de dagliga kontakterna med vårdnadshavarna är du engagerad, barnfokuserad och serviceinriktad.

Kvalifikationer
Du ska ha en gymnasieutbildning inom omvårdnadsprogrammet, barn och ungdom eller annan utbildning som bedöms likvärdig av arbetsgivaren.

Det är meriterande om du har erfarenhet av att arbeta som barnskötare och vana att dokumentera.

Som person är du engagerad, flexibel och ansvarstagande. Ditt barnfokus är tydligt och du har en positiv attityd till ditt arbete och ditt arbetslag såväl som till hela förskolan och enheten. Du samverkar med barn, kollegor och föräldrar på ett lyhört och smidigt sätt och lyssnar, kommunicerar samt hanterar konflikter på ett konstruktivt sätt. Du handlar i enlighet med fattade beslut, läroplanen, mål, policys och riktlinjer. Du är lugn, stabil och har förmågan att fokusera på rätt saker.

Övrigt
Du ska kunna arbeta förskolans alla öppettider, arbeta 100% samt kunna närvara på 1-2 kvällsmöten per månad. Stockholms stad arbetar med kompetensbaserad rekrytering som syftar till att se till varje persons kompetens och därmed motverka diskriminering. Intervjuer kommer ske löpande och tjänsten kan komma att tillsättas innan avsatt slutdatum. Stockholms stad arbetar med kompetensbaserad rekrytering som syftar till att se till varje persons kompetens och därmed motverka diskriminering. Inför rekryteringsarbetet har vi tagit ställning till rekryteringskanaler och marknadsföring. Vi undanber oss därför kontakt med mediesäljare, rekryteringssajter och liknande.

Stockholms stad arbetar med kompetensbaserad rekrytering som syftar till att se till varje persons kompetens och därmed motverka diskriminering.

Inför rekryteringsarbetet har vi tagit ställning till rekryteringskanaler och marknadsföring. Vi undanber oss därför kontakt med mediesäljare, rekryteringssajter och liknande.
Kontaktperson




Madeleine Monder




0850809355






Helena Gyllensporre




0850809332";

            SwedishTokenizer tokenizer = new SwedishTokenizer(new System.IO.StringReader(readLine));

            List<Token> sentence;

            int sentenceIndex = 0;

            while ((sentence = tokenizer.ReadSentence()) != null)
            {
                TaggedToken[] sent = new TaggedToken[sentence.Count];

                for (int j = 0; j < sentence.Count; j++)
                {
                    Token tok = sentence[j];

                    var id = $"{sentenceIndex}:{tok.Offset}";

                    sent[j] = new TaggedToken(tok, id);
                }

                TaggedToken[] taggedSent = sucTagger.TagSentence(sent, true, false);

                Console.WriteLine(string.Join(" ", taggedSent.Select(token => $"{token.Token.Value}/{token.Token.Type}")));

                sentenceIndex++;
            }

            Console.ReadLine();
        }
    }
}
