Day 1 – Building a Local AI Application Using .NET, Ollama & Semantic Kernel

Date: 31 October 2026
Project: Cricket AI Assistant
Technology: C#, .NET 9, Ollama, Llama 3.2, Semantic Kernel

---

1. Learning Objective

Today's objective was to understand how to integrate a locally hosted Large Language Model (LLM) into a .NET application using Semantic Kernel.

We built a basic Cricket AI Assistant that can:

- Answer cricket-related questions.
- Connect a local LLM to a .NET application.
- Execute C# functions through AI function calling.
- Use Semantic Kernel plugins to expose application functionality to an LLM.

2. Important Definitions

2.1 Artificial Intelligence (AI)

Definition: Artificial Intelligence is a technology that enables machines to perform tasks that typically require human intelligence, such as understanding language, solving problems, learning and making decisions.

Example: A cricket assistant that understands questions and explains cricket rules.

2.2 Large Language Model (LLM)

Definition: A Large Language Model is an AI model trained on large amounts of text data to understand and generate human-like language.

Examples:

- GPT
- Llama
- Gemini
- Claude

In our project, we used Llama 3.2.

2.3 Ollama

Definition: Ollama is a tool that allows developers to download and run LLMs locally on their machines.

Why we used Ollama:

- Run AI models locally.
- No requirement for a paid cloud AI API.
- Experiment with different models.
- Connect local models to .NET applications.

Useful commands:

docker exec -it ollama ollama list
docker exec -it ollama ollama ps
docker exec -it ollama ollama run llama3.2:1b

2.4 Local LLM

Definition: A Local LLM is a language model running on your own computer or local server instead of relying on a remote AI service.

Our setup:

.NET Console Application
        |
        v
Semantic Kernel
        |
        v
Ollama API
        |
        v
Llama 3.2:1b
        |
        v
AI Response

2.5 Docker

Definition: Docker is a platform used to package and run applications in isolated environments called containers.

We used Docker Desktop to run Ollama.

Our container exposed:

localhost:11434

2.6 API (Application Programming Interface)

Definition: An API is a communication interface that allows two software applications to exchange information.

In our project, the .NET application communicates with Ollama through its HTTP API.

Two Ollama API styles we explored:

Native API:

http://localhost:11434/api/chat

OpenAI-compatible API:

http://localhost:11434/v1/chat/completions

The Semantic Kernel OpenAI-compatible connector uses the second API style.

3. Understanding Semantic Kernel

3.1 What is Semantic Kernel?

Definition: Semantic Kernel is an open-source SDK from Microsoft that helps developers integrate AI models into applications and orchestrate their capabilities using programming languages such as C#.

It acts as a bridge between an AI model and our application.

Why do we need Semantic Kernel?

An LLM by itself can generate responses, but it does not automatically know how to:

- Execute our C# business logic.
- Access our application services.
- Call our third-party APIs.
- Retrieve application data.

Semantic Kernel helps connect these capabilities to the model.

Main capabilities

1. Connect AI models to applications.
2. Register plugins and functions.
3. Support automatic function calling.
4. Manage AI interactions.
5. Integrate application logic and external services.

3.2 Semantic Kernel Architecture

       .NET Application
              |
              v
       Semantic Kernel
              |
       +------+------+
       |             |
       v             v
   Chat Service    Plugins
       |             |
       v             v
     Ollama       C# Functions
       |
       v
    Llama 3.2

4. Project Setup

4.1 Create a .NET Console Application

Command:

dotnet new console -n ollamaDemo

Move into the project:

cd ollamaDemo

4.2 Install NuGet Packages

We installed:

dotnet add package Microsoft.SemanticKernel

dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI

Microsoft.SemanticKernel: Provides the core Semantic Kernel functionality.

Microsoft.SemanticKernel.Connectors.OpenAI: Provides the OpenAI-compatible connector used to communicate with Ollama's compatible API.

4.3 Project Structure

Our basic project structure:

ollamaDemo
│
├── Plugins
│   └── CricketPlugin.cs
│
├── Services
│   └── OllamaService.cs
│
├── Program.cs
│
└── appsettings.json

5. Understanding appsettings.json

Definition: appsettings.json is a configuration file commonly used in .NET applications to store application settings separately from source code.

Our configuration:

{
  "Ollama": {
    "BaseUrl": "http://localhost:11434/v1",
    "Model": "llama3.2:1b",
    "ApiKey": "ollama"
  }
}

Configuration properties

Property| Purpose
BaseUrl| Ollama OpenAI-compatible API base address
Model| Name of the model to use
ApiKey| Placeholder API key accepted by the connector for local Ollama

Reading JSON configuration in C#

We used "System.Text.Json" to deserialize the JSON file.

Definition – Serialization: Converting an object into a JSON or another data format.

Definition – Deserialization: Converting JSON or another data format back into an object.

Example:

var configPath = Path.Combine(
    AppContext.BaseDirectory,
    "appsettings.json");

var config = JsonSerializer.Deserialize<AppConfig>(
    File.ReadAllText(configPath),
    new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

Important: "AppContext.BaseDirectory" returns the application's base directory. In our project, it points to the output folder containing the running application.

This helped us load the correct appsettings.json file.

6. Connecting Semantic Kernel to Ollama

We configured Semantic Kernel to communicate with Ollama using the OpenAI-compatible connector.

Example:

builder.AddOpenAIChatCompletion(
    modelId: model,
    apiKey: "ollama",
    endpoint: new Uri(baseUrl),
    httpClient: httpClient);

Important concepts

IChatCompletionService

Definition: A Semantic Kernel abstraction used to communicate with chat completion models.

It allows our application to send conversation messages to an AI model and receive responses.

Endpoint

Definition: An endpoint is a specific address through which an application or service can be accessed.

Our configured base endpoint:

http://localhost:11434/v1

Why "/v1" is important:

The OpenAI-compatible connector expects the "/v1" API base path. Without it, the connector may construct an incorrect request URL and return HTTP 404.

7. Understanding Plugins

7.1 What is a Plugin?

Definition: A plugin is a collection of functions that extends the capabilities of an AI application by exposing application functionality to the AI model.

For example, our Cricket plugin exposes cricket-related C# methods to the LLM.

Possible future plugins:

- CricketPlugin
- WeatherPlugin
- CalculatorPlugin
- DatabasePlugin
- EmailPlugin

7.2 What is a Kernel Function?

Definition: A Kernel Function is a method exposed to Semantic Kernel so that an AI model can request its execution.

We use the "[KernelFunction]" attribute to identify methods that should be available to the kernel.

Example:

[KernelFunction("ExplainCricketTerm")]
[Description("Explains a cricket term.")]
public string ExplainCricketTerm(string term)
{
    // Cricket explanation logic
}

Important attributes

[KernelFunction]

Marks a method as a function that Semantic Kernel can expose to the model.

[Description]

Provides information about a function or its parameters. Semantic Kernel uses these descriptions when making functions available to the model.

Descriptions help the model understand when and how to use a function.

7.3 Registering a Plugin

We registered our Cricket plugin using:

builder.Plugins.AddFromType<CricketPlugin>("Cricket");

Explanation:

- "AddFromType" registers a plugin using its C# class.
- "CricketPlugin" is our plugin class.
- ""Cricket"" is the plugin name.

After registration, the kernel can discover the functions exposed by that plugin.

8. Automatic Function Calling

8.1 What is Function Calling?

Definition: Function calling is a capability that allows an AI model to request the execution of a predefined application function when it determines that the function is useful for answering a user's question.

Example:

User:

"What is a yorker ball?"

The model can identify that the "ExplainCricketTerm" function is relevant and request its execution.

8.2 How Does Function Calling Work?

User asks a question
        |
        v
      LLM
        |
        v
Identifies relevant function
        |
        v
Requests function execution
        |
        v
Semantic Kernel
        |
        v
Executes C# method
        |
        v
Returns function result to LLM
        |
        v
LLM generates final answer

Important: The LLM generally requests a function call. Semantic Kernel handles the actual function invocation in the application.

8.3 Enabling Automatic Function Calling

We used:

var settings = new OpenAIPromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(),
    Temperature = 0
};

FunctionChoiceBehavior.Auto()

Definition: Configures the model interaction to allow automatic selection of available functions.

The model can decide whether a function is required for a question.

Temperature

Definition: Temperature controls the randomness of the model's generated responses.

- Lower temperature: More predictable responses.
- Higher temperature: More variation and creativity.

We used "Temperature = 0" to encourage more consistent responses.

9. Understanding Our Cricket Plugin

Our plugin exposed cricket-related functions.

Examples of functions:

ExplainCricketTerm

Purpose: Explains cricket terminology.

Example questions:

- What is a yorker?
- What is LBW?
- What is a maiden over?
- What is retired hurt?

CalculateBattingStrikeRate

Purpose: Calculates a batsman's strike rate.

Formula:

Strike Rate = (Runs Scored / Balls Faced) × 100

Example:

Runs = 75
Balls Faced = 50

Strike Rate = (75 / 50) × 100

Strike Rate = 150

This demonstrates how we can expose our own C# business logic to an AI model.

10. Problems Encountered and Solutions

Problem 1: Ollama connection issues

Problem: The application could not communicate with Ollama.

Solution: Verified the Docker container and Ollama API.

docker exec -it ollama ollama list

We also verified the API through:

http://localhost:11434/api/tags

Problem 2: HTTP 404 – Not Found

Problem: The application returned:

Service request failed.
Status: 404 (Not Found)

Root cause: The BaseUrl in appsettings.json was:

http://localhost:11434

But the Semantic Kernel OpenAI-compatible connector required:

http://localhost:11434/v1

Solution: Updated the configuration.

Problem 3: JSON configuration loading

Problem: The application needed to read the configuration file from the output directory.

Solution:

Path.Combine(
    AppContext.BaseDirectory,
    "appsettings.json");

This ensures the application reads the file from the correct runtime location.

Problem 4: Incorrect AI responses

We tested questions such as:

- What is a run out?
- What is retired hurt?
- What is a duck out?
- What is a hat-trick?

The model generated some incorrect cricket explanations and occasionally selected an unrelated function.

Reason: The small Llama 3.2 1B model has limited reasoning and function-selection reliability.

Learning: Function calling does not guarantee factual accuracy. An AI application needs validation and reliable data sources.

11. Important Distinction: LLM vs Semantic Kernel vs Plugin

Component| Responsibility
LLM| Understands prompts and generates responses
Ollama| Runs and serves the local LLM
Semantic Kernel| Connects and orchestrates AI capabilities within the application
Plugin| Groups application capabilities exposed to the AI
Kernel Function| Exposes an individual C# method
.NET Application| Hosts the application logic and controls execution

Example:

If a user asks, "Calculate my batting strike rate":

1. The LLM understands the request.
2. Semantic Kernel provides the available function descriptions.
3. The model requests "CalculateBattingStrikeRate".
4. Semantic Kernel executes the C# function.
5. The result is returned to the model.
6. The model presents the result to the user.

12. What I Learned Today

- What Artificial Intelligence and LLMs are.
- What Ollama is and how it runs local models.
- How to use Docker to host Ollama.
- How to create a .NET 9 console application.
- How to install Semantic Kernel NuGet packages.
- How to configure Ollama through appsettings.json.
- How to deserialize JSON configuration in C#.
- How to connect Ollama with Semantic Kernel.
- What "IChatCompletionService" does.
- What Semantic Kernel plugins are.
- What "[KernelFunction]" and "[Description]" attributes do.
- How to register a plugin.
- How automatic function calling works.
- How Semantic Kernel executes C# functions based on model requests.
- Why small models can produce hallucinations and incorrect function selections.

13. Next Learning Topics

Recommended learning sequence:

Order| Topic| Objective
1| Chat History| Maintain previous conversation context
2| Prompt Engineering| Create effective system and user prompts
3| Structured Output| Generate predictable JSON responses
4| Embeddings| Convert text into numerical vector representations
5| Vector Database| Store and search vector representations
6| RAG| Generate answers using trusted external knowledge
7| AI Agents| Enable AI systems to perform multi-step tasks

14. Final Summary

Today, I built the foundation of a local AI application using .NET 9, Ollama and Semantic Kernel.

I connected the Llama 3.2 model to my .NET console application, configured the connection through appsettings.json, created a Cricket plugin and exposed C# methods using Kernel Functions.

I also explored automatic function calling, where the LLM identifies a relevant function and Semantic Kernel executes the corresponding C# method.

My biggest takeaway: An LLM alone generates responses, but Semantic Kernel helps connect the LLM to real application functionality and business logic.

This is the foundation for building more advanced AI applications, including RAG-based systems and AI agents.