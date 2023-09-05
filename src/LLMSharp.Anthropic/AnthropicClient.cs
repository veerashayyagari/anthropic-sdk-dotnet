using LLMSharp.Anthropic.Models;
using LLMSharp.Anthropic.Tokenizer;
using LLMSharp.Anthropic.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
        /// If logger is null, fallback to default 'ConsoleLogger' implementation
        /// </summary>
        /// <param name="options" cref="ClientOptions">options for customizing anthropic client behavior</param>
        /// <param name="logger">AnthropicClient logger implementation, Default: ConsoleLogger </param>
        public AnthropicClient(ClientOptions options, ILogger<AnthropicClient>? logger)
        {
            this._logger = logger ?? LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<AnthropicClient>();
            this._httpClient = BuildClientFromOptions(options);
            this._defaultRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(options.MaxRetries, retryAttempt =>
                {
                    this._logger.Warn($"Retrying: ${retryAttempt}", null);
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                });
            this._logger.Info("Successfully set up Anthropic HttpClient instance");
        }

        /// <summary>
        /// Get non-streaming AnthropicCompletion object from Anthropic Completions API
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

            if (!response.IsSuccessStatusCode)
            {
                await response.ProcessAsAnthropicClientException();
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonSerializer.Deserialize<AnthropicCompletion>(responseBody);
        }

        /// <summary>
        /// Get non-streaming AnthropicCompletion object including token usage from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateNonStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion object with prompt response</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        public async Task<AnthropicCompletion?> GetCompletionsWithUsageInfoAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                await response.ProcessAsAnthropicClientException();
            }

            var claudeTokenizer = new ClaudeTokenizer();
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var completion = JsonSerializer.Deserialize<AnthropicCompletion>(responseBody);
            if (completion != null)
            {
                completion.UsageInfo = new AnthropicTokenUsage
                {
                    PromptTokens = claudeTokenizer.CountTokens(requestParams.Prompt),
                    CompletionTokens = claudeTokenizer.CountTokens(completion?.Completion),
                };
            }

            return completion;
        }

        /// <summary>
        /// Get non-streaming raw httpresponse from Anthropic Completions API
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

        /// <summary>
        /// Get Streaming AnthropicCompletion object from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>Stream of AnthropicCompletion objects</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        public async Task<IAsyncEnumerable<AnthropicCompletion?>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await response.ProcessAsAnthropicClientException();
            }

            Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return contentStream.ReadCompletionsFromSseStream();
        }

        /// <summary>
        /// Get Streaming AnthropicCompletion object with token usage info from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>Stream of AnthropicCompletion objects</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        public async IAsyncEnumerable<AnthropicCompletion?> GetStreamingCompletionsWithUsageInfoAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await response.ProcessAsAnthropicClientException();
            }

            Stream contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var completions = contentStream.ReadCompletionsFromSseStream();
            var claudeTokenizer = new ClaudeTokenizer();
            int promptTokens = claudeTokenizer.CountTokens(requestParams.Prompt);
            await foreach (var completion in completions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!string.IsNullOrEmpty(completion?.Completion))
                {
                    int completionTokens = claudeTokenizer.CountTokens(completion!.Completion);
                    completion.UsageInfo = new AnthropicTokenUsage { PromptTokens = promptTokens, CompletionTokens = completionTokens };
                }

                yield return completion;
            }
        }

        /// <summary>
        /// Get the SSE stream from Anthorpic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>Server Sent Event Stream</returns>
        /// <exception cref="AnthropicClientException">Gets thrown on non success response code.</exception>
        public async Task<Stream> GetStreamingCompletionsAsStreamAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var response = await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await response.ProcessAsAnthropicClientException();
            }

            return await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get streaming raw httpresponse from Anthropic Completions API
        /// </summary>
        /// <param name="requestParams" cref="AnthropicCreateStreamingCompletionParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions" cref="AnthropicRequestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>AnthropicCompletion HttpResponse with anthropic response payload and headers</returns>
        public async Task<HttpResponseMessage> GetRawStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            return await this.GetRawChatCompletionsResponse(requestParams, requestOptions, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Calls Anthropic Completions API endpoint and returns the HttpResponse
        /// Code will take care of overriding values of timeout, retryPolicy and/or headers on a per request basis
        /// </summary>
        /// <typeparam name="T">type is either AnthropicCreateNonStreamingCompletionParams or AnthropicCreateStreamingCompletionParams</typeparam>
        /// <param name="requestParams">Input parameters like prompt, temperature for generating completions</param>
        /// <param name="requestOptions">Request specific overrides for ClientOptions</param>
        /// <param name="cancellationToken">Request cancellation token</param>
        /// <returns>HttpResponseMessage from the AnthropicCompletions API endpoint</returns>
        private async Task<HttpResponseMessage> GetRawChatCompletionsResponse<T>(T requestParams, AnthropicRequestOptions? requestOptions, CancellationToken cancellationToken) where T : AnthropicCreateCompletionBaseParams
        {
            ValidateCompletionParams(requestParams);

            this._logger.Info("Validating Completion Parameters successful");

            if (string.IsNullOrEmpty(requestParams.Metadata?.UserId))
            {
                requestParams.Metadata = new AnthropicCreateCompletionMetadata { UserId = Guid.NewGuid().ToString() };
            }

            IAsyncPolicy<HttpResponseMessage> retryPolicy = _defaultRetryPolicy;
            if (requestOptions?.MaxRetries.HasValue == true)
            {
                retryPolicy = HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(requestOptions.MaxRetries.Value, retryAttempt =>
                    {
                        this._logger.Warn($"Retrying: ${retryAttempt}", null);
                        return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    });
            }

            CancellationToken requestCancellationToken = cancellationToken;
            if (requestOptions?.Timeout.HasValue == true)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(requestOptions.Timeout.Value);
                requestCancellationToken = cts.Token;
            }

            HttpRequestMessage message = new() { Content = requestParams.ToStringContent(), Method = HttpMethod.Post, RequestUri = new Uri(Constants.COMPLETIONS_ENDPOINT, UriKind.Relative) };
            if (IsHttp2SupportAvailable())
            {
                _logger.Info("Upgrading http request to use http/2.");
                message.Version = new Version(2, 0);
            }

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

            this._logger.Info($"Making Request to Anthropic API endpoint. Request will timeout after {requestOptions?.Timeout ?? _httpClient.Timeout.Milliseconds} milliseconds");
            var response = await retryPolicy.ExecuteAsync(
                () => (requestParams is AnthropicCreateNonStreamingCompletionParams) ?
                _httpClient.SendAsync(message, requestCancellationToken) :
                _httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, requestCancellationToken))
            .ConfigureAwait(false);
            return response;
        }

        /// <summary>
        /// Validate Completion Input params and throws an error if any parameters are invalid
        /// </summary>
        /// <param name="completionParams">Completion input parameters to validate</param>
        /// <exception cref="ArgumentNullException">Gets thrown if completionParams property is null</exception>
        /// <exception cref="ArgumentException">Gets thrown if prompt is empty/null, temperature and/or topP are not with in the valid range</exception>
        private static void ValidateCompletionParams(AnthropicCreateCompletionBaseParams completionParams)
        {
            if (completionParams == null)
            {
                throw new ArgumentNullException(nameof(completionParams));
            }

            if (string.IsNullOrEmpty(completionParams.Prompt))
            {
                throw new ArgumentException($"{nameof(completionParams.Prompt)} is null");
            }

            if (completionParams.Temperature < 0 || completionParams.Temperature > 1)
            {
                throw new ArgumentException($"{completionParams.Temperature}: Is invalid value for Temperature. Should be between 0 and 1.");
            }

            if (completionParams.TopP.HasValue && (completionParams.TopP.Value < 0 || completionParams.TopP.Value > 1))
            {
                throw new ArgumentException($"{completionParams.TopP}: Is invalid value for TopP. Should be between 0 and 1.");
            }
        }

        /// <summary>
        /// Create an httpclient using the provided clientOptions
        /// </summary>
        /// <param name="options">ClientOptions used to configure HttpClient</param>
        /// <returns>HttpClient for calling Anthropic Completions RestAPI</returns>
        /// <exception cref="ArgumentNullException">Gets thrown if clientoptions parameter is null</exception>
        /// <exception cref="ArgumentException">Gets thrown if either BaseUrl is null/invalid or both ApiKey and AuthToken values are present</exception>
        private static HttpClient BuildClientFromOptions(ClientOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (string.IsNullOrEmpty(options.BaseUrl))
            {
                throw new ArgumentException($"{nameof(options.BaseUrl)} is null or empty");
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
                    Timeout = TimeSpan.FromMilliseconds(options.Timeout),
                };

                return client
                    .AddDefaultHeaders()
                    .AddAuthHeaders(options)
                    .OverrideDefaultHeaders(options.DefaultHeaders);
            }

            throw new ArgumentException($"{options.BaseUrl}: is not a valid Uri");
        }

        /// <summary>
        /// Checks whether the current runtime framework supports http/2 with the built in httpclient
        /// </summary>
        /// <returns>true if the runtime framework is .net core 3 and above or .net 5 and above</returns>
        private bool IsHttp2SupportAvailable()
        {
            _logger.Info($"Current Runtime Framework: {RuntimeInformation.FrameworkDescription}");
            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.InvariantCultureIgnoreCase)) return false;
            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Native", StringComparison.InvariantCultureIgnoreCase)) return false;
            if (RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.InvariantCultureIgnoreCase))
            {
                var versionString = RuntimeInformation.FrameworkDescription.Split(' ')[2];
                Version version = new Version(versionString);
                return version.Major >= 3;
            }

            return true;
        }
    }
}
