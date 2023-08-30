using LLMSharp.Anthropic.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Utils
{
    internal class LanguageModelJsonConverter : JsonConverter<AnthropicLanguageModel>
    {
        public override AnthropicLanguageModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string val = reader.GetString()!.ToLowerInvariant();
            if (val == "claude-2")
                return AnthropicLanguageModel.Claude2;
            if (val == "claude-instant-1")
                return AnthropicLanguageModel.ClaudeInstant1;
            throw new Exception($"Unknown Language Model {val}");
        }

        public override void Write(Utf8JsonWriter writer, AnthropicLanguageModel value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
