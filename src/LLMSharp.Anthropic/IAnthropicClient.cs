using LLMSharp.Anthropic.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic
{
    public interface IAnthropicClient
    {
        Task<AnthropicCompletion> GetCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        Task<IAsyncEnumerable<AnthropicCompletion>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);
    }
}
