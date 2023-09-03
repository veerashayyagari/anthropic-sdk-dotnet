using LLMSharp.Anthropic.Models;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Utils
{
    /// <summary>
    /// Custom Converter for Json Serialization/Deserialization of LanguageModel Enum
    /// </summary>
    internal sealed class LanguageModelJsonConverter : JsonConverter<AnthropicLanguageModel>
    {
        public override AnthropicLanguageModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string val = reader.GetString()!.ToLowerInvariant();
            if (val == "claude-2")
                return AnthropicLanguageModel.Claude2;
            if (val == "claude-instant-1")
                return AnthropicLanguageModel.ClaudeInstant1;
            throw new InvalidOperationException($"Unknown Language Model {val}");
        }

        public override void Write(Utf8JsonWriter writer, AnthropicLanguageModel modelValue, JsonSerializerOptions options)
        {
            if (modelValue == AnthropicLanguageModel.Claude2)
            {
                writer.WriteStringValue("claude-2");
                return;
            }
            else if (modelValue == AnthropicLanguageModel.ClaudeInstant1)
            {
                writer.WriteStringValue("claude-instant-1");
                return;
            }

            throw new InvalidOperationException($"Unknown Language Model {modelValue}");
        }
    }
}
