using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System.ClientModel;
using System.Numerics.Tensors;


IConfiguration config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

//used OpenRouter.ai as the endpoint for the OpenAIClient, and specified the model ID for generating embeddings
var apiKey = config["OpenRouter:Token"] ?? throw new InvalidOperationException("Missing Openrouterai token.");
// I got this end point from TypeScript(OpenAI) SDK, and it works for C# as well. I have tested it and it works fine.
var endpoint = new Uri("https://openrouter.ai/api/v1"); 
var modelId = "liquid/lfm-2.5-embedding-350m:free";

var options = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

//getting chat client using the OpenAIClient and the specified model ID
IChatClient client = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetChatClient(modelId)
    .AsIChatClient();

//getting embedding client using the OpenAIClient and the specified model ID
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OpenAIClient(new ApiKeyCredential(apiKey), options)
    .GetEmbeddingClient(modelId)
    .AsIEmbeddingGenerator();

//Generating embedding for a given text
//var embeddingResponse = await embeddingGenerator.GenerateVectorAsync("Hello, world!");
//Console.WriteLine($"Embedding vector length: {embeddingResponse.Span.Length}");
//foreach (var value in embeddingResponse.Span)
//{
//    Console.WriteLine(value);
//}


//comparing embeddings for multiple different texts
var catVector = await embeddingGenerator.GenerateVectorAsync("cat");
var dogVector = await embeddingGenerator.GenerateVectorAsync("dog");
var kittenVector = await embeddingGenerator.GenerateVectorAsync("kitten");

Console.WriteLine($"Cosine similarity between 'cat' and 'dog': {TensorPrimitives.CosineSimilarity(catVector.Span, dogVector.Span)}");
Console.WriteLine($"Cosine similarity between 'cat' and 'kitten': {TensorPrimitives.CosineSimilarity(catVector.Span, kittenVector.Span)}");
Console.WriteLine($"Cosine similarity between 'dog' and 'kitten': {TensorPrimitives.CosineSimilarity(dogVector.Span, kittenVector.Span)}");






