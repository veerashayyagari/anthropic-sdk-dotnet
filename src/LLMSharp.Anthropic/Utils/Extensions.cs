using LLMSharp.Anthropic.Models;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Utils
{
    internal static class Extensions
    {
        internal static StringContent ToStringContent<T>(this T val) where T : class
        {
            JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
            var jsonString = JsonSerializer.Serialize<T>(val, options);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        internal static async IAsyncEnumerable<AnthropicCompletion?> ReadCompletionsFromSseStream(this Stream contentStream)
        {
            var sseStreamReader = new SseReader(contentStream);

            while (true)
            {
                SseLine? sseEvent = await sseStreamReader.TryReadLineAsync().ConfigureAwait(false);

                if (sseEvent == null)
                {
                    contentStream?.Dispose();
                    break;
                }

                ReadOnlyMemory<char> name = sseEvent.Value.FieldName;

                if (name.Span.Length == 0 || name.Span.SequenceEqual("event".AsSpan())) continue;

                if (!name.Span.SequenceEqual("data".AsSpan()))
                    throw new InvalidDataException();

                ReadOnlyMemory<char> value = sseEvent.Value.FieldValue;

                if (value.Span.SequenceEqual("[DONE]".AsSpan()))
                {
                    contentStream?.Dispose();
                    break;
                }

                JsonDocument sseMessageJson = JsonDocument.Parse(value);
                AnthropicCompletion? chatCompletionsFromSse = JsonSerializer.Deserialize<AnthropicCompletion>(sseMessageJson);
                yield return chatCompletionsFromSse;
            }
        }
    }
}