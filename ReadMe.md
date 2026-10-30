# Local AI Development with .NET – Complete Hands-on Learning Notes

**Date:** 30 October 2026  
**Project:** Local AI Agent Learning  
**Technologies:** .NET, C#, Docker, Ollama, Qwen 2.5, Semantic Kernel

---

# 1. Objective

The objective was to run an AI model locally on my machine and integrate it with a .NET application without depending on paid cloud AI APIs.

The learning goals were:

1. Understand how local LLMs work.
2. Run an AI model using Docker and Ollama.
3. Communicate with the local AI model through REST APIs.
4. Integrate an LLM into a .NET application.
5. Understand Microsoft's Semantic Kernel.
6. Maintain conversation history using ChatHistory.
7. Prepare the foundation for learning RAG and AI Agents.

# 2. Architecture Overview

The architecture implemented:

```text
                 User
                  |
                  v
          .NET Console App
                  |
                  v
           Semantic Kernel
                  |
                  v
        IChatCompletionService
                  |
                  v
             Ollama API
          localhost:11434
                  |
                  v
          Docker Container
                  |
                  v
          Qwen 2.5 (0.5B)
                  |
                  v
          Generated Response
```

Technology responsibilities:

| Technology | Responsibility |
|---|---|
| .NET 10 | Application development |
| C# | Application programming |
| Docker Desktop | Container runtime |
| Ollama | Local LLM server |
| Qwen 2.5 (0.5B) | Language model |
| Semantic Kernel | AI integration and orchestration |
| ChatHistory | Conversation context |

# 3. Step 1 – Install Docker Desktop

Ollama can run locally using Docker without installing the Ollama Windows application.

Download Docker Desktop:

https://www.docker.com/products/docker-desktop/

Installation steps:

1. Download Docker Desktop for Windows.
2. Install it.
3. Start Docker Desktop.
4. Ensure the Linux container engine is running.
5. Verify Docker from PowerShell.

Check Docker:

```powershell
docker --version
```

Check Docker engine:

```powershell
docker version
```

The Docker version command should display both Client and Server information.

### Initial issue encountered

Error:

```text
docker: error during connect:

open //./pipe/dockerDesktopLinuxEngine:
The system cannot find the file specified.
```

Reason:

Docker Desktop's Linux engine was not running or accessible.

Resolution:

1. Open Docker Desktop.
2. Wait until the engine starts.
3. Verify Docker Server using `docker version`.
4. Retry the Docker command.

If Docker cannot start, check WSL:

```powershell
wsl --status
```

# 4. Step 2 – Run Ollama using Docker

Ollama is a local LLM runtime that allows applications to interact with language models through HTTP APIs.

Instead of installing Ollama directly on Windows, we used its official Docker image.

Navigate to the working directory:

```powershell
mkdir C:\Users\ashok\agents
cd C:\Users\ashok\agents
```

Start Ollama:

```powershell
docker run -d `
  -v ollama:/root/.ollama `
  -p 11434:11434 `
  --name ollama `
  ollama/ollama
```

### Understanding the command

| Parameter | Explanation |
|---|---|
| `docker run` | Creates and starts a container |
| `-d` | Runs in the background |
| `-v ollama:/root/.ollama` | Creates persistent model storage |
| `-p 11434:11434` | Exposes Ollama's API port |
| `--name ollama` | Assigns a container name |
| `ollama/ollama` | Official Ollama Docker image |

### Verify the container

```powershell
docker ps
```

Expected:

```text
CONTAINER ID   IMAGE          STATUS
xxxxxxxx       ollama/ollama  Up
```

### View container logs

```powershell
docker logs ollama
```

Follow logs continuously:

```powershell
docker logs -f ollama
```

Important log:

```text
Listening on [::]:11434
```

This confirms that the Ollama API server has started.

### Test Ollama in the browser

Open:

http://localhost:11434

Expected response:

```text
Ollama is running
```

# 5. Step 3 – Understand Ollama Models

Ollama provides a library of downloadable language models.

Model selection depends on:

- Available RAM
- CPU/GPU capability
- Model size
- Response speed
- Quality requirements

Initially, Phi-4-mini was considered. However, Docker logs showed limited available memory.

The environment reported:

```text
Inference compute: CPU
Total memory: 3.7 GiB
Available memory: 1.6 GiB
```

Therefore, a smaller model was selected.

### Selected model

**Qwen 2.5 – 0.5B**

Download the model:

```powershell
docker exec -it ollama ollama pull qwen2.5:0.5b
```

List downloaded models:

```powershell
docker exec -it ollama ollama list
```

Start an interactive chat:

```powershell
docker exec -it ollama ollama run qwen2.5:0.5b
```

Example:

```text
>>> What is dependency injection?

Dependency injection is a design pattern that
provides dependencies to a class rather than
creating them inside the class.
```

Exit:

```text
/bye
```

# 6. Step 4 – Understand Ollama REST API

Before integrating Semantic Kernel, we tested the model directly using its REST API.

Ollama provides this endpoint:

```text
POST http://localhost:11434/api/generate
```

### PowerShell test

Prepare the request:

```powershell
$body = @{
    model = "qwen2.5:0.5b"
    prompt = "Explain dependency injection in one sentence."
    stream = $false
    options = @{
        num_predict = 64
    }
} | ConvertTo-Json -Depth 5
```

Send the request:

```powershell
Invoke-RestMethod `
    -Uri "http://localhost:11434/api/generate" `
    -Method Post `
    -ContentType "application/json" `
    -Body $body `
    -TimeoutSec 300
```

### Understanding the request

| Property | Purpose |
|---|---|
| `model` | Specifies which model to use |
| `prompt` | Input question |
| `stream` | Controls streaming response |
| `num_predict` | Limits generated tokens |
| `TimeoutSec` | Maximum time PowerShell waits |

### Successful response

```text
model: qwen2.5:0.5b

response:
Dependency injection in software development
allows for better separation of concerns,
enhancing modularity and making the code
more maintainable and testable.

done: True
```

### Important performance observations

| Metric | Result |
|---|---:|
| Total duration | 14.7 seconds |
| Model loading duration | 12.1 seconds |
| Prompt evaluation | 0.7 seconds |
| Response generation | 1.8 seconds |
| Generated tokens | 27 |

These timings were from one test and will vary depending on whether the model is already loaded.

**Learning:** We successfully confirmed that Ollama was working independently of our .NET application.

# 7. Step 5 – Create a .NET Console Application

Create a new console application.

```powershell
cd C:\Users\ashok\agents

dotnet new console -n OllamaDemo

cd OllamaDemo
```

Check the project:

```powershell
dotnet build
```

Run the initial application:

```powershell
dotnet run
```

# 8. Step 6 – First Integration Using OllamaSharp

Before learning Semantic Kernel, we explored the direct `IChatClient` abstraction from Microsoft.Extensions.AI.

Install packages:

```powershell
Install-Package Microsoft.Extensions.AI
Install-Package OllamaSharp
```

Example:

```csharp
using Microsoft.Extensions.AI;
using OllamaSharp;

// Connect to local Ollama
IChatClient client = new OllamaApiClient(
    new Uri("http://localhost:11434/"),
    "qwen2.5:0.5b"
);

// Send prompt
var response = await client.GetResponseAsync(
    "What are SOLID principles?"
);

// Display response
Console.WriteLine(response.Text);
Console.ReadKey();
```

### Important concepts

- `IChatClient`: Common abstraction for interacting with chat models.
- `OllamaApiClient`: Connects the application to Ollama.
- `GetResponseAsync()`: Sends the prompt and retrieves a response.
- `response.Text`: Contains the generated answer.

This approach demonstrated that different AI providers can be accessed through common .NET abstractions.

# 9. Step 7 – Introduction to Microsoft Semantic Kernel

Next, we explored Microsoft's Semantic Kernel.

Semantic Kernel is a development framework that helps .NET applications integrate AI models and coordinate AI functionality.

It supports:

- Chat completion
- Prompt templates
- Conversation history
- Plugins
- Function calling
- AI orchestration
- Agent development

### Semantic Kernel architecture

```text
.NET Application
       |
       v
  Semantic Kernel
       |
       +-------------------+
       |                   |
       v                   v
 Chat Completion       Plugins
       |                   |
       v                   v
     LLM              C# Functions
       |
       v
  AI Response
```

### Install Semantic Kernel packages

```powershell
Install-Package Microsoft.SemanticKernel
Install-Package Microsoft.SemanticKernel.Connectors.Ollama -Prerelease
```

Note: The Ollama connector package is experimental. Its API signatures may change between package versions.

# 10. Step 8 – Understand Semantic Kernel Components

### 10.1 Kernel

The Kernel is the central object that coordinates registered AI services and functionality.

```csharp
var builder = Kernel.CreateBuilder();

Kernel kernel = builder.Build();
```

Similar concept in .NET:

The builder pattern and dependency injection service registration.

### 10.2 Chat Completion Service

This abstraction allows Semantic Kernel to communicate with a chat model.

```csharp
var chat =
    kernel.GetRequiredService<IChatCompletionService>();
```

### 10.3 ChatHistory

ChatHistory maintains messages exchanged during a conversation.

```csharp
var history = new ChatHistory();
```

It can store:

- System messages
- User messages
- Assistant responses

### 10.4 System Message

A system message defines the AI assistant's behaviour and instructions.

```csharp
history.AddSystemMessage("""
    You are a helpful AI assistant.
    Explain concepts in simple language.
    """);
```

### 10.5 User Message

```csharp
history.AddUserMessage("What is dependency injection?");
```

### 10.6 Assistant Message

```csharp
history.AddAssistantMessage("Dependency injection is...");
```

### 10.7 GetChatMessageContentAsync

This method sends the conversation history to the chat completion service and retrieves the AI response.

```csharp
var response = await chat.GetChatMessageContentAsync(
    history,
    kernel: kernel
);
```

# 11. Step 9 – Complete Semantic Kernel Implementation

The following implementation connects Semantic Kernel to our local Ollama container.

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0070

// 1. Configure HttpClient
using var httpClient = new HttpClient
{
    BaseAddress = new Uri("http://localhost:11434"),
    Timeout = TimeSpan.FromMinutes(5)
};

// 2. Create Kernel builder
var builder = Kernel.CreateBuilder();

// 3. Register Ollama chat completion
builder.AddOllamaChatCompletion(
    modelId: "qwen2.5:0.5b",
    httpClient: httpClient
);

// 4. Build Kernel
Kernel kernel = builder.Build();

// 5. Get chat completion service
var chat =
    kernel.GetRequiredService<IChatCompletionService>();

// 6. Create conversation history
var history = new ChatHistory();

// 7. Configure AI assistant behaviour
history.AddSystemMessage("""
    You are an intelligent, helpful and versatile AI assistant.

    Your expertise includes:

    1. Technology:
       .NET, C#, Angular, SQL, Azure,
       AI, Machine Learning, RAG and AI Agents.

    2. Current Affairs:
       National and international news,
       economy, science, technology and sports.
       Clearly mention when information may be outdated.
       Never invent current news or statistics.

    3. Competitive Exams:
       General Knowledge, logical reasoning,
       quantitative aptitude and verbal ability.

    4. Mathematics:
       Arithmetic, percentages, ratios,
       profit and loss, averages, algebra,
       probability and step-by-step calculations.

    5. Games and Sports:
       Cricket, football, chess and other games.
       Explain rules, strategies and techniques.

    Response guidelines:
    - Use simple and understandable English.
    - Give accurate and practical answers.
    - Explain complex concepts with examples.
    - Show mathematical calculations step by step.
    - Provide practical code examples when needed.
    - Keep simple answers concise.
    - Acknowledge uncertainty.
    - Never pretend to have real-time internet access.
    """);

// 8. Start continuous conversation
Console.WriteLine("=================================");
Console.WriteLine("       Local AI Assistant");
Console.WriteLine("=================================");
Console.WriteLine("Model: Qwen 2.5 0.5B");
Console.WriteLine("Provider: Ollama");
Console.WriteLine("Type 'exit' to quit.");

while (true)
{
    Console.Write("\nQ: ");

    var userQuestion = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userQuestion))
        continue;

    if (userQuestion.Equals(
        "exit",
        StringComparison.OrdinalIgnoreCase))
        break;

    // Add user question to conversation history
    history.AddUserMessage(userQuestion);

    try
    {
        // Request AI response
        var response = await chat.GetChatMessageContentAsync(
            history,
            kernel: kernel
        );

        // Store assistant response
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
```

The custom `HttpClient` provides a five-minute timeout for slower local inference.

The connector overload used above depends on the installed connector version. If its API signature differs, check the installed package documentation.

# 12. Step 10 – Configure a General-Purpose AI Assistant

Initially, the system prompt was designed only for .NET technical questions.

We then expanded it to support multiple areas:

| Category | Capabilities |
|---|---|
| Technology | .NET, C#, Azure, AI, programming |
| Current Affairs | National and international developments |
| Competitive Exams | GK, reasoning and aptitude |
| Mathematics | Arithmetic and calculations |
| Games | Cricket, football, chess and rules |
| General Knowledge | Everyday learning and explanations |

Important limitation:

A system prompt does not provide live internet access or update a model's training knowledge.

For current news, live scores and recent developments, a web search integration will be required.

# 13. Troubleshooting – Issues Encountered

## Issue 1: Docker engine connection error

Error:

```text
dockerDesktopLinuxEngine:
The system cannot find the file specified.
```

Cause:

Docker Desktop Linux engine was unavailable.

Solution:

Start Docker Desktop and verify its Linux engine.

## Issue 2: Ollama image not found locally

Message:

```text
Unable to find image 'ollama/ollama:latest' locally
```

This was not an error.

Docker automatically downloaded the image from Docker Hub.

## Issue 3: Ollama API returned 404

Log:

```text
POST "/api/chat"
404
```

At that point, model availability had not yet been verified.

We subsequently checked and downloaded the model.

## Issue 4: Semantic Kernel request timeout

Error:

```text
System.Threading.Tasks.TaskCanceledException:

The request was canceled due to the configured
HttpClient.Timeout of 100 seconds elapsing.
```

Diagnosis:

We tested Ollama independently through PowerShell.

The direct API successfully returned an AI-generated response.

We then configured a custom HttpClient with a five-minute timeout for the Semantic Kernel integration.

## Issue 5: CPU-based inference

Docker logs showed:

```text
inference compute: CPU
```

The selected model was therefore kept small to reduce memory requirements.

# 14. Important Concepts Learned Today

| Concept | What I learned |
|---|---|
| LLM | A model that understands and generates language |
| Local LLM | A language model running on my own machine |
| Ollama | Local model runtime and API server |
| Docker | Containerization platform |
| REST API | Communication between applications |
| HttpClient | Sending HTTP requests from .NET |
| Semantic Kernel | AI integration and orchestration framework |
| Kernel | Central coordinator for registered AI services |
| Chat Completion | Generating AI responses |
| ChatHistory | Maintaining conversation context |
| System Prompt | Defining assistant behaviour |
| Embeddings | Numerical representation of text |
| RAG | Retrieving external knowledge for LLM responses |
| AI Agent | AI system that can use tools to accomplish tasks |

# 15. What I Have Achieved

At the end of today's learning session:

- Installed and configured Docker Desktop.
- Started an Ollama container.
- Downloaded a local Qwen 2.5 model.
- Verified Ollama through its REST API.
- Created a .NET Console application.
- Explored OllamaSharp and Microsoft.Extensions.AI.
- Integrated Ollama with Microsoft Semantic Kernel.
- Learned Kernel and ChatHistory.
- Created a general-purpose local AI assistant.
- Explored timeout troubleshooting and local inference performance.

**Main achievement:** Successfully established the foundation for local AI development using .NET and Semantic Kernel without requiring paid cloud model API calls.

# 16. Next Learning Roadmap

The next topics to learn:

| Order | Topic | Practical implementation |
|---|---|---|
| 1 | Prompt Engineering | Improve AI response quality |
| 2 | Semantic Kernel Plugins | Expose C# methods to AI |
| 3 | Function Calling | Let AI invoke application functions |
| 4 | Embeddings | Convert text into vectors |
| 5 | Vector Databases | Store and retrieve embeddings |
| 6 | RAG | Chat with PDF and project documents |
| 7 | AI Agents | Build goal-oriented AI applications |
| 8 | Agent Workflows | Coordinate multiple agents |
| 9 | Azure AI | Deploy and integrate cloud AI services |
| 10 | Production AI | Security, evaluation and monitoring |

# 17. Useful Official References

1. Ollama: https://ollama.com/
2. Ollama Docker: https://hub.docker.com/r/ollama/ollama
3. Ollama Model Library: https://ollama.com/library
4. Microsoft Semantic Kernel: https://learn.microsoft.com/en-us/semantic-kernel/
5. Microsoft.Extensions.AI: https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai
6. Microsoft .NET AI: https://learn.microsoft.com/en-us/dotnet/ai/

---

## Final Learning Reflection

Today I moved beyond simply using AI tools and started understanding how AI models can be integrated into real software applications.

I learned how to run an LLM locally, communicate with it through APIs, integrate it into .NET and use Semantic Kernel to manage AI interactions.

This is the beginning of my journey towards building practical AI-powered applications using my existing .NET and Azure development experience.

**Next milestone: Build my first RAG application and AI Agent using .NET.**