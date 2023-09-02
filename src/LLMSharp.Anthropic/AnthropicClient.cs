using LLMSharp.Anthropic.Models;
using LLMSharp.Anthropic.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic
{
    /// <summary>
    /// Anthropic Completions Rest API client implementation
    /// </summary>
    public class AnthropicClient : IAnthropicClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicClient> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _defaultRetryPolicy;

        /// <summary>
        /// Default constructor, with all the default client options
        /// </summary>
        public AnthropicClient() : this(new ClientOptions()) { }

        /// <summary>
        /// Constructor with custom client options
        /// </summary>
        /// <param name="options" cref="ClientOptions">options for customizing anthropic client behavior</param>
        public AnthropicClient(ClientOptions options) : this(options, null) { }

        /// <summary>
        /// Constructor with custom client options and custom logger implementation
        /// If logger is null, fallsback to default 'ConsoleLogger' implementation
        /// </summary>
        /// <param name="options" cref="ClientOptions">options for customizing anthropic client behavior</param>
        /// <param name="logger">AnthropicClient logger implementation, Default: ConsoleLogger </param>
        public AnthropicClient(ClientOptions options, ILogger<AnthropicClient>? logger)
        {
            this._logger = logger ?? LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<AnthropicClient>();
            this._httpClient = BuildClientFromOptions(options);
            this._defaultRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(options.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// Get non-streaming completions from Anthropic
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateNonStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion object with prompt response</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        public async Task<AnthropicCompletion?> GetCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                throw new AnthropicClientException(response.StatusCode, response.Headers, responseBody);
            }

            return JsonSerializer.Deserialize<AnthropicCompletion>(responseBody);
        }

        /// <summary>
        /// Get non-streaming raw httpresponse from Anthropic
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateNonStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion HttpResponse with anthropic response payload and headers</returns>
        public async Task<HttpResponseMessage> GetRawCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IAsyncEnumerable<AnthropicCompletion?>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new AnthropicClientException(response.StatusCode, response.Headers, responseBody);
            }

            Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return contentStream.ReadCompletionsFromSseStream();
        }

        public async Task<Stream> GetRawStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new AnthropicClientException(response.StatusCode, response.Headers, responseBody);
            }

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> GetRawChatCompletionsResponse<T>(T requestParams, AnthropicRequestOptions? requestOptions, CancellationToken cancellationToken) where T : AnthropicCreateCompletionBaseParams
        {
            ValidateCompletionParams(requestParams);

            IAsyncPolicy<HttpResponseMessage> retryPolicy = _defaultRetryPolicy;
            if (requestOptions?.MaxRetries.HasValue == true)
            {
                retryPolicy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(requestOptions.MaxRetries.Value, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            }

            CancellationToken requestCancellationToken = cancellationToken;
            if (requestOptions?.Timeout.HasValue == true)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(requestOptions.Timeout.Value);
                requestCancellationToken = cts.Token;
            }

            HttpRequestMessage message = new() { Content = requestParams.ToStringContent(), Method = HttpMethod.Post, RequestUri = new Uri(Constants.COMPLETIONS_ENDPOINT, UriKind.Relative) };

            if (requestOptions?.RequestHeaders != null)
            {
                foreach (var header in requestOptions.RequestHeaders)
                {
                    if (message.Headers.Contains(header.Key))
                    {
                        message.Headers.Remove(header.Key);
                    }

                    message.Headers.Add(header.Key, header.Value);
                }
            }

            var response = await retryPolicy.ExecuteAsync(
                () => (requestParams is AnthropicCreateNonStreamingCompletionParams) ?
                _httpClient.SendAsync(message, requestCancellationToken) :
                _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, requestCancellationToken))
            .ConfigureAwait(false);
            return response;
        }

        private void ValidateCompletionParams(AnthropicCreateCompletionBaseParams completionParams)
        {
            if (string.IsNullOrEmpty(completionParams.Prompt))
            {
                throw new ArgumentNullException(nameof(completionParams.Prompt));
            }

            if (completionParams.Temperature < 0 || completionParams.Temperature > 1)
            {
                throw new ArgumentException($"{completionParams.Temperature}: Is invalid value for Temperature. Should be between 0 and 1.", nameof(completionParams.Temperature));
            }

            if (completionParams.TopP.HasValue && (completionParams.TopP.Value < 0 || completionParams.TopP.Value > 1))
            {
                throw new ArgumentException($"{completionParams.TopP}: Is invalid value for TopP. Should be between 0 and 1.", nameof(completionParams.TopP));
            }
        }

        private HttpClient BuildClientFromOptions(ClientOptions options)
        {
            if (string.IsNullOrEmpty(options.BaseUrl))
            {
                throw new ArgumentNullException(nameof(options.BaseUrl));
            }

            if (!string.IsNullOrEmpty(options.ApiKey) && !string.IsNullOrEmpty(options.AuthToken))
            {
                throw new ArgumentException($"Only one of ApiKey or AuthToken is expected. Found both");
            }

            if (Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out Uri baseUri))
            {
                var client = new HttpClient
                {
                    BaseAddress = baseUri,
                    Timeout = TimeSpan.FromMilliseconds(options.Timeout)
                };

                return client
                    .AddDefaultHeaders()
                    .AddAuthHeaders(options)
                    .OverrideDefaultHeaders(options.DefaultHeaders);
            }

            throw new ArgumentException($"{options.BaseUrl}: is not a valid Uri", nameof(options.BaseUrl));
        }
    }
}
