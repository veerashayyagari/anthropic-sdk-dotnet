using LLMSharp.Anthropic.Models;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic
{
    /// <summary>
    /// Contract for implementing AnthropicClient instance
    /// </summary>
    public interface IAnthropicClient
    {
        /// <summary>
        /// Get non-streaming AnthropicCompletion object from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateNonStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion object with prompt response</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        Task<AnthropicCompletion?> GetCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get non-streaming raw httpresponse from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateNonStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion HttpResponse with anthropic response payload and headers</returns>
        Task<HttpResponseMessage> GetRawCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get Streaming AnthropicCompletion object from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>Stream of AnthropicCompletion objects</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        Task<IAsyncEnumerable<AnthropicCompletion?>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the raw SSE stream from Anthorpic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>Server Sent Event Stream</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        Task<Stream> GetRawStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default);
    }
}
