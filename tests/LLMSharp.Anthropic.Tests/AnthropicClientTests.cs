using LLMSharp.Anthropic.Models;
using System.Text;

namespace LLMSharp.Anthropic.Tests
{
    [TestClass]
    public class AnthropicClientTests
    {
        private readonly AnthropicClient _client;

        public AnthropicClientTests()
        {
            this._client = new AnthropicClient();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Create_Anthropic_Client_With_EmptyBaseUrl_Should_Throw_Exception()
        {
            ClientOptions options = new ClientOptions();
            options.BaseUrl = string.Empty;
            AnthropicClient client = new AnthropicClient(options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Create_Anthropic_Client_With_InvalidBaseUrl_Should_Throw_Exception()
        {
            ClientOptions options = new ClientOptions();
            options.BaseUrl = "abc.com";
            AnthropicClient client = new AnthropicClient(options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Create_Anthropic_Client_With_Both_ApiKey_and_AuthToken_Should_Throw_Exception()
        {
            ClientOptions options = new ClientOptions();
            options.ApiKey = "abc.com";
            options.AuthToken = "some bearer token";
            AnthropicClient client = new AnthropicClient(options);
        }

        [TestMethod]
        public async Task When_Used_With_Default_Params_NonStreaming_Call_Should_Succeed()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            var completion = await this._client.GetCompletionsAsync(input);
            Assert.IsNotNull(completion);
            Assert.IsTrue(string.IsNullOrEmpty(completion.Completion) == false);
        }

        [TestMethod]        
        public async Task When_Used_With_Request_Specific_WrongApiKey_NonStreaming_Call_Should_Throw()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
                RequestHeaders = new Dictionary<string, IEnumerable<string>>
                {
                    {"X-Api-Key", new string[]{"some random value"} }
                }
            };

            var exception = await Assert.ThrowsExceptionAsync<AnthropicClientException>(() => this._client.GetCompletionsAsync(input, options)).ConfigureAwait(false);   
            Assert.IsNotNull(exception);
            Assert.IsTrue(exception.HttpStatusCode == System.Net.HttpStatusCode.Unauthorized);
            Assert.IsTrue(exception.Message.Contains("Api Key", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task When_Used_With_Default_Params_RawNonStreaming_Call_Should_Succeed()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            var completion = await this._client.GetRawCompletionsAsync(input);
            Assert.IsNotNull(completion);
            Assert.IsTrue(completion.IsSuccessStatusCode);
        }

        [TestMethod]
        public async Task When_Used_With_Request_Specific_Params_RawNonStreaming_Call_Should_Succeed_Overriding_ClientOptions()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);            
            Assert.IsNotNull(completionResponse);
            Assert.IsTrue(completionResponse.IsSuccessStatusCode);
            Assert.IsTrue(completionResponse.RequestMessage?.Version.Major == 2);
            Assert.IsTrue(completionResponse.RequestMessage.Headers.Contains("X-Api-Key"));
            Assert.IsTrue(completionResponse.RequestMessage.Headers.GetValues("user-agent").FirstOrDefault() == "llmsharp-anthropic-client-sdk");
        }

        [TestMethod]
        public async Task When_Used_With_Request_Specific_WrongApiKey_RawNonStreaming_Call_Should_Fail()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            
            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,   
                RequestHeaders = new Dictionary<string, IEnumerable<string>>
                {
                    {"X-Api-Key", new string[]{"some random value"} }
                }
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);
            Assert.IsNotNull(completionResponse);            
            Assert.IsTrue(completionResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Empty_Prompt_NonStreaming_Call_Should_Fail()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            input.Prompt = string.Empty;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Invalid_Temperature_NonStreaming_Call_Should_Fail()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            input.Temperature = 2;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Invalid_TopP_NonStreaming_Call_Should_Fail()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            input.TopP = 2;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);
        }

        [TestMethod]
        [ExpectedException(typeof(TaskCanceledException))]
        public async Task When_Used_With_VeryShort_Timeout_NonStreaming_Call_Should_Fail()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();            

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
                Timeout = 5 // 5 ms
            };
            var completionResponse = await this._client.GetRawCompletionsAsync(input, options);
        }

        [TestMethod]
        public async Task When_Used_With_Default_Params_Streaming_Call_Should_Succeed()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            var completionStream = await this._client.GetStreamingCompletionsAsync(input);
            Assert.IsNotNull(completionStream);
            StringBuilder chatCompletion = new StringBuilder();
            await foreach (var completion in completionStream)
            {
                chatCompletion.Append(completion?.Completion);
            }

            Assert.IsTrue(chatCompletion.Length > 0);
        }

        [TestMethod]
        public async Task When_Used_With_Default_Params_RawStream_Call_Should_Succeed()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            var completionStream = await this._client.GetStreamingCompletionsAsStreamAsync(input);
            Assert.IsNotNull(completionStream);
            StreamReader reader = new StreamReader(completionStream);
            StringBuilder completion = new StringBuilder();

            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                completion.Append(line);
            }

            Assert.IsTrue(completion.Length > 0);
        }

        [TestMethod]
        public async Task When_Used_With_Request_Specific_WrongApiKey_Streaming_Call_Should_Fail()
        {
            AnthropicCreateStreamingCompletionParams input = new();

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
                RequestHeaders = new Dictionary<string, IEnumerable<string>>
                {
                    {"X-Api-Key", new string[]{"some random value"} }
                }
            };
            var completionResponse = await this._client.GetRawStreamingCompletionsAsync(input, options);
            Assert.IsNotNull(completionResponse);
            Assert.IsTrue(completionResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized);
        }

        [TestMethod]
        public async Task When_Used_With_Default_Params_RawHttpResponseStream_Call_Should_Succeed()
        {
            AnthropicCreateStreamingCompletionParams input = new();            
            var completionStream = await this._client.GetRawStreamingCompletionsAsync(input);
            Assert.IsNotNull(completionStream);
            Assert.IsTrue(completionStream.IsSuccessStatusCode);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Empty_Prompt_Streaming_Call_Should_Fail()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            input.Prompt = string.Empty;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,                
            };
            var completionResponse = await this._client.GetRawStreamingCompletionsAsync(input, options);            
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Invalid_Temperature_Streaming_Call_Should_Fail()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            input.Temperature = 2;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
            };
            var completionResponse = await this._client.GetRawStreamingCompletionsAsync(input, options);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task When_Used_With_Invalid_TopP_Streaming_Call_Should_Fail()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            input.TopP = 2;

            AnthropicRequestOptions options = new AnthropicRequestOptions
            {
                MaxRetries = 0,
            };
            var completionResponse = await this._client.GetRawStreamingCompletionsAsync(input, options);
        }
    }
}