using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EnigmaContest.Code;

namespace EnigmaContest.Models
{
    public class StringEncrypt
    {
        internal string _name;
        internal List<string> _hints;
        internal string _param;
        internal bool _iscancelling = false;
        internal Func<string, string, string> _transform;


        internal LinqEnigmaContentDataContext db = new LinqEnigmaContentDataContext();
        internal Random r = new Random();

        public const int MAX_LEVELS = 10;

        public string Name => _name;
        public string Hint => _hints.Choice();
        public string Parameters => _param;
        public bool IsCancelling => _iscancelling;

        public string Apply(string str) => _transform(str, _param);

        public static List<string> Ciphers = new List<string>() { "dl", "p", "wr", "wi", "ls", "lb", "ps" };

        public static List<Tuple<int, string>> Messages = new List<Tuple<int, string>>()
    {
        new Tuple<int,string>(1,"Photons have neither morals nor visas."),
        new Tuple<int,string>(2,"Design a system an idiot can use and only an idiot will want to use it."),
        new Tuple<int,string>(3,"These are questions for action, not speculation, which is idle."),
        new Tuple<int,string>(4,"Inside every small problem is a big one trying to get government funding."),
        new Tuple<int,string>(5,"The windows registry is dead ground which microsoft keeps trying to plant in."),
        new Tuple<int,string>(6,"The future has arrived; it's just not evenly distributed."),
        new Tuple<int,string>(7,"We can't really predict the future at all. All we can do is invent it."),
        new Tuple<int,string>(8,"Can money pay for all the days I lived half a sleep?"),
        new Tuple<int,string>(9,"It's dangerous to be right when the government is wrong."),
        new Tuple<int,string>(10,"Indifference will certainly be the downfall of mankind, but who cares?"),
    };


        public Tuple<int, string> GetMessage()
        {
            var max = db.Messages.Count();
            var m = db.Messages.Skip(r.Next(0, max - 1)).Take(1).First();
            return new Tuple<int,string>(m.Id, m.Text);
        }

        public CipherMessage GetNewMessage(int complexity)
        {
            // 0. prepare everything
            var r = new Random();
            var m = new CipherMessage();
            var msg = GetMessage();
            m.MessageId = msg.Item1;
            m.OriginalMessage = msg.Item2;
            m.EncodedMessage = m.OriginalMessage;

            // 1. compute the level (# of encryptions in the pipeline)
            complexity++;
            m.Level = Math.Min(r.Next(1, complexity / (Ciphers.Count * 2) + 1), MAX_LEVELS);
            m.Complexity = 1;
            var level = 0;
            // 2. with every level
            while (level++ < m.Level)
            {
                //    2.1 Compute the complexity for the level
                var lcomplex = r.Next(1, complexity);
                m.Complexity += lcomplex;

                //    2.2. Get the encoding
                string cipher; StringEncrypt encoding;
                retry:
                cipher = Ciphers.Choice();
                encoding = GetEncryption(cipher, lcomplex);
                if (encoding.IsCancelling && m.EncodingParams.Any(ep => ep.Item1 == cipher)) goto retry;
                //    2.3. Encode message
                m.EncodedMessage = encoding.Apply(m.EncodedMessage).Trim();
                //    2.4. Get params and validators
                m.Hints.Add(encoding.Hint);
                m.EncodingParams.Add(new Tuple<string, string>(cipher, encoding.Parameters));

            }
            // 3. return the object  
            return m;
        }

        public static string Check(string name, int complexity, string input)
        {
            return
                GetEncryption(name, complexity)
                .Apply(input);
        }

        public static string Try(string name, string param, string input)
        {
            return
                GetEncryption(name, param)
                .Apply(input);
        }

        public static StringEncrypt GetEncryption(string name, string param)
        {
            StringEncrypt test;
            switch (name)
            {
                case "dl":
                    test = new DoubleLetterEncrypt(param);
                    break;
                case "p":
                    test = new PalindromeEncrypt(param);
                    break;
                case "wr":
                    test = new WordsReverseEncrypt(param);
                    break;
                case "wi":
                    test = new WordsInverseEncrypt(param);
                    break;
                case "ls":
                    test = new LetterShiftEncrypt(param);
                    break;
                case "lb":
                    test = new LetterSubstituteEncrypt(param);
                    break;
                case "ps":
                    test = new PhraseShiftEncrypt(param);
                    break;

                default:
                    test = new NoneEncrypt(param);
                    break;
            }
            return test;
        }

        public static StringEncrypt GetEncryption(string name, int complexity)
        {
            StringEncrypt test;
            switch (name)
            {
                case "dl":
                    test = new DoubleLetterEncrypt(complexity);
                    break;
                case "p":
                    test = new PalindromeEncrypt(complexity);
                    break;
                case "wr":
                    test = new WordsReverseEncrypt(complexity);
                    break;
                case "wi":
                    test = new WordsInverseEncrypt(complexity);
                    break;
                case "ls":
                    test = new LetterShiftEncrypt(complexity);
                    break;
                case "lb":
                    test = new LetterSubstituteEncrypt(complexity);
                    break;
                case "ps":
                    test = new PhraseShiftEncrypt(complexity);
                    break;

                default:
                    test = new NoneEncrypt(complexity);
                    break;
            }

            return test;
        }

    }

    public class CipherMessage
    {
        public int MessageId { get; set; }
        public string OriginalMessage { get; set; }
        public string EncodedMessage { get; set; }
        public List<string> Hints { get; set; }
        public List<Tuple<string, string>> EncodingParams { get; set; }
        public int Complexity { get; set; }
        public int Level { get; set; }
        public int Score => Level + Complexity;



        public CipherMessage()
        {
            Hints = new List<string>();
            EncodingParams = new List<Tuple<string, string>>();
        }
    }

    public class DecodedMessage
    {
        public int MessageId { get; set; }
        public string OriginalMessage { get; set; }
        public int Complexity { get; set; }
        public int Score { get; set; }
        public bool UsedHints { get; set; }
    }

    public class DoubleLetterEncrypt : StringEncrypt
    {

        private void _init()
        {
            _name = "Letter doubling";
            _transform = (str, par) => string.Join("", str.ToCharArray().Select(a => new string(a, Convert.ToInt32(par))));
            _hints = new List<string>
            {
                "I see too many letters",
                "Why there are so many doubled letters?",
                $"The text seems to be rather long, {_param}x long as usual",
                this.Apply("What?")
            };
        }

        public DoubleLetterEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public DoubleLetterEncrypt(int complexity)
        {
            _param = (complexity / 20 + 2).ToString();
            _init();
        }
    }

    public class NoneEncrypt : StringEncrypt
    {

        private void _init()
        {
            _name = "None";
            _transform = (str, par) => str;
            _hints = new List<string>
            {
                "Nothing",
                "None",
                "Somebody just forgot to encrypt it",
            };
        }

        public NoneEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public NoneEncrypt(int complexity)
        {
            _param = complexity.ToString();
            _init();
        }

    }

    public class PalindromeEncrypt : StringEncrypt
    {

        private void _init()
        {
            _name = "Palindroming";
            _iscancelling = true;
            _transform = (str, par) => string.Join("", str.ToCharArray().Reverse());
            _hints = new List<string>
            {
                "Can you read backwards?",
                "These words sound very similarly, maybe we start from another side?",
                "Maybe we can use a mirror here?",
                this.Apply("What") + " are you trying to do?"
            };
        }

        public PalindromeEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public PalindromeEncrypt(int complexity)
        {
            _param = (complexity).ToString();
            _init();
        }
    }

    public class WordsReverseEncrypt : StringEncrypt
    {

        private void _init()
        {
            _name = "Word Reverse";
            _iscancelling = true;
            _transform = (str, par) => string.Join(" ", str.Split(new char[] { ' ' }).Reverse());
            _hints = new List<string>
            {
                "It sounds like Master Yodo's language",
                "Words are in the strange order!",
                "Other language might have a different words order, try to make it more English",
                this.Apply("What would that mean?")
            };
        }

        public WordsReverseEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public WordsReverseEncrypt(int complexity)
        {
            _param = (complexity).ToString();
            _init();
        }
    }

    public class WordsInverseEncrypt : StringEncrypt
    {
        private void _init()
        {
            _name = "Word Inverse";
            _iscancelling = true;
            _transform = (str, par) => string.Join(" ", str.Split(new char[] { ' ' }).Select(a => string.Join("", a.ToCharArray().Reverse())));
            _hints = new List<string>
            {
                "I feel dizzy",
                "Words are mirrored, no?",
                "We need to look at every word with a mirror!",
                this.Apply("What would that mean?")
            };
        }

        public WordsInverseEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public WordsInverseEncrypt(int complexity)
        {
            _param = (complexity).ToString();
            _init();
        }
    }

    public class LetterShiftEncrypt : StringEncrypt
    {
        private string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private void _init()
        {
            _name = "Letter Shift";

            _transform = (str, par) => string.Join("", str.ToCharArray().Select(a => alphabet.Contains(a) ? alphabet[((alphabet.Length / 2) + alphabet.IndexOf(a) % (alphabet.Length / 2) + Convert.ToInt32(_param)) % (alphabet.Length / 2)] : a));
            _hints = new List<string>
            {
                "The text looks shifted " + ((Convert.ToInt32(_param)>0)?"to the right":"to the left") + "(" + this.Apply("abc") + " = abc " + ")",
                this.Apply("abcdefg") + " = abcdefg " + " try to generalize this knowledge",
                "It seems like all letters are substituted with different ones, what kind of system this might be? " + "(" + this.Apply("abc") + " = abc " + ")"
            };
        }

        public LetterShiftEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public LetterShiftEncrypt(int complexity)
        {
            _param = (((int)Math.Pow((-1), complexity)) * complexity / 10 + ((int)Math.Pow((-1), complexity))).ToString();
            _init();
        }
    }

    public class PhraseShiftEncrypt : StringEncrypt
    {

        private void _init()
        {
            _name = "Phrase Shift";

            _transform = (str, par) =>
            {
                var _shift = Convert.ToInt32(_param) % str.Length;
                if (_shift < 0) _shift = (str.Length + _shift) % str.Length;
                var left = str.Substring(str.Length - _shift);
                return (left + str).Substring(0, str.Length);
            };

            _hints = new List<string>
            {
                "The text looks shifted " + ((Convert.ToInt32(_param)>0)?"to the right":"to the left"),
                "The phrase reminds me about a running line I see often in the news..."
            };
        }

        public PhraseShiftEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public PhraseShiftEncrypt(int complexity)
        {
            _param = (((int)Math.Pow((-1), complexity)) * complexity / 3 + ((int)Math.Pow((-1), complexity)) * 5).ToString();
            _init();
        }
    }

    public class LetterSubstituteEncrypt : StringEncrypt
    {
        private string alphabet = "olsaibmntryw" + "|" +
                                  "01$@!ßµптяУш";
        private string from;
        private string to;


        private void _init()
        {
            _name = "Letter Substutute";
            _iscancelling = true;
            var zones = _param.Split(new char[] { '|' });
            from = zones[0];
            to = zones[1];

            _transform = (str, par) => string.Join("", str.ToLower().ToCharArray().Select(a => from.Contains(a) ? to[from.IndexOf(a)] : a));
            _hints = new List<string>
            {
                "I think Ceasar was using this kind of cipher",
                this.Apply("They definitely substitute something with something, we need to investigate."),
                "It seems like all letters are substituted with different ones, what kind of system this might be?"
            };
        }

        public LetterSubstituteEncrypt(string param)
        {
            _param = param;
            _init();
        }

        public LetterSubstituteEncrypt(int complexity)
        {
            var len = Math.Min(complexity / 3 + 3, (alphabet.Length - 1) / 2);
            var zones = alphabet.Split(new char[] { '|' });
            _param = zones[0].Substring(0, len) + "|" + zones[1].Substring(0, len);
            _init();
        }
    }

    public class DecodeResult
    {
        public string UserMessage { get; set; }
        public double Fitness { get; set; }
        public int Percent => (int)(Fitness * 100);
        public bool IsCorrect { get; set; }
        public int Score { get; set; }
        public int TotalScore { get; set; }
    }

    public class Leader
    {
        public string Name { get; set; }
        public int TotalScore { get; set; }
        public int Age { get; set; }

        public DateTime LastAttempt { get; set; }
    }

}