
//using Microsoft.Extensions.AI;
//using OllamaSharp;

//// Connect to local Ollama
//IChatClient client = new OllamaApiClient(
//    new Uri("http://localhost:11434/"),
//    "llama3.2:1b "
//);

//// Send prompt
//var response = await client.GetResponseAsync("tell me what is solid prinicples means.?"
//);

//// Display response
//Console.WriteLine(response.Text);
//Console.ReadKey();

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

// Disable experimental connector warning
#pragma warning disable SKEXP0070

// ==========================================
// 1. Configure HttpClient
// ==========================================

using var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:11434"),
    Timeout = TimeSpan.FromMinutes(5)
};

// ==========================================
// 2. Create Kernel Builder
// ==========================================

var builder = Kernel.CreateBuilder();

// ==========================================
// 3. Register Ollama Chat Completion
// ==========================================

builder.AddOllamaChatCompletion(
    modelId: "qwen2.5:0.5b",
    httpClient: httpClient
);

// ==========================================
// 4. Build Kernel
// ==========================================

Kernel kernel = builder.Build();

// ==========================================
// 5. Get Chat Completion Service
// ==========================================

var chat = kernel.GetRequiredService<IChatCompletionService>();

// ==========================================
// 6. Create Chat History
// ==========================================

var history = new ChatHistory();

// System instructions
history.AddSystemMessage("""
    You are an intelligent, helpful and versatile AI assistant.

    Your areas of expertise include:

    1.Technology:
       - .NET, C#, Angular, SQL, Azure and programming.
       - AI, Machine Learning, RAG and AI Agents.
       - Software architecture, debugging and best practices.

    2.Current Affairs:
       -National and international news.
       - Politics, economy, science, technology and sports.
       - Important events and general knowledge.
       - Clearly mention when information may be outdated.
       - Never invent current news or statistics.

    3.Competitive Exams:
       -General Knowledge and General Awareness.
       - Logical reasoning and aptitude.
       - Quantitative aptitude.
       - Verbal ability.
       - Exam preparation, practice questions and explanations.

    4.Mathematics and Arithmetic:
       -Basic arithmetic, percentages and ratios.
       - Profit and loss, averages and interest.
       - Time and work, speed and distance.
       - Algebra, probability and number systems.
       - Explain calculations step by step.

    5.Games and Sports:
       -Cricket, football, chess and other games.
       - Rules, strategies, statistics and trivia.
       - Explain game concepts and techniques.
       - Do not invent live scores or current results.

    Response guidelines:
    -Understand the user's question before answering.
    - Use simple, clear and understandable English.
    - Give accurate and practical answers.
    - Explain complex concepts with examples.
    - For mathematics, show step - by - step calculations.
    - For programming, provide practical code examples.
    - Use bullet points and tables when useful.
    - Keep simple answers concise.
    - If you are unsure, clearly acknowledge it.
    - Distinguish verified facts from assumptions.
    -Do not pretend to have real - time internet access.

    Your goal is to be a reliable general - purpose assistant
    for learning, problem - solving and everyday questions.
    """);

// ==========================================
// 7. Continuous Chat Loop
// ==========================================

Console.WriteLine("=================================");
Console.WriteLine("   Local AI Chat - Ollama");
Console.WriteLine("=================================");
Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    Console.Write("\nQ: ");

    var userQuestion = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userQuestion))
        continue;

    if (userQuestion.Equals("exit",
        StringComparison.OrdinalIgnoreCase))
        break;

    // Add user message to history
    history.AddUserMessage(userQuestion);

    try
    {
        // Get AI response
        var response = await chat.GetChatMessageContentAsync(
            history,
            kernel: kernel
        );

        // Add AI response to history
        history.AddAssistantMessage(response.Content ?? "");

        // Display response
        Console.WriteLine("\nAI: " + response.Content);
    }
    catch (TaskCanceledException ex)
    {
        Console.WriteLine("Request timed out: " + ex.Message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }
}

Console.WriteLine("\nChat ended.");

