using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OllamaConsole;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Ollama Türkçe Sohbet Uygulaması");
        Console.WriteLine("--------------------------------");

        var ollamaClient = new OllamaClient("http://localhost:11434");

        // Sistem promptu ile Türkçe yanıt vermesini sağlayalım
        var systemPrompt = "Sen Türkçe konuşan yardımcı bir yapay zeka asistanısın. " +
                          "Sorulara Türkçe, anlaşılır ve doğal bir dille cevap ver.";

        while (true)
        {
            Console.Write("\nSorunuzu yazın (çıkmak için 'exit' yazın): ");
            var input = Console.ReadLine();

            if (string.IsNullOrEmpty(input) || input.ToLower() == "exit")
                break;

            try
            {
                Console.WriteLine("\nYanıt bekleniyor...");
                var response = await ollamaClient.GenerateResponseAsync(input, systemPrompt);
                Console.WriteLine($"\nYanıt:\n{response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
            }
        }
    }
}

public class OllamaClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private const string MODEL_NAME = "neural-chat"; // Türkçe için daha uygun bir model

    public OllamaClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    public async Task<string> GenerateResponseAsync(string prompt, string systemPrompt)
    {
        var request = new
        {
            model = MODEL_NAME,
            prompt = prompt,
            system = systemPrompt,
            stream = false,
            options = new
            {
                temperature = 0.7, // Yaratıcılık seviyesi
                top_p = 0.9,      // Çeşitlilik kontrolü
                seed = 42         // Tutarlı yanıtlar için
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync($"{_baseUrl}/api/generate", content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var ollamaResponse = JsonSerializer.Deserialize<OllamaResponse>(jsonResponse);

        return ollamaResponse?.Response ?? "Yanıt alınamadı.";
    }
}

public class OllamaResponse
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("response")]
    public string? Response { get; set; }

    [JsonPropertyName("context")]
    public List<int>? Context { get; set; }
}