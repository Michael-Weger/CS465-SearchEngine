using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CS465_SearchEngine.Source.Index.Utility
{
    /// <summary>
    /// A utility class for performing Porter Stemming https://tartarus.org/martin/PorterStemmer/def.txt
    /// </summary>
    public class PorterStemmer
    {
        private static readonly char[] vowels = { 'a', 'e', 'i', 'o', 'u' };
        
        /// <summary>
        /// Performs Porter Stemming running through each step of the algorithm on the provided input string.
        /// </summary>
        /// <param name="word">Input string to stem.</param>
        /// <returns>Stemmed version of the input string.</returns>
        public static string StemWord(string word)
        {
            word = Step1A(word);
            word = Step1B(word);
            word = Step1C(word);

            word = Step2(word);

            word = Step3(word);

            word = Step4(word);

            word = Step5A(word);
            return Step5B(word);
        }

        public static List<string> StemWords(List<string> words)
        {
            List<string> stemmedWords = new List<string>(words.Count);
            words.ForEach(word => stemmedWords.Add(PorterStemmer.StemWord(word)));
            return stemmedWords;
        }

        /// <summary>
        /// Whether or not the string contains a vowel. Can be used on stems to determine *v*
        /// *v* - the stem contains a vowel.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool ContainsVowel(string str)
        {
            bool followsVowel = false;
            foreach (char ch in str)
            {
                if (vowels.Contains(ch) || (ch == 'y' && !followsVowel))
                    return true;

                followsVowel = false;
            }

            return false;
        }

        private static string GetStem(string str, string suffix)
        {
            return str.Substring(0, str.LastIndexOf(suffix));
        }

        /// <summary>
        /// *d  - the stem ends with a double consonant (e.g. -TT, -SS).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool EndsInDoubleConsonant(string str)
        {
            return str.Length > 1 && !vowels.Contains(str[str.Length - 1]) && str[str.Length - 1] == str[str.Length - 2];
        }

        /// <summary>
        /// *o  - the stem ends cvc, where the second c is not W, X or Y (e.g. -WIL, -HOP).
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static bool EndsInCVC(string str)
        {
            int length = str.Length;

            return str.Length > 2 && !vowels.Contains(str[length - 3]) && // C
                (vowels.Contains(str[length - 2]) || str[length - 2] == 'y') // V, y is valid as it follows consonant
                && (!vowels.Contains(str[length - 1]) && str[length - 1] != 'w' && str[length - 1] != 'x' && str[length - 1] != 'y'); // C, not w, x, or y
        }

        private static int ComputeMeasure(string str)
        {
            int measure = 0;
            bool followsVowel = false;

            foreach (char ch in str)
            {
                if(vowels.Contains(ch) || (ch == 'y' && !followsVowel))
                {
                    followsVowel = true;
                }
                else if(followsVowel)
                {
                    measure++;
                    followsVowel = false;
                }
            }

            return measure;
        }

        /// <summary>
        /// Step 1a; Largely deals with plurals and past participles.
        /// 
        /// SSES -> SS caresses  ->  caress
        /// IES  -> I  ponies    ->  poni
        ///            ties      ->  ti
        /// SS   -> SS caress    ->  caress
        /// S    ->    cats      ->  cat
        /// </summary>
        /// <param name="input">string input to mutate.</param>
        /// <returns>String mutated by Step1A</returns>
        private static string Step1A(string str)
        {
            if(str.EndsWith("sses") || str.EndsWith("ies"))
            {
                str = GetStem(str, "es");
            }
            else if(str.EndsWith("s") && !str.EndsWith("ss"))
            {
                str = GetStem(str, "s");
            }

            return str;
        }

        /// <summary>
        /// Step 1b; Still largely deals with plurals and past participles.   
        /// 
        /// (m>0) EED -> EE     feed      ->  feed
        ///                     agreed    ->  agree
        /// (*v*) ED  ->        plastered ->  plaster
        ///                     bled      ->  bled
        /// (*v*) ING ->        motoring  ->  motor
        ///                     sing      ->  sing
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Step1B(string str)
        {
            int  measure = ComputeMeasure(str);
            bool success = false;

            if (measure > 0 && str.EndsWith("eed"))
            {
                str = GetStem(str, "d");
            }
            else if (str.EndsWith("ed") && ContainsVowel(GetStem(str, "ed")))
            {
                success = true;
                str = GetStem(str, "ed");
            }
            else if(str.EndsWith("ing") && ContainsVowel(GetStem(str, "ing")))
            {
                success = true;
                str = GetStem(str, "ing");
            }

            return success ? Step1BPart2(str) : str;
        }

        /// <summary>
        /// Step1B Part2; Called when the ed or ing removal is successful.
        /// 
        /// AT -> ATE                       conflat(ed)  ->  conflate
        /// BL -> BLE                       troubl(ed)   ->  trouble
        /// IZ -> IZE                       siz(ed)      ->  size
        /// (* d and not 
        /// (* L or *S or *Z)) -> single letter
        ///                                 hopp(ing)    ->  hop
        ///                                 tann(ed)     ->  tan
        ///                                 fall(ing)    ->  fall
        ///                                 hiss(ing)    ->  hiss
        ///                                 fizz(ed)     ->  fizz
        /// (m= 1 and* o) -> E              fail(ing)    ->  fail
        ///                                 fil(ing)     ->  file
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Step1BPart2(string str)
        {
            int measure = ComputeMeasure(str);

            if (str.EndsWith("at") || str.EndsWith("bl") || str.EndsWith("iz") || (measure == 1 && EndsInCVC(str)))
            {
                str += 'e';
            }
            else if (EndsInDoubleConsonant(str) && str[str.Length - 1] != 'l' && str[str.Length - 1] != 's' && str[str.Length - 1] != 'z')
            {
                str = GetStem(str, str[str.Length - 1].ToString());
            }

            return str;
        }

        private static string Step1C(string str)
        {
            if(ContainsVowel(str) && str.EndsWith('y'))
            {
                str = GetStem(str, "y");
            }

            return str;
        }

        /// <summary>
        /// (m>0) ATIONAL ->  ATE  relational      ->  relate
        /// (m>0) TIONAL  ->  TION conditional     ->  condition
        ///                         rational       ->  rational
        /// (m>0) ENCI    ->  ENCE  valenci        ->  valence
        /// (m>0) ANCI    ->  ANCE  hesitanci      ->  hesitance
        /// (m>0) IZER    ->  IZE   digitizer      ->  digitize
        /// (m>0) ABLI    ->  ABLE  conformabli    ->  conformable
        /// (m>0) ALLI    ->  AL    radicalli      ->  radical
        /// (m>0) ENTLI   ->  ENT   differentli    ->  different
        /// (m>0) ELI     ->  E     vileli         ->  vile
        /// (m>0) OUSLI   ->  OUS   analogousli    ->  analogous
        /// (m>0) IZATION ->  IZE   vietnamization ->  vietnamize
        /// (m>0) ATION   ->  ATE   predication    ->  predicate
        /// (m>0) ATOR    ->  ATE   operator       ->  operate
        /// (m>0) ALISM   ->  AL    feudalism      ->  feudal
        /// (m>0) IVENESS ->  IVE   decisiveness   ->  decisive
        /// (m>0) FULNESS ->  FUL   hopefulness    ->  hopeful
        /// (m>0) OUSNESS ->  OUS   callousness    ->  callous
        /// (m>0) ALITI   ->  AL    formaliti      ->  formal
        /// (m>0) IVITI   ->  IVE   sensitiviti    ->  sensitive
        /// (m>0) BILITI  ->  BLE   sensibiliti    ->  sensible
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Step2(string str)
        {
            int measure = ComputeMeasure(str);

            if(measure > 0)
            {
                if(str.EndsWith("ational"))
                {
                    str = GetStem(str, "ational");
                    str += "ate";
                }
                else if(str.EndsWith("tional"))
                {
                    str = GetStem(str, "tional");
                    str += "tion";
                }
                else if(str.EndsWith("enci"))
                {
                    str = GetStem(str, "enci");
                    str += "ence";
                }
                else if (str.EndsWith("anci"))
                {
                    str = GetStem(str, "anci");
                    str += "ance";
                }
                else if (str.EndsWith("izer"))
                {
                    str = GetStem(str, "izer");
                    str += "ize";
                }
                else if (str.EndsWith("abli"))
                {
                    str = GetStem(str, "abli");
                    str += "able";
                }
                else if (str.EndsWith("alli"))
                {
                    str = GetStem(str, "alli");
                    str += "al";
                }
                else if (str.EndsWith("entli"))
                {
                    str = GetStem(str, "entli");
                    str += "ent";
                }
                else if (str.EndsWith("eli"))
                {
                    str = GetStem(str, "eli");
                    str += "e";
                }
                else if (str.EndsWith("ousli"))
                {
                    str = GetStem(str, "ousli");
                    str += "ous";
                }
                else if (str.EndsWith("ization"))
                {
                    str = GetStem(str, "ization");
                    str += "ize";
                }
                else if (str.EndsWith("ation"))
                {
                    str = GetStem(str, "ation");
                    str += "ate";
                }
                else if (str.EndsWith("ator"))
                {
                    str = GetStem(str, "ator");
                    str += "ate";
                }
                else if (str.EndsWith("alism"))
                {
                    str = GetStem(str, "alism");
                    str += "al";
                }
                else if (str.EndsWith("iveness"))
                {
                    str = GetStem(str, "iveness");
                    str += "ive";
                }
                else if (str.EndsWith("fulness"))
                {
                    str = GetStem(str, "fulness");
                    str += "ful";
                }
                else if (str.EndsWith("ousness"))
                {
                    str = GetStem(str, "ousness");
                    str += "ous";
                }
                else if (str.EndsWith("aliti"))
                {
                    str = GetStem(str, "aliti");
                    str += "al";
                }
                else if (str.EndsWith("iviti"))
                {
                    str = GetStem(str, "iviti");
                    str += "ive";
                }
                else if (str.EndsWith("biliti"))
                {
                    str = GetStem(str, "biliti");
                    str += "ble";
                }
            }

            return str;
        }

        /// <summary>
        /// Step 3; Largely removes suffixes
        /// 
        /// (m>0) ICATE ->  IC  triplicate     ->  triplic
        /// (m>0) ATIVE ->      formative      ->  form
        /// (m>0) ALIZE ->  AL  formalize      ->  formal
        /// (m>0) ICITI ->  IC  electriciti    ->  electric
        /// (m>0) ICAL  ->  IC  electrical     ->  electric
        /// (m>0) FUL   ->      hopeful        ->  hope
        /// (m>0) NESS  ->      goodness       ->  good
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Step3(string str)
        {
            int measure = ComputeMeasure(str);

            if (measure > 0)
            {
                if(str.EndsWith("icate"))
                {
                    str = GetStem(str, "icate");
                    str += "ic";
                }
                else if(str.EndsWith("ative"))
                {
                    str = GetStem(str, "ative");
                }
                else if (str.EndsWith("alize"))
                {
                    str = GetStem(str, "alize");
                    str += "al";
                }
                else if (str.EndsWith("iciti"))
                {
                    str = GetStem(str, "iciti");
                    str += "ic";
                }
                else if (str.EndsWith("ical"))
                {
                    str = GetStem(str, "ical");
                    str += "ic";
                }
                else if (str.EndsWith("ful"))
                {
                    str = GetStem(str, "ful");
                }
                else if (str.EndsWith("ness"))
                {
                    str = GetStem(str, "ness");
                }
            }

            return str;
        }

        /// <summary>
        /// Step 4; Largely removes suffixes
        /// 
        /// (m>1) AL    ->                  revival        ->  reviv
        /// (m>1) ANCE  ->                  allowance      ->  allow
        /// (m>1) ENCE  ->                  inference      ->  infer
        /// (m>1) ER    ->                  airliner       ->  airlin
        ///  m>1) IC    ->                  gyroscopic     ->  gyroscop
        /// (m>1) ABLE  ->                  adjustable     ->  adjust
        /// (m>1) IBLE  ->                  defensible     ->  defens
        /// (m>1) ANT   ->                  irritant       ->  irrit
        /// (m>1) EMENT ->                  replacement    ->  replac
        /// (m>1) MENT  ->                  adjustment     ->  adjust
        /// (m>1) ENT   ->                  dependent      ->  depend
        /// (m>1 and (* S or *T)) ION ->    adoption       ->  adopt
        /// (m>1) OU    ->                  homologou      ->  homolog
        /// (m>1) ISM   ->                  communism      ->  commun
        /// (m>1) ATE   ->                  activate       ->  activ
        /// (m>1) ITI   ->                  angulariti     ->  angular
        /// (m>1) OUS   ->                  homologous     ->  homolog
        /// (m>1) IVE   ->                  effective      ->  effect
        /// (m>1) IZE   ->                  bowdlerize     ->  bowdler
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string Step4(string str)
        {
            int measure = ComputeMeasure(str);

            if (measure > 1)
            {
                if (str.EndsWith("al"))
                {
                    str = GetStem(str, "al");
                }
                else if (str.EndsWith("ance"))
                {
                    str = GetStem(str, "ance");
                }
                else if (str.EndsWith("ence"))
                {
                    str = GetStem(str, "ence");
                }
                else if (str.EndsWith("er"))
                {
                    str = GetStem(str, "er");
                }
                else if (str.EndsWith("ic"))
                {
                    str = GetStem(str, "ic");
                }
                else if (str.EndsWith("able"))
                {
                    str = GetStem(str, "able");
                }
                else if (str.EndsWith("ible"))
                {
                    str = GetStem(str, "ible");
                }
                else if (str.EndsWith("ant"))
                {
                    str = GetStem(str, "ant");
                }
                else if (str.EndsWith("ement"))
                {
                    str = GetStem(str, "ement");
                }
                else if (str.EndsWith("ment"))
                {
                    str = GetStem(str, "ment");
                }
                else if (str.EndsWith("ent"))
                {
                    str = GetStem(str, "ent");
                }
                else if (str.EndsWith("ion") && (str[str.Length - 4] == 's' || str[str.Length - 4] == 't'))
                {
                    str = GetStem(str, "ion");
                }
                else if (str.EndsWith("ou"))
                {
                    str = GetStem(str, "ou");
                }
                else if (str.EndsWith("ism"))
                {
                    str = GetStem(str, "ism");
                }
                else if (str.EndsWith("ate"))
                {
                    str = GetStem(str, "ate");
                }
                else if (str.EndsWith("iti"))
                {
                    str = GetStem(str, "iti");
                }
                else if (str.EndsWith("ous"))
                {
                    str = GetStem(str, "ous");
                }
                else if (str.EndsWith("ive"))
                {
                    str = GetStem(str, "ive");
                }
                else if (str.EndsWith("ize"))
                {
                    str = GetStem(str, "ize");
                }
            }

            return str;
        }

        /// <summary>
        /// Step 5a; Clean-up
        /// 
        /// (m>1) E ->                  probate ->  probat
        ///                             rate    ->  rate
        /// (m= 1 and not *o) E ->      cease   ->  ceas
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Step5A(string str)
        {
            int measure = ComputeMeasure(str);

            if((measure > 1 && str.EndsWith("e")) || (measure == 1 && !EndsInCVC(str) && str.EndsWith("e")))
            {
                str = GetStem(str, "e");
            }

            return str;
        }

        /// <summary>
        /// Step 5b; Final clean-up
        /// (m > 1 and *d and *L) -> single letter
        ///                            controll       ->  control
        ///                            roll           ->  roll
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Step5B(string str)
        {
            int measure = ComputeMeasure(str);

            if (measure > 1 && EndsInDoubleConsonant(str) &&  str.EndsWith("l"))
            {
                str = GetStem(str, str[str.Length - 1].ToString());
            }

            return str;
        }
    }
}
