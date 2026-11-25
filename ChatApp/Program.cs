using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using System.ClientModel;
using OpenAI;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKey = config["Groq:Token"] ?? throw new InvalidOperationException("Missing Groq token.");
var endpoint = new Uri("https://api.groq.com/openai/v1");
var modelId = "openai/gpt-oss-safeguard-20b";

var credential = new ApiKeyCredential(apiKey);
var options = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

IChatClient client = new OpenAIClient(credential, options)
    .GetChatClient(modelId)
    .AsIChatClient();


//For using Ollama API, you can use the following code snippet instead of the above OpenAI client initialization:
//locatlhost getting from docker run -p 11434:11434 ollama/ollama:latest
//IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"), llama3.2);

#region Chatapp

// Start the conversation with context for the AI model
List<ChatMessage> chatHistory = new() 
{
    new ChatMessage(ChatRole.System, """
            You are a friendly hiking enthusiast who helps people discover fun hikes in their area.
            You introduce yourself when first saying hello.
            When helping people out, you always ask them for this information
            to inform the hiking recommendation you provide:

            1. The location where they would like to hike
            2. What hiking intensity they are looking for

            You will then provide three suggestions for nearby hikes that vary in length
            after you get that information. You will also share an interesting fact about
            the local nature on the hikes when making a recommendation. At the end of your
            response, ask if there is anything else you can help with.
        """)
};

while(true)
{
    Console.WriteLine("Your Prompt :");
    var userPrompt = Console.ReadLine();
    chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));

    //Stream the response from the AI model
    Console.WriteLine("Ai reply :");
    var response = " ";
    await foreach (var item in client.GetStreamingResponseAsync(chatHistory))
    {
        Console.Write(item.Text);
        response += item.Text;
    }

    chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));
    Console.WriteLine();
}
#endregion
