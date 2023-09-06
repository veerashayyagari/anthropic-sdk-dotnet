using LLMSharp.Anthropic.Utils;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    /// <summary>
    /// AnthropicCompletion object returned by both Non-Streaming and Streaming Completions
    /// </summary>
    public class AnthropicCompletion
    {
        /// <summary>
        /// The resulting completion up to and excluding the stop sequences.
        /// </summary>
        [JsonPropertyName("completion")]
        public string? Completion { get; set; }

        /// <summary>
        /// The model that performed the completion
        /// </summary>        
        public string? Model { get; set; }

        /// <summary>
        /// The reason that we stopped sampling.
        /// 
        /// This may be one the following values:
        /// 
        /// - `"stop_sequence"`: we reached a stop sequence — either provided by you via the
        /// `StopSequences` parameter, or a stop sequence built into the model
        /// - `"max_tokens"`: we exceeded `MaxTokensToSample` or the model's maximum
        /// </summary>
        [JsonPropertyName("stop_reason")]
        public string? StopReason { get; set; }

        /// <summary>
        /// The stop sequence encountered when sampling is stopped.
        /// </summary>
        [JsonPropertyName("stop")]
        public string? Stop { get; set; }

        /// <summary>
        /// Anthropic Token usage info : Prompt tokens, Response tokens, total tokens
        /// </summary>
        public AnthropicTokenUsage? UsageInfo { get; set; }
    }
}
