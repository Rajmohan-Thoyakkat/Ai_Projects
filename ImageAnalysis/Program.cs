using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;

IConfiguration config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKey = config["OpenRouter:Token"] ?? throw new InvalidOperationException("Missing OpenRouter token.");
var endpoint = new Uri("https://openrouter.ai/api/v1/images");
var modelId = "openai/gpt-image-2.5-sunburst";

var credential = new ApiKeyCredential(apiKey);
var endpointOptions = new OpenAIClientOptions()
{
    Endpoint = endpoint
};


IChatClient client = new ChatClientBuilder(new OpenAIClient(credential, endpointOptions).GetChatClient(modelId).AsIChatClient())
    .UseFunctionInvocation()
    .Build();

//user prompt
var promptDescribe = "Describe the image";
var promptAnalyser = "How many red cars are in the image? and what are the other car colors?";

//prompts
string systemPrompt = """
    You are an image analysis assistant. You will be provided with an image and a prompt. 
    Your task is to analyze the image and provide a detailed response based on the prompt.
    """;
var userPrompt = promptDescribe;

List<ChatMessage> messages = 
[
    new ChatMessage(ChatRole.System, systemPrompt),
    new ChatMessage(ChatRole.User, userPrompt)
];

//read image from files
var imagename = "cars.png";
string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "Images", imagename);


AIContent aIContent = new DataContent(File.ReadAllBytes(imagePath), "image/png");
var message = new ChatMessage(ChatRole.User, [aIContent]);
messages.Add(message);

var response = await client.GetResponseAsync(messages);
Console.WriteLine($"Prompt: {userPrompt}");
Console.WriteLine($"Image: {imagename}");
Console.WriteLine($"Response: {response}");