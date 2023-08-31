using LLMSharp.Anthropic.Models;
using System.Runtime.CompilerServices;
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
        public async Task When_Used_With_Default_Params_NonStreaming_Call_Should_Succeed()
        {
            AnthropicCreateNonStreamingCompletionParams input = new();
            var completion = await this._client.GetCompletionsAsync(input);
            Assert.IsNotNull(completion);
            Assert.IsTrue(string.IsNullOrEmpty(completion.Completion) == false);
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
        public async Task When_Used_With_Default_Params_RawStreaming_Call_Should_Succeed()
        {
            AnthropicCreateStreamingCompletionParams input = new();
            var completionStream = await this._client.GetRawStreamingCompletionsAsync(input);
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
    }
}