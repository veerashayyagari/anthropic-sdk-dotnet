using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;

namespace LLMSharp.Anthropic.Utils
{
    internal static class HttpClientExtensions
    {
        /// <summary>
        /// Adds default headers like 'accept', 'user-agent' and 'anthropic-version'
        /// to the httpclient instance
        /// </summary>
        /// <param name="client">httpclient instance</param>
        /// <returns>httpclient instance with default headers configured</returns>
        internal static HttpClient AddDefaultHeaders(this HttpClient client)
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");            
            client.DefaultRequestHeaders.Add("User-Agent", Constants.LLMSHARP_USER_AGENT);
            client.DefaultRequestHeaders.Add("anthropic-version", Constants.ANTHROPIC_API_VERSION);
            return client;
        }

        /// <summary>
        /// Adds auth headers to the httpclient instance
        /// if 'ApiKey' is provided, 'X-Api-Key' header is configured with the apikey
        /// if 'AuthToken' is provided, 'Authorization' header is configured with the Bearer Token
        /// </summary>
        /// <param name="client">httpclient instance</param>
        /// <param name="options">clientOptions to configure httpclient</param>
        /// <returns>httpclient with authheaders configured from values in clientOptions</returns>
        internal static HttpClient AddAuthHeaders(this HttpClient client, ClientOptions options)
        {
            if(!string.IsNullOrEmpty(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);                
            }

            if(!string.IsNullOrEmpty(options.AuthToken))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.AuthToken}");                
            }

            return client;
        }

        /// <summary>
        /// Utility method that overrides any existing headers on the httpclient with provided headers
        /// </summary>
        /// <param name="client">httpclient to be configured</param>
        /// <param name="headers">custom headers to be used for overriding</param>
        /// <returns>httpclient configured with custom header overrides</returns>
        internal static HttpClient OverrideDefaultHeaders(this HttpClient client, IDictionary<string, IEnumerable<string>>? headers)
        {
            if(headers == null) return client;

            foreach(var header in headers)
            {
                if(client.DefaultRequestHeaders.Contains(header.Key))
                {
                    client.DefaultRequestHeaders.Remove(header.Key);
                }

                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }

            return client;
        }
    }
}
