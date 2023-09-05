using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    /// <summary>
    /// An object describing metadata about the request.
    /// </summary>
    public class AnthropicCreateCompletionMetadata
    {
        /// <summary>
        /// An external identifier for the user who is associated with the request.
        /// This should be a uuid, hash value, or other opaque identifier. Anthropic may use
        /// this id to help detect abuse. Do not include any identifying information such as
        /// name, email address, or phone number.
        /// </summary>
        [JsonPropertyName("user_id")]
        public string? UserId { get; set; }
    }
}
