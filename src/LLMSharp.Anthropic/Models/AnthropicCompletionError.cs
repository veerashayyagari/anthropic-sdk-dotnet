using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    /// <summary>
    /// Class representing the error payload from Anthropic API
    /// </summary>
    public class AnthropicCompletionErrorPayload
    {
        /// <summary>
        /// Type of the Anthropic API error
        /// </summary>
        [JsonPropertyName("type")]
        public string? ErrorType { get; set; }

        /// <summary>
        /// Error message for the current API failure
        /// </summary>
        [JsonPropertyName("message")]
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// Class representing the error response from Anthropic API
    /// </summary>
    public class AnthropicCompletionError
    {
        /// <summary>
        /// Error payload
        /// </summary>
        [JsonPropertyName("error")]
        public AnthropicCompletionErrorPayload? Error { get; set; }
    }
}
