using System.Net.Http;
using System.Net.Http.Headers;

namespace LLMSharp.Anthropic.Utils
{
    internal static class HttpClientExtensions
    {
        internal static HttpClient AddDefaultHeaders(this HttpClient client)
        {
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("Content-Type", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", Constants.LLMSHARP_USER_AGENT);
            client.DefaultRequestHeaders.Add("anthropic-version", Constants.ANTHROPIC_API_VERSION);
            return client;
        }

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

        internal static HttpClient OverrideDefaultHeaders(this HttpClient client, HttpHeaders? headers)
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
