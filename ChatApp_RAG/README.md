# AI Chat with Custom Data

This project is an AI chat application that demonstrates how to chat with custom data using an AI language model. Please note that this template is currently in an early preview stage. If you have feedback, please take a [brief survey](https://aka.ms/dotnet-chat-templatePreview2-survey).

>[!NOTE]
> Before running this project you need to configure the API keys or endpoints for the providers you have chosen. See below for details specific to your choices.

# Configure the AI Model Provider

This project can use either OpenAI or OpenRouter. Configure one of the following using .NET User Secrets:

1. In Visual Studio, right-click the project in Solution Explorer and select "Manage User Secrets".
2. Add one of these values to `secrets.json`:

   ```json
   {
     "OpenAI:Key": "YOUR-API-KEY"
   }
   ```

   or

   ```json
   {
     "OpenRouter:Token": "YOUR-OPENROUTER-TOKEN"
   }
   ```

If you use OpenRouter, make sure your account has access to the selected model.

