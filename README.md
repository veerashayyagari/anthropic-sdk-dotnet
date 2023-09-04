# anthropic-sdk-dotnet
C# Client SDK (Unofficial) for [Anthropic Large Language Models](https://www.anthropic.com/)

## Install

```
dotnet add package LLMSharp.Anthropic
```

## Usage

### Quickstart

- Create ClientOptions object with Anthropic API key

```csharp
using LLMSharp.Anthropic;

// if you want to explicitly pass the api key
ClientOptions options = new ClientOptions { ApiKey = <your anthropic api key> };
```

OR

```csharp
using LLMSharp.Anthropic;

// set API key as an environment variable with key 'ANTHROPIC_API_KEY'
ClientOptions options = new ClientOptions();
```

- Create an instance of AnthropicClient and pass in the client Options.

```csharp
using LLMSharp.Anthropic;

AnthropicClient client = new AnthropicClient(options);
```

- Get non streaming chat completions for a prompt

```csharp
using LLMSharp.Anthropic;
using LLMSharp.Anthropic.Models;

string prompt = $"{Constants.HUMAN_PROMPT}<your custom prompt will go here>{Constants.AI_PROMPT}";
AnthropicCreateNonStreamingCompletionParams reqParams = new AnthropicCreateNonStreamingCompletionParams { Prompt = prompt };
AnthropicCompletion completion = await client.GetCompletionsAsync(reqParams);
Console.WriteLine(completion.Completion);
```

- Get streaming chat completions for a prompt

```csharp
using LLMSharp.Anthropic;
using LLMSharp.Anthropic.Models;

string prompt = $"{Constants.HUMAN_PROMPT}<your custom prompt will go here>{Constants.AI_PROMPT}";
AnthropicCreateStreamingCompletionParams reqParams = new AnthropicCreateStreamingCompletionParams { Prompt = prompt };
IAsyncEnuemrable<AnthropicCompletion> completions = await client.GetStreamingCompletionsAsync(reqParams);

await using(AnthropicCompletion completion in completions)
{
    Console.WriteLine(completion.Completion);
}

```