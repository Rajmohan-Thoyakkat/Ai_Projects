using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.ComponentModel.DataAnnotations;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKey = config["Groq:Token"] ?? throw new InvalidOperationException("Missing Groq token.");
var endpoint = new Uri("https://api.groq.com/openai/v1");
var modelId = "openai/gpt-oss-safeguard-20b";

var credential = new ApiKeyCredential(apiKey);
var endpointOptions = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

IChatClient client = new ChatClientBuilder(new OpenAIClient(credential, endpointOptions).GetChatClient(modelId).AsIChatClient())
    .UseFunctionInvocation()    
    .Build();

var ChatOptions = new ChatOptions
{
    Tools = [AIFunctionFactory.Create((string location, string unit)=>
    {
        // Here you would call a weather API to get the weather for the location
        var temperature = Random.Shared.Next(5, 20);
         var conditions = Random.Shared.Next(0, 1) == 0 ? "sunny" : "rainy";

        return $"The weather is {temperature} degrees C and {conditions}.";
    },
    "get_current_Whether",
    "Get the current weather for a given location and unit of measurement."
    )]

};

List<ChatMessage> chatHistory = [new(ChatRole.System, """
    
    You are a hiking enthusiast who helps people discover fun hikes in their area. 
    You are upbeat and friendly.
    
    """)];
chatHistory.Add(new ChatMessage(ChatRole.User, " I live in Istanbul and I'm looking for a moderate intensity hike. " +
    "\r\n    What's the current weather like? "));

Console.WriteLine($"{chatHistory.Last().Role} >>>{chatHistory.Last()}");

ChatResponse response = await client.GetResponseAsync(chatHistory, ChatOptions);
chatHistory.Add(new ChatMessage(ChatRole.Assistant, response.Text));

Console.WriteLine($"{chatHistory.Last().Role} >>>{chatHistory.Last()}");



/**
 * using Microsoft.Extensions.AI;
using OllamaSharp;

string GetCurrentWeather() => Random.Shared.NextDouble() > 0.5 ? "It's sunny" : "It's raining";

IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.1");

client = ChatClientBuilderChatClientExtensions
    .AsBuilder(client)
    .UseFunctionInvocation()
    .Build();

ChatOptions options = new() { Tools = [AIFunctionFactory.Create(GetCurrentWeather)] };

var response = client.GetStreamingResponseAsync("Should I wear a rain coat?", options);
await foreach (var update in response)
{
    Console.Write(update);
}
*/