using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RussianHelper
{
    public class RussianLanguageProcessor
    {
        private readonly Dictionary<char, string> _pronunciationRules;
        private readonly Dictionary<string, string> _commonWords;

        public RussianLanguageProcessor()
        {
            _pronunciationRules = InitializePronunciationRules();
            _commonWords = InitializeCommonWords();
        }

        public bool IsRussianWord(string word)
        {
            if (string.IsNullOrEmpty(word)) return false;
            
            // Check if the word contains Cyrillic characters
            return word.Any(c => c >= 0x0400 && c <= 0x04FF);
        }

        public string GetPronunciation(string word)
        {
            if (string.IsNullOrEmpty(word)) return string.Empty;
            
            // Check if we have a cached pronunciation for common words
            if (_commonWords.TryGetValue(word.ToLower(), out var cached))
            {
                return cached;
            }
            
            // Generate pronunciation using rules
            return GeneratePronunciationByRules(word);
        }

        public Task<string> GenerateFullPronunciationAsync(string text)
        {
            if (string.IsNullOrEmpty(text)) return Task.FromResult(string.Empty);
            
            var words = text.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();
            
            foreach (var word in words)
            {
                var cleanWord = word.Trim(new[] { '.', ',', '!', '?', ':', ';', '"', '\'', '(', ')', '[', ']', '{', '}' });
                
                if (IsRussianWord(cleanWord))
                {
                    var pronunciation = GetPronunciation(cleanWord);
                    result.AppendLine($"{cleanWord} → [{pronunciation}]");
                }
                else
                {
                    result.AppendLine($"{cleanWord} → (not Russian)");
                }
            }
            
            return Task.FromResult(result.ToString());
        }

        private string GeneratePronunciationByRules(string word)
        {
            var pronunciation = new StringBuilder();
            var chars = word.ToCharArray();
            
            for (int i = 0; i < chars.Length; i++)
            {
                var currentChar = chars[i];
                var nextChar = i < chars.Length - 1 ? chars[i + 1] : '\0';
                
                if (_pronunciationRules.TryGetValue(currentChar, out var rule))
                {
                    // Apply special rules for certain combinations
                    var specialRule = GetSpecialRule(currentChar, nextChar);
                    if (!string.IsNullOrEmpty(specialRule))
                    {
                        pronunciation.Append(specialRule);
                        if (specialRule.Length > 1) i++; // Skip next character
                    }
                    else
                    {
                        pronunciation.Append(rule);
                    }
                }
                else
                {
                    pronunciation.Append(currentChar);
                }
            }
            
            return pronunciation.ToString();
        }

        private string GetSpecialRule(char current, char next)
        {
            // Special pronunciation rules for Russian letter combinations
            var combination = $"{current}{next}".ToLower();
            
            return combination switch
            {
                "жч" => "shch",
                "жш" => "zhsh",
                "чш" => "chsh",
                "тс" => "ts",
                "дс" => "ds",
                "зж" => "zzh",
                "сж" => "szh",
                "сч" => "sch",
                _ => string.Empty
            };
        }

        private Dictionary<char, string> InitializePronunciationRules()
        {
            return new Dictionary<char, string>
            {
                // Vowels
                {'а', "a"}, {'А', "a"},
                {'е', "ye"}, {'Е', "ye"},
                {'ё', "yo"}, {'Ё', "yo"},
                {'и', "ee"}, {'И', "ee"},
                {'о', "o"}, {'О', "o"},
                {'у', "oo"}, {'У', "oo"},
                {'ы', "y"}, {'Ы', "y"},
                {'э', "e"}, {'Э', "e"},
                {'ю', "yu"}, {'Ю', "yu"},
                {'я', "ya"}, {'Я', "ya"},
                
                // Consonants
                {'б', "b"}, {'Б', "b"},
                {'в', "v"}, {'В', "v"},
                {'г', "g"}, {'Г', "g"},
                {'д', "d"}, {'Д', "d"},
                {'ж', "zh"}, {'Ж', "zh"},
                {'з', "z"}, {'З', "z"},
                {'й', "y"}, {'Й', "y"},
                {'к', "k"}, {'К', "k"},
                {'л', "l"}, {'Л', "l"},
                {'м', "m"}, {'М', "m"},
                {'н', "n"}, {'Н', "n"},
                {'п', "p"}, {'П', "p"},
                {'р', "r"}, {'Р', "r"},
                {'с', "s"}, {'С', "s"},
                {'т', "t"}, {'Т', "t"},
                {'ф', "f"}, {'Ф', "f"},
                {'х', "kh"}, {'Х', "kh"},
                {'ц', "ts"}, {'Ц', "ts"},
                {'ч', "ch"}, {'Ч', "ch"},
                {'ш', "sh"}, {'Ш', "sh"},
                {'щ', "shch"}, {'Щ', "shch"},
                {'ъ', "'"}, {'Ъ', "'"},
                {'ь', "'"}, {'Ь', "'"}
            };
        }

        private Dictionary<string, string> InitializeCommonWords()
        {
            return new Dictionary<string, string>
            {
                // Common greetings
                {"привет", "privet"},
                {"здравствуйте", "zdravstvuyte"},
                {"доброе", "dobroye"},
                {"утро", "utro"},
                {"день", "dyen"},
                {"вечер", "vecher"},
                {"добрый", "dobryy"},
                {"пока", "poka"},
                {"до", "do"},
                {"свидания", "svidaniya"},
                
                // Common words
                {"как", "kak"},
                {"дела", "dela"},
                {"хорошо", "khorosho"},
                {"плохо", "plokho"},
                {"спасибо", "spasibo"},
                {"пожалуйста", "pozhalusta"},
                {"извините", "izvinite"},
                {"да", "da"},
                {"нет", "nyet"},
                {"что", "chto"},
                {"где", "gde"},
                {"когда", "kogda"},
                {"почему", "pochemu"},
                {"кто", "kto"},
                {"я", "ya"},
                {"ты", "ty"},
                {"он", "on"},
                {"она", "ona"},
                {"оно", "ono"},
                {"мы", "my"},
                {"вы", "vy"},
                {"они", "oni"},
                {"это", "eto"},
                {"то", "to"},
                {"все", "vse"},
                {"всего", "vsego"},
                {"очень", "ochen"},
                {"много", "mnogo"},
                {"мало", "malo"},
                {"большой", "bolshoy"},
                {"маленький", "malenkiy"},
                {"новый", "novyy"},
                {"старый", "staryy"},
                {"хороший", "khoroshiy"},
                {"плохой", "plokhoy"},
                {"красивый", "krasivyy"},
                {"красивая", "krasivaya"},
                {"красивое", "krasivoye"},
                {"красивые", "krasivye"},
                {"изучаю", "izuchayu"},
                {"русский", "russkiy"},
                {"язык", "yazyk"},
                {"слово", "slovo"},
                {"предложение", "predlozheniye"},
                {"текст", "tekst"},
                {"книга", "kniga"},
                {"дом", "dom"},
                {"работа", "rabota"},
                {"семья", "semya"},
                {"друг", "drug"},
                {"подруга", "podruga"},
                {"любовь", "lyubov"},
                {"жизнь", "zhizn"},
                {"время", "vremya"},
                {"место", "mesto"},
                {"город", "gorod"},
                {"страна", "strana"},
                {"мир", "mir"},
                {"человек", "chelovek"},
                {"люди", "lyudi"},
                {"ребенок", "rebenok"},
                {"дети", "deti"},
                {"мужчина", "muzhchina"},
                {"женщина", "zhenschina"},
                {"мама", "mama"},
                {"папа", "papa"},
                {"брат", "brat"},
                {"сестра", "sestra"},
                {"бабушка", "babushka"},
                {"дедушка", "dedushka"},
                {"еда", "eda"},
                {"вода", "voda"},
                {"хлеб", "khleb"},
                {"молоко", "moloko"},
                {"чай", "chay"},
                {"кофе", "kofe"},
                {"мясо", "myaso"},
                {"рыба", "ryba"},
                {"овощи", "ovoshchi"},
                {"фрукты", "frukty"},
                {"цвет", "tsvet"},
                {"красный", "krasnyy"},
                {"синий", "siniy"},
                {"зеленый", "zelenyy"},
                {"желтый", "zheltyy"},
                {"белый", "belyy"},
                {"черный", "chernyy"},
                {"число", "chislo"},
                {"один", "odin"},
                {"два", "dva"},
                {"три", "tri"},
                {"четыре", "chetyre"},
                {"пять", "pyat"},
                {"шесть", "shest"},
                {"семь", "sem"},
                {"восемь", "vosem"},
                {"девять", "devyat"},
                {"десять", "desyat"}
            };
        }
    }
}
