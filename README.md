[![Build and Test Solution](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/build-and-test.yml)
[![Publish Nugets](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/publish-nugets.yml/badge.svg)](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/publish-nugets.yml)
[![CodeQL](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/codeql.yml/badge.svg)](https://github.com/veerashayyagari/anthropic-sdk-dotnet/actions/workflows/codeql.yml)

# anthropic-sdk-dotnet
C# Client SDK (Unofficial) for [Anthropic Large Language Models](https://www.anthropic.com/)
- Inspired by the [official Anthropic Python SDK](https://github.com/anthropics/anthropic-sdk-python).
- Goal is to provide the efficient and flexible SDK for dotnet developers building LLM Apps using Anthropic
- Includes methods for fetching UsageInfo using [AnthropicTokenizer](https://github.com/veerashayyagari/llmsharp-tokenizers).
- SDK uses http/2 for calling Anthropic API endpoints when the client code targets .NET Core 3.0/3.1 or .NET >= 5, for other versions the SDK falls back to using http/1.1


## Install 💽

```
dotnet add package LLMSharp.Anthropic
```

## Usage

### Quickstart 🚀

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

### TLDR 🎯

#### AnthropicClient Methods :

- **GetCompletionsAsync** : Get non streaming completions, returns an AnthropicCompletion object.
- **GetRawCompletionsAsync** : Get non stream completions as raw httpresponse message.   
- **GetCompletionsWithUsageInfoAsync** : Get non streaming completions with token usage info (prompt tokens , completion tokens), returns an AnthropicCompletion object with usage info. 

- **GetStreamingCompletionsAsync** : Get streaming completions, returns an IAsyncEnumerable stream of AnthropicCompletion objects.
- **GetStreamingCompletionsAsStreamAsync** : Get streaming completions as a raw stream, returns a SSE stream.
- **GetRawStreamingCompletionsAsync** : Get streaming completions raw httpresponse message.
- **GetStreamingCompletionsWithUsageInfoAsync** : Get streaming completions with token usage info (prompt tokens , completion tokens), returns an AnthropicCompletion object with usage info. 

#### Input models :
- **AnthropicCreateNonStreamingCompletionParams** : is used for passing input parameters for non streaming completion methods.
- **AnthropicCreateNonStreamingCompletionParams** : is used for passing input parameters for streaming completion methods.
- **ClientOptions** : Provides various options for configuring AnthropicClient instance
- **AnthropicRequestOptions** : Provides options for overriding AnthropicClient configuration with request specific configuration

### Advanced Usage 📋

- I want to control additional attributes like 'Temperature', 'TopP' and 'TopK' => customize using 
**AnthropicCreateStreamingCompletionParams** and **AnthropicCreateNonStreamingCompletionParams** for Streaming and non streaming completions respectively

```csharp
var userQuestion = "Why is the sky blue?";
var prompt = $"{Constants.HUMAN_PROMPT}{userQuestion}{Constants.AI_PROMPT}";

AnthropicCreateStreamingCompletionParams reqParams = 
    new AnthropicCreateStreamingCompletionParams
    {
        /*
        * The maximum number of tokens to generate before stopping.
        * Default : 2048
        */
        MaxTokens = 2048,
        /*
        * The model that will complete your prompt.
        * Possible Options: claude-2 , claude-instant-1, your custom model name
        * Default: "claude-2"
        */
        Model = "claude-2",
        /*
        * The prompt that you want Claude to complete.
        * Default : "\n\nHuman:Hello,Claude. \n\nAssistant:"
        * See [comments on prompts](https://docs.anthropic.com/claude/docs/introduction-to-prompt-design)
        */
        Prompt = prompt,
        /*
        * Sequences that will cause the model to stop generating completion text.
        * Anthropic models stop on `"\n\nHuman:"`, and may include additional built-in stop 
        sequences in the future. By providing the stop_sequences parameter, you may include 
        additional strings that will cause the model to stop generating.
        */
        StopSequences = new string[]{""},
        /*
        * Amount of randomness injected into the response.
        * Defaults to 1.
        * Ranges from 0 to 1. Use temp closer to 0 for analytical / multiple choice,
        * and closer to 1 for creative and generative tasks.
        */
        Temperature = 1,
        /*
        * Use nucleus sampling.In nucleus sampling, we compute the cumulative distribution 
        * over all the options for each subsequent token in decreasing probability order 
        * and cut it off once it reaches a particular probability specified by `TopP`. You 
        * should either alter Temperature or TopP , but not both
        */       
        TopP = null,
        /*
        * Only sample from the top K options for each subsequent token.
        * Used to remove "long tail" low probability responses.
        * Default 'null'
        */
        TopK = null,

    };

var completions = await client.GetStreamingCompletionsAsync(reqParams);

// Use 'AnthropicCreateNonStreamingCompletionParams' and GetCompletionsAsync if looking for NonStreaming completions

```

- I want the raw http response that the anthropic api returned.
    - Non streaming completions get httpresponsemessage => use **GetRawCompletionsAsync**
    ```csharp
    using LLMSharp.Anthropic;
    HttpResponseMessage message = await client.GetRawCompletionsAsync(options);
    ```
    - Streaming completions get httpresponsemessage => use **GetRawStreamingCompletionsAsync**
    ```csharp
    using LLMSharp.Anthropic;
    HttpResponseMessage message = await client.GetRawStreamingCompletionsAsync(options);
    ```
    - Streaming completions get raw stream => use **GetStreamingCompletionsAsStreamAsync**
    ```csharp
    using LLMSharp.Anthropic;
    Stream stream = await client.GetStreamingCompletionsAsStreamAsync(options);
    ```

- I want more control when creating AnthropicClient => customize **ClientOptions**

```csharp
using LLMSharp.Anthropic;

ClientOptions options = new ClientOptions
{
    ApiKey = "<apikey>",
    BaseUrl = "a different baseurl other than default",
    DefaultHeaders = new Dictionary<string, IEnumerable<string>>
    {
        "header1", new string[]{"val1", "val2"}
    },
    MaxRetries = 3, // max retries
    Timeout = 500 // 500 ms
};

AnthropicClient client = new AnthropicClient(options);
```

|  ClientOptions Property   |  Default                               |  Description                                        |
|-------------------------  |----------------------------------------|----------------------------------------------------:|
|      ApiKey               | ANTHROPIC_API_KEY env variable         | Obtain API Key from 'https://console.anthropic.com/'|
|      BaseUrl              | "https://api.anthropic.com"            | BaseUrl for Anthropic API endpoint                  |
|      DefaultHeaders       | null                                   | Default headers to include in every Request         |
|      AuthToken            | ANTHROPIC_AUTH_TOKEN env variable      | Bearer AuthToken used as 'Authorization' Header. Use ApiKey or AuthToken but not both     |
|      TimeOut              | 10 minutes (600000 ms)                 | The maximum amount of time (in milliseconds) that the client should wait for a response                  |
|      MaxRetries           | 2                                      | The maximum number of times that the client will retry a request in case of a temporary failure, like a network error or a 5XX error from the server.                  |

- I want to override 'MaxRetries' and 'Timeout' on a per request basis , instead of those configured using ClientOptions for the AnthropicClient => Use **AnthropicRequestOptions**

```csharp
using LLMSharp.Anthropic;
using LLMSharp.Anthropic.Models;

var userQuestion = "Why is the sky blue?";
var prompt = $"{Constants.HUMAN_PROMPT}{userQuestion}{Constants.AI_PROMPT}";

AnthropicCreateStreamingCompletionParams reqParams = new AnthropicCreateStreamingCompletionParams{Prompt = prompt};

AnthropicRequestOptions reqOptions = new AnthropicRequestOptions
{
    MaxRetries = 5, // overrides max number of retries for the request to 5
    Timeout = 500 // overrides timeout per retry for this request to 500 ms
};

var completions = await client.GetStreamingCompletionsAsync(reqParams, reqOptions);

```

- I want to override 'Anthropic Api Key' or 'Auth Token' on a per request basis => Use 'RequestHeaders' properties of **AnthropicRequestOptions** to override.

```csharp
using LLMSharp.Anthropic;
using LLMSharp.Anthropic.Models;

var userQuestion = "Why is the sky blue?";
var prompt = $"{Constants.HUMAN_PROMPT}{userQuestion}{Constants.AI_PROMPT}";

AnthropicCreateStreamingCompletionParams reqParams = new AnthropicCreateStreamingCompletionParams{Prompt = prompt};

// override 'x-api-key' request header for this request
AnthropicRequestOptions reqOptions = new AnthropicRequestOptions
{
    RequestHeaders = new Dictionary<string, IEnumerable<string>>
    {
        "X-API-KEY", new string[]{"your new api key"}
    }
};

var completions = await client.GetStreamingCompletionsAsync(reqParams, reqOptions);
```

Use 'Authorization' header if needs to override AuthToken

```csharp
using LLMSharp.Anthropic;
using LLMSharp.Anthropic.Models;

var userQuestion = "Why is the sky blue?";
var prompt = $"{Constants.HUMAN_PROMPT}{userQuestion}{Constants.AI_PROMPT}";

AnthropicCreateStreamingCompletionParams reqParams = new AnthropicCreateStreamingCompletionParams{Prompt = prompt};

// override 'x-api-key' request header for this request
AnthropicRequestOptions reqOptions = new AnthropicRequestOptions
{
    RequestHeaders = new Dictionary<string, IEnumerable<string>>
    {
        "Authorization", new string[]{"Bearer <your new auth token>"}
    }
};

var completions = await client.GetStreamingCompletionsAsync(reqParams, reqOptions);
```

## Contribute 🤝

Got a pull request? Open it, and I'll review it as soon as possible.