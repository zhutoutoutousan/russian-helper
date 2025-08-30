using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RussianHelper
{
    public class TranslationResult
    {
        public string Meaning { get; set; } = "";
        public string Examples { get; set; } = "";
        public string Grammar { get; set; } = "";
        public string Level { get; set; } = "";
        public bool IsLoading { get; set; } = false;
        public string Error { get; set; } = "";
    }

    public class TranslationService
    {
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, TranslationResult> _cache;
        private const string DEEPSEEK_API_KEY = "sk-a5018fece97d405b94cb92591f71493e";
        private const string DEEPSEEK_API_URL = "https://api.deepseek.com/chat/completions";

        public TranslationService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {DEEPSEEK_API_KEY}");
            _cache = InitializeTranslationCache();
        }

        public async Task<TranslationResult> GetTranslationAsync(string word)
        {
            // Check cache first
            if (_cache.TryGetValue(word.ToLower(), out var cachedResult))
            {
                return cachedResult;
            }

            // Create loading result
            var loadingResult = new TranslationResult
            {
                Meaning = "Loading...",
                Examples = "Please wait...",
                Grammar = "Loading...",
                Level = "Loading...",
                IsLoading = true
            };

            try
            {
                // Call DeepSeek API
                var result = await CallDeepSeekAPIAsync(word);
                result.IsLoading = false;
                
                // Cache the result
                _cache[word.ToLower()] = result;
                
                return result;
            }
            catch (Exception ex)
            {
                var errorResult = new TranslationResult
                {
                    Meaning = "Translation Error",
                    Examples = "Unable to get translation",
                    Grammar = "Error",
                    Level = "Error",
                    IsLoading = false,
                    Error = ex.Message
                };
                
                // Cache error result to avoid repeated API calls
                _cache[word.ToLower()] = errorResult;
                
                return errorResult;
            }
        }

        private async Task<TranslationResult> CallDeepSeekAPIAsync(string word)
        {
            var prompt = $@"You are a Russian language expert. Please provide a detailed translation and analysis for the Russian word: '{word}'

Please respond in the following JSON format:
{{
    ""meaning"": ""English translation and explanation"",
    ""examples"": ""2-3 example sentences using this word"",
    ""grammar"": ""Grammatical information (part of speech, gender, etc.)"",
    ""level"": ""CEFR level (A1, A2, B1, B2, C1, C2)""
}}

Keep responses concise but informative.";

            var requestBody = new
            {
                model = "deepseek-chat",
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful Russian language assistant. Always respond with valid JSON." },
                    new { role = "user", content = prompt }
                },
                stream = false,
                temperature = 0.3
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(DEEPSEEK_API_URL, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"API Error: {response.StatusCode} - {responseContent}");
            }

            // Parse the response
            var apiResponse = JsonSerializer.Deserialize<DeepSeekResponse>(responseContent);
            
            if (apiResponse?.Choices?.Length > 0)
            {
                var messageContent = apiResponse.Choices[0].Message.Content;
                
                try
                {
                    // Try to parse the JSON response from the AI
                    var translationData = JsonSerializer.Deserialize<TranslationData>(messageContent);
                    
                    return new TranslationResult
                    {
                        Meaning = translationData?.Meaning ?? "Translation not available",
                        Examples = translationData?.Examples ?? "Examples not available",
                        Grammar = translationData?.Grammar ?? "Grammar info not available",
                        Level = translationData?.Level ?? "Level not available",
                        IsLoading = false
                    };
                }
                catch
                {
                    // If JSON parsing fails, return the raw response
                    return new TranslationResult
                    {
                        Meaning = messageContent,
                        Examples = "See translation above",
                        Grammar = "Information available in translation",
                        Level = "See translation above",
                        IsLoading = false
                    };
                }
            }

            throw new Exception("No response from API");
        }

        private Dictionary<string, TranslationResult> InitializeTranslationCache()
        {
            return new Dictionary<string, TranslationResult>
            {
                {"привет", new TranslationResult { Meaning = "Hello / Hi", Examples = "Привет, как дела? - Hello, how are you?", Grammar = "Interjection", Level = "A1" }},
                {"здравствуйте", new TranslationResult { Meaning = "Hello (formal)", Examples = "Здравствуйте, рад вас видеть. - Hello, nice to see you.", Grammar = "Interjection", Level = "A1" }},
                {"спасибо", new TranslationResult { Meaning = "Thank you", Examples = "Спасибо за помощь. - Thank you for help.", Grammar = "Interjection", Level = "A1" }},
                {"пожалуйста", new TranslationResult { Meaning = "Please / You're welcome", Examples = "Пожалуйста, помогите мне. - Please help me.", Grammar = "Interjection", Level = "A1" }},
                {"извините", new TranslationResult { Meaning = "Excuse me / Sorry", Examples = "Извините, где банк? - Excuse me, where is the bank?", Grammar = "Interjection", Level = "A1" }},
                {"да", new TranslationResult { Meaning = "Yes", Examples = "Да, я понимаю. - Yes, I understand.", Grammar = "Particle", Level = "A1" }},
                {"нет", new TranslationResult { Meaning = "No", Examples = "Нет, я не знаю. - No, I don't know.", Grammar = "Particle", Level = "A1" }},
                {"хорошо", new TranslationResult { Meaning = "Good / Well / OK", Examples = "Хорошо, я согласен. - OK, I agree.", Grammar = "Adverb", Level = "A1" }},
                {"плохо", new TranslationResult { Meaning = "Bad / Poorly", Examples = "Мне плохо. - I feel bad.", Grammar = "Adverb", Level = "A1" }},
                {"как", new TranslationResult { Meaning = "How / As / Like", Examples = "Как дела? - How are you?", Grammar = "Adverb/Conjunction", Level = "A1" }},
                {"дела", new TranslationResult { Meaning = "Affairs / Matters / Things", Examples = "Как дела? - How are things?", Grammar = "Noun (plural)", Level = "A1" }},
                {"русский", new TranslationResult { Meaning = "Russian", Examples = "Я изучаю русский язык. - I study Russian language.", Grammar = "Adjective", Level = "A1" }},
                {"язык", new TranslationResult { Meaning = "Language / Tongue", Examples = "Русский язык - Russian language", Grammar = "Noun (masculine)", Level = "A1" }},
                {"я", new TranslationResult { Meaning = "I", Examples = "Я студент. - I am a student.", Grammar = "Personal Pronoun", Level = "A1" }},
                {"изучаю", new TranslationResult { Meaning = "I study / I am studying", Examples = "Я изучаю русский. - I study Russian.", Grammar = "Verb (1st person)", Level = "A1" }}
            };
        }
    }

    // API Response Models
    public class DeepSeekResponse
    {
        public Choice[] Choices { get; set; } = Array.Empty<Choice>();
    }

    public class Choice
    {
        public Message Message { get; set; } = new();
    }

    public class Message
    {
        public string Content { get; set; } = "";
    }

    public class TranslationData
    {
        public string Meaning { get; set; } = "";
        public string Examples { get; set; } = "";
        public string Grammar { get; set; } = "";
        public string Level { get; set; } = "";
    }
}
