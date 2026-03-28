using LiveKit.Proto;
using service.Models;
using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace service.Services
{
    public class TopicExtractorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _groqApiKey;
        private readonly string _groqUrl = "https://api.groq.com/openai/v1/chat/completions";
        private static readonly ConcurrentDictionary<string, string> _roomTopics = new();

        public TopicExtractorService(HttpClient httpClient, IConfiguration config)
        {
            _groqApiKey = config["GroqConfig:GROQ_API_KEY"]
              ?? config["GROQ_API_KEY"] 
              ?? throw new Exception("Missing Groq Key!");
            _httpClient = httpClient;
        }

        /// <summary>
        /// Sends a chunk of transcript to Groq and returns a 2-3 word topic.
        /// </summary>
        public async Task<TopicResult> GetTopicFromTranscript(string roomId, string transcriptChunk)
        {
            _roomTopics.TryGetValue(roomId, out var lastTopic);
            lastTopic ??= "";

            var requestBody = new
            {
                model = "llama-3.1-8b-instant",
                messages = new[]
                {
                    new {
                        role = "system",
                        content = "You are a topic extractor for a networking app. Your job is to describe the current conversation in 2-3 words. " +
                                  "If the new transcript is a shift from the previous topic, prefix your response with [BRANCH]. Be strict about this, a slight change can warrant BRANCH. Prefer branching over staying " +
                                  "If there is no previous topic, or the topic is similar, do NOT use [BRANCH]. " +
                                  "Return ONLY the topic name."
                    },
                    new {
                        role = "user",
                        content = $"PREVIOUS TOPIC: {lastTopic}\n\nNEW TRANSCRIPT CHUNK: {transcriptChunk}"
                    }
                },
                temperature = 0.3 //keep low for accuracy
            };

            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _groqApiKey);

                var response = await _httpClient.PostAsJsonAsync(_groqUrl, requestBody);
                response.EnsureSuccessStatusCode();

                //openAI style response: see docs: https://platform.openai.com/docs/api-reference/chat/create#chat/create-response-format
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();

                var rawContent = json.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString()?.Trim() ?? "";

                bool isBranch = rawContent.Contains("[BRANCH]", StringComparison.OrdinalIgnoreCase);
                string cleanTopic = rawContent.Replace("[BRANCH]", "", StringComparison.OrdinalIgnoreCase).Trim();

                if (!string.IsNullOrWhiteSpace(cleanTopic))
                {
                    _roomTopics[roomId] = cleanTopic;
                }

                return new TopicResult
                {
                    Topic = cleanTopic,
                    IsBranch = isBranch,
                    RawResponse = rawContent
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Groq Error: {ex.Message}");
                return new TopicResult
                {
                    Topic = "General Chat",
                    IsBranch = false
                };
            }
        }

        public void ClearRoom(string roomId) => _roomTopics.TryRemove(roomId, out _); //dead room clearer thing
    }
}



