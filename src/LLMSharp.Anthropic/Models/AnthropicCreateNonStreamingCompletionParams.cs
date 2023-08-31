using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    public class AnthropicCreateNonStreamingCompletionParams: AnthropicCreateCompletionBaseParams
    {
        /// <summary>
        /// Whether to incrementally stream the response using server-sent events.
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; private set; } = false;
    }
}
