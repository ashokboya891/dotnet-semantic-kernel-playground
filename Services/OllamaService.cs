using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ollamaDemo.Plugins;

namespace ollamaDemo.Services
{
 
    public class OllamaService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chat;

        public OllamaService(
            string baseUrl,
            string model,
            string apiKey)
        {
            var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            var builder = Kernel.CreateBuilder();

            // Register local Ollama model.
            builder.AddOpenAIChatCompletion(
                modelId: model,
                apiKey: apiKey,
                endpoint: new Uri(baseUrl),
                httpClient: httpClient);

            // Register Cricket plugin.
            builder.Plugins.AddFromType<CricketPlugin>("Cricket");

            // Build Kernel.
            _kernel = builder.Build();

            // Get chat completion service.
            _chat = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> AskAsync(string question)
        {
            var history = new ChatHistory();

            history.AddSystemMessage("""
            You are a Cricket AI Assistant.

            Answer cricket-related questions in simple language.

            You have access to Cricket plugin functions.
            Use the appropriate function when it helps answer
            the user's question.

            Do not invent function results.
            """);

            history.AddUserMessage(question);

            var settings = new OpenAIPromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
                Temperature = 0
            };

            var response = await _chat.GetChatMessageContentAsync(
                history,
                executionSettings: settings,
                kernel: _kernel);

            return response.Content ?? "No response received.";
        }
    }
    //public class OllamaService
    //{
    //    private readonly IChatCompletionService _chat;

    //    public OllamaService(string baseUrl, string model, string apiKey)
    //    {
    //        var httpClient = new HttpClient
    //        {
    //            BaseAddress = new Uri(baseUrl),
    //            Timeout = TimeSpan.FromMinutes(5)
    //        };

    //        var builder = Kernel.CreateBuilder();

    //        builder.AddOpenAIChatCompletion(
    //            modelId: model,
    //            apiKey: apiKey,
    //            endpoint: new Uri("http://localhost:11434/v1"),
    //            httpClient: httpClient);

    //        var kernel = builder.Build();

    //        _chat = kernel.GetRequiredService<IChatCompletionService>();
    //    }

    //    public async Task<string> AskAsync(string question)
    //    {
    //        var history = new ChatHistory();

    //        history.AddSystemMessage(
    //            "You are a helpful Cricket AI Assistant. " +
    //            "Answer cricket-related questions in simple language.");

    //        history.AddUserMessage(question);

    //        var response = await _chat.GetChatMessageContentAsync(history);

    //        return response.Content ?? "No response received.";
    //    }
    //}
}
