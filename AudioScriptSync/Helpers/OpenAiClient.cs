using System.Text;
using System.Text.Json;

namespace AudioScriptSync;

public class OpenAiClient
{
    private readonly string apikey;
    private readonly string model;
    private readonly HttpClient http;
    public OpenAiClient(string apikey, string model = "text-davinci-003")
    {
        this.apikey = apikey;
        this.model = model;
        http = new HttpClient();
        http.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");

    }
    public async Task<string> Talk(string msg)
    {
        var json = new
        {
            prompt = msg,
            model , //ada $0.0004/1k davinci $0.02/1k
            max_tokens = 2000
        };

        var response = await http.PostAsync(
            "https://api.openai.com/v1/completions",
            new StringContent(JsonSerializer.Serialize(json), Encoding.UTF8, "application/json"));
        var responseText = await response.Content.ReadAsStringAsync();
        var data = JsonSerializer.Deserialize<OpenAiResponse>(responseText);
        var text = data.choices[0].text;
        return text;
    }
}

public class OpenAiResponse
{
    public List<OpenAiResponseChoice> choices { get; set; }
}

public class OpenAiResponseChoice
{
    public string text { get; set; }
}