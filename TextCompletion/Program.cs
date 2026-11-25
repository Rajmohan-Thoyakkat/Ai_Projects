using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenAI;
using System.ClientModel;
using System.Net;
using System.Text.Json.Serialization;

IConfigurationRoot config = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

var apiKey = config["Groq:Token"] ?? throw new InvalidOperationException("Missing Groq token.");
var endpoint = new Uri("https://api.groq.com/openai/v1");
var modelId = "openai/gpt-oss-safeguard-20b";

var credential = new ApiKeyCredential(apiKey);
var options = new OpenAIClientOptions()
{
    Endpoint = endpoint
};

// Create chat client using the OpenAIClient and the specified model ID
IChatClient client = new OpenAIClient(credential, options)
    .GetChatClient(modelId)
    .AsIChatClient();

#region Basic Chat Request
//Get and send a request to the chat model
//ChatResponse response = await client.GetResponseAsync("what is Ai. Explain Max 20 words");
//Console.WriteLine(response);
#endregion

#region Chat Request with System and User Messages

//string prompt = "what is Ai. Explain Max 20 words";
//Console.WriteLine($"User >>> {prompt}");

//ChatResponse response = await client.GetResponseAsync(prompt);
//Console.WriteLine($"Assistant >>> { response}");

//Console.WriteLine($"Token Used : input {response.Usage?.InputTokenCount},output{response.Usage?.OutputTokenCount}");

#endregion

#region Streaming

//string prompt = "what is Ai. Explain Max 200 words";
//Console.WriteLine($"User >>> {prompt}");

//var responseStream = client.GetStreamingResponseAsync(prompt);

//await foreach (var message in responseStream)
//{
//    Console.Write(message.Text);
//}

#endregion

#region Classification
//var classificationPrompt = "Classify the following text into categories: " +
//    "'-Complaint'" +
//    "'-Suggestion'" +
//    "'-Feedback'" +
//    "-Other'." +
//    "1. I love you the layout" +
//    "2. You should add a night mode" +
//    "3. The app crashes frequently" +
//    "4. The layout is confusing";

//Console.WriteLine($"User >>> {classificationPrompt}");

//ChatResponse response = await client.GetResponseAsync(classificationPrompt);
//Console.WriteLine($"Assistant >>> \n{response}");


#endregion

#region Summarization

//var summarizationPrompt = "Summarize the following text in 20 words or less: " +
//    "'The quick brown fox jumps over the lazy dog. The dog barked at the fox, but the fox was too quick.'";

//Console.WriteLine($"User >>> {summarizationPrompt}");

//ChatResponse summarizationResponse = await client.GetResponseAsync(summarizationPrompt);
//Console.WriteLine($"Assistant >>> \n{summarizationResponse}");

#endregion

#region Sentiment Analysis

//var sentimentPrompt = "Analyze the sentiment of the following text: " +
//    "'I had a great experience with your service. The staff was friendly and helpful.'";

//Console.WriteLine($"User >>> {sentimentPrompt}");

//ChatResponse response = await client.GetResponseAsync(sentimentPrompt);
//Console.WriteLine($"Assistant >>> \n{response}");

#endregion

#region Structered Output

var carLists = new[]
{
    "Check out this stylish 2019 Toyota Camry. It has a clean title, only 40,000 miles on the odometer, and a well-maintained interior. The car offers great fuel efficiency, a spacious trunk, and modern safety features like lane departure alert. Minimum offer price: $18,000. Contact Metro Auto at (555) 111-2222 to schedule a test drive.",
    "Lease this sporty 2021 Honda Civic! With only 10,000 miles, it includes a sunroof, premium sound system, and backup camera. Perfect for city driving with its compact size and great fuel mileage. Located in Uptown Motors, monthly lease starts at $250 (excl. taxes). Call (555) 333-4444 for more info.",
    "A classic 1968 Ford Mustang, perfect for enthusiasts. The vehicle needs some interior restoration, but the engine runs smoothly. V8 engine, manual transmission, around 80,000 miles. This vintage gem is priced at $25,000. Contact Retro Wheels at (555) 777-8888 if you’re interested.",
    //"Brand new 2023 Tesla Model 3 for lease. Zero miles, fully electric, autopilot capabilities, and a sleek design. Monthly lease starts at $450. Clean lines, minimalist interior, top-notch performance. For more details, call EVolution Cars at (555) 999-0000.",
    //"Selling a 2015 Subaru Outback in good condition. 60,000 miles on it, includes all-wheel drive, heated seats, and ample cargo space for family getaways. Minimum offer price: $14,000. Contact Forrest Autos at (555) 222-1212 if you want a reliable adventure companion.",

};

foreach(var carlist in carLists)
{
    var response = await client.GetResponseAsync<carInfo>(
         $"""
        Convert the following car listing into a JSON object matching this C# schema:
        Condition: "New" or "Used"
        Make: (car manufacturer)
        Model: (car model)
        Year: (four-digit year)
        ListingType: "Sale" or "Lease"
        Price: integer only
        Features: array of short strings
        TenWordSummary: exactly ten words to summarize this listing

        Here is the listing:
        {carlist}
        """);

    if (response.TryGetResult(out var info))
    {
        // Convert the CarDetails object to JSON for display
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(
            info, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
    }
    else
    {
        Console.WriteLine("Response was not in the expected format.");
    }

}



class carInfo
{
    public required string Condition   { get; set; }//eg/ new or used
    public required string Make { get; set; }//eg/ Toyota, Ford, Honda
    public required string Model { get; set; }//eg/ Camry, Mustang, Civic
    public int Year { get; set; }//eg/ 2019, 2021, 1968
    public int Price { get; set; }//eg/ 18000, 25000, 14000
    public required string[] Features { get; set; }
    public required string TenWordsSummary   { get; set; }

    public required CarListingType ListingType { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
enum CarListingType
{    Sale,
    Lease
}
#endregion
