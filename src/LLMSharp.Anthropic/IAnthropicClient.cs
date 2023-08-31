using LLMSharp.Anthropic.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic
{
    public interface IAnthropicClient
    {
        Task<AnthropicCompletion?> GetCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        Task<HttpResponseMessage> GetRawCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        Task<IAsyncEnumerable<AnthropicCompletion?>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        Task<Stream> GetRawStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default);
    }
}
