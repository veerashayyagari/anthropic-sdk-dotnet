using System.Collections.Generic;

namespace LLMSharp.Anthropic.Models
{
    /// <summary>
    /// Request specific options that override any client specific options
    /// </summary>
    public class AnthropicRequestOptions
    {
        /// <summary>
        /// Any custom headers specific to the request,
        /// Request headers will take precedence over DefaultHeaders in ClientOptions
        /// </summary>
        public IDictionary<string, IEnumerable<string>>? RequestHeaders { get; set; }

        /// <summary>
        /// The maximum number of times that the client will retry a request in case of a
        /// temporary failure, like a network error or a 5XX error from the server.
        /// 
        /// Default : MaxRetries set in ClientOptions (default 2)
        /// </summary>
        public int? MaxRetries { get; set; }

        /// <summary>
        /// The maximum amount of time (in milliseconds) that the client should wait for a response
        /// from the server before timing out a single request.
        /// 
        /// Default : Timeout set in ClientOptions (default 10 mins)
        /// 
        /// This will override any default timeout set in ClientOptions
        /// 
        /// Note that the request timeouts are retried by default, so in a worst-case scenario
        /// you may wait much longer than this timeout before the call succeeds or fails.
        /// </summary>
        public int? Timeout { get; set; }
    }
}
