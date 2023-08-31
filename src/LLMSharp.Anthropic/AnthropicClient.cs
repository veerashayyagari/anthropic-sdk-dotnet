﻿using LLMSharp.Anthropic.Models;
using LLMSharp.Anthropic.Utils;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic
{
    public class AnthropicClient : IAnthropicClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnthropicClient> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _defaultRetryPolicy;

        public AnthropicClient(ClientOptions options) : this(options, null) { }

        public AnthropicClient(ClientOptions options, ILogger<AnthropicClient>? logger)
        {
            this._logger = logger ?? LoggerFactory.Create(logging => logging.AddConsole()).CreateLogger<AnthropicClient>();
            this._httpClient = BuildClientFromOptions(options);
            this._defaultRetryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(options.MaxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public async Task<AnthropicCompletion?> GetCompletionsAsync(
            AnthropicCreateNonStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default)
        {
            ValidateCompletionParams(requestParams);
            
            HttpRequestMessage message = new() { Content = requestParams.ToStringContent(), Method = HttpMethod.Post, RequestUri = new Uri(Constants.COMPLETIONS_ENDPOINT, UriKind.Relative)};            
            var response = await this._defaultRetryPolicy.ExecuteAsync(
                () => _httpClient.SendAsync(message, cancellationToken)).ConfigureAwait(false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            if(response.StatusCode != HttpStatusCode.OK)
            {
                throw new AnthropicClientException(response.StatusCode, response.Headers, responseBody);
            }
            
            return JsonSerializer.Deserialize<AnthropicCompletion>(responseBody);            
        }

        public Task<IAsyncEnumerable<AnthropicCompletion>> GetStreamingCompletionsAsync(
            AnthropicCreateStreamingCompletionParams requestParams,
            AnthropicRequestOptions? requestOptions,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private void ValidateCompletionParams(AnthropicCreateCompletionBaseParams completionParams)
        {
            if(string.IsNullOrEmpty(completionParams.Prompt))
            {
                throw new ArgumentNullException(nameof(completionParams.Prompt));
            }

            if(completionParams.Temperature < 0 || completionParams.Temperature > 0)
            {
                throw new ArgumentException($"{completionParams.Temperature}: Is invalid value for Temperature. Should be between 0 and 1.", nameof(completionParams.Temperature));
            }

            if(completionParams.TopP.HasValue && (completionParams.TopP.Value < 0 || completionParams.TopP.Value > 1))
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
