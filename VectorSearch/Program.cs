using CommunityToolkit.VectorData.InMemory;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OpenAI;
using System.ClientModel;
using VectorSearch;

IConfiguration config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

//used OpenRouter.ai as the endpoint for the OpenAIClient, and specified the model ID for generating embeddings
var apiKey = config["OpenRouter:Token"] ?? throw new InvalidOperationException("Missing Openrouterai token.");
// I got this end point from TypeScript(OpenAI) SDK, and it works for C# as well. I have tested it and it works fine.
var endpoint = new Uri("https://openrouter.ai/api/v1");
var modelId = "liquid/lfm-2.5-embedding-350m:free";

var Options = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

//getting embedding client using the OpenAIClient and the specified model ID
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new OpenAIClient(new ApiKeyCredential(apiKey), Options)
    .GetEmbeddingClient(modelId)
    .AsIEmbeddingGenerator();

// Create and populate the vector store
var vectorStore = new InMemoryVectorStore();

var movieStore = vectorStore.GetCollection<int,Movie>("movies");
await movieStore.EnsureCollectionExistsAsync();

foreach(var movie in MovieData.Movies)
{
    // generate the embedding vector for the movie description
    movie.Vector = await embeddingGenerator.GenerateVectorAsync(movie.Description);

    // add the overall movie to the in-memory vector store's movie collection
    await movieStore.UpsertAsync(movie);
}

//1-Embed the user’s query
//2-Vectorized search
//3-Returns the records

// generate the embedding vector for the user's prompt
//var query = "I want to see family friendly movie";
var query = "A story about a young lion who becomes king";
var queryVector = await embeddingGenerator.GenerateVectorAsync(query);

// search the knowledge store based on the user's prompt
var searchResults = movieStore.SearchAsync(queryVector, top:2);

// see the results just so we know what they look like
await foreach (var result in searchResults)
{
    Console.WriteLine($"Title: {result.Record.Title}");
    Console.WriteLine($"Description: {result.Record.Description}");
    Console.WriteLine($"Score: {result.Score}");
    Console.WriteLine();
}
