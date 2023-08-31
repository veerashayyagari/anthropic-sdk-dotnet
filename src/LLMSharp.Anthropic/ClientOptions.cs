using System.Net.Http.Headers;

namespace LLMSharp.Anthropic
{
    /// <summary>
    /// Options for building/customizing Anthropic Client
    /// </summary>
    public class ClientOptions
    {
        /// <summary>
        /// Anthropic Api Key. Set either this property or AuthToken, not both
        /// 
        /// Default: "ANTHROPIC_API_KEY" environment variable.
        /// Set it to null if you want to send unauthenticated requests.
        /// </summary>
        public string? ApiKey { get; set; } = System.Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");

        /// <summary>
        /// Override the default base URL for the API, e.g., "https://api.example.com/v2/"
        /// Default : https://api.anthropic.com
        /// </summary>
        public string BaseUrl { get; set; } = "https://api.anthropic.com";

        /// <summary>
        /// The maximum amount of time (in milliseconds) that the client should wait for a response
        /// from the server before timing out a single request.
        /// 
        /// Default : 10 minutes
        /// 
        /// Note that the request timeouts are retried by default, so in a worst-case scenario
        /// you may wait much longer than this timeout before the promise succeeds or fails.
        /// </summary>
        public int Timeout { get; set; } = 600000;

        /// <summary>
        /// The maximum number of times that the client will retry a request in case of a
        /// temporary failure, like a network error or a 5XX error from the server.
        /// 
        /// Default : 2
        /// </summary>
        public int MaxRetries { get; set; } = 2;

        /// <summary>
        /// Default headers to include with every request to the API.
        /// 
        /// These can be removed in individual requests by explicitly setting the
        /// header to 'null' in RequestOptions.
        /// </summary>
        public HttpHeaders? DefaultHeaders { get; set; }

        /// <summary>
        /// Authentication Bearer Token
        /// Set either ApiKey or AuthToken property but not both
        /// Default : ANTHROPIC_AUTH_TOKEN environment variable
        /// </summary>
        public string? AuthToken { get; set; } = System.Environment.GetEnvironmentVariable("ANTHROPIC_AUTH_TOKEN");
    }
}
