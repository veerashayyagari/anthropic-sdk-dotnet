using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    public class AnthropicCompletionErrorPayload
    {
        [JsonPropertyName("type")]
        public string? ErrorType { get; set; }

        [JsonPropertyName("message")]
        public string? ErrorMessage { get; set; }
    }

    public class AnthropicCompletionError
    {
        [JsonPropertyName("error")]
        public AnthropicCompletionErrorPayload? Error { get; set; }
    }
}
