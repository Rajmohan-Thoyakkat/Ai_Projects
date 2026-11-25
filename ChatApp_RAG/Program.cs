using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using ChatApp_RAG.Components;
using ChatApp_RAG.Services;
using ChatApp_RAG.Services.Ingestion;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// You will need to set the endpoint and key to your own values.
// You can do this using Visual Studio's "Manage User Secrets" UI, or on the command line:
//   cd this-project-directory
//   dotnet user-secrets set OpenAI:Key YOUR-API-KEY
//   dotnet user-secrets set OpenRouter:Token YOUR-OPENROUTER-TOKEN

var credential = new ApiKeyCredential(builder.Configuration["OpenRouter:Token"] 
                    ?? throw new InvalidOperationException("OpenRouter:Token is not set in user secrets."));
var OpenAIOption = new OpenAIClientOptions
{
    Endpoint = new Uri("https://openrouter.ai/api/v1"),
};

var openAIClient = new OpenAIClient(credential, OpenAIOption);

#pragma warning disable OPENAI001 // GetResponsesClient() is experimental and subject to change or removal in future updates.
var chatClient = openAIClient.GetResponsesClient().AsIChatClient("openai/gpt-4o-mini");
#pragma warning restore OPENAI001

var embeddingGenerator = openAIClient.GetEmbeddingClient("openai/text-embedding-3-small").AsIEmbeddingGenerator();

var vectorStorePath = Path.Combine(AppContext.BaseDirectory, "vector-store.db");
var vectorStoreConnectionString = $"Data Source={vectorStorePath}";
builder.Services.AddSqliteVectorStore(_ => vectorStoreConnectionString);
builder.Services.AddSqliteCollection<Guid, IngestedChunk>(IngestedChunk.CollectionName, vectorStoreConnectionString);

builder.Services.AddSingleton<DataIngestor>();
builder.Services.AddSingleton<SemanticSearch>();
builder.Services.AddKeyedSingleton("ingestion_directory", new DirectoryInfo(Path.Combine(builder.Environment.WebRootPath, "Data")));
builder.Services.AddChatClient(chatClient).UseFunctionInvocation().UseLogging();
builder.Services.AddEmbeddingGenerator(embeddingGenerator);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();

app.UseStaticFiles();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// By default, documents are ingested on demand from the /wwwroot/Data directory.
// Important: ensure that any content you ingest is trusted, as it may be reflected back
// to users or could be a source of prompt injection risk.

app.Run();
