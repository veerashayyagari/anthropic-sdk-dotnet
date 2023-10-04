using LLMSharp.Anthropic.Models;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LLMSharp.Anthropic.Utils
{
    internal static class Extensions
    {
        private static readonly Action<ILogger<AnthropicClient>, string, Exception?> _logInfo = 
            LoggerMessage.Define<string>(LogLevel.Information, new EventId(100000, "llmsharp-anthropic-sdk-info"), "{Message}");

        private static readonly Action<ILogger<AnthropicClient>, string, Exception?> _logWarning = 
            LoggerMessage.Define<string>(LogLevel.Warning, new EventId(200000, "llmsharp-anthropic-sdk-warning"), "{Message}");

        private static readonly Action<ILogger<AnthropicClient>, string, Exception?> _logError = 
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(300000, "llmsharp-anthropic-sdk-error"), "{Message}");

        private static readonly JsonSerializerOptions options = new() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

        /// <summary>
        /// Json serializes a given class and converts it to UTF-8 encoded stringcontent to be used in HttpMessage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <returns></returns>
        internal static StringContent ToStringContent<T>(this T val) where T : class
        {            
            var jsonString = JsonSerializer.Serialize<T>(val, options);
            return new StringContent(jsonString, Encoding.UTF8, "application/json");
        }

        /// <summary>
        /// Reads a Raw SSE stream and parses it into IAsyncEnumerable stream of AnthropicCompletion objects
        /// </summary>
        /// <param name="contentStream">Raw SSE stream</param>
        /// <returns>IAsyncEnumerable stream of AnthropicCompletion objects</returns>
        /// <exception cref="InvalidDataException">Gets thrown when the stream is not a valid SSE stream</exception>
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

        /// <summary>
        /// Processes HttpResponseMessage into an Anthropic Client Exception
        /// Checks if response body can be deserialized into AnthropicCompletionError payload, and throws appropriate exception
        /// </summary>
        /// <param name="response">HttpResponseMessage with an error response code</param>
        /// <returns></returns>
        /// <exception cref="AnthropicClientException"></exception>
        internal static async Task ProcessAsAnthropicClientException(this HttpResponseMessage response)
        {
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var completionError = JsonSerializer.Deserialize<AnthropicCompletionError>(responseBody);
            if (completionError != null)
            {
                throw new AnthropicClientException(response.StatusCode, response.Headers, completionError);
            }

            throw new AnthropicClientException(response.StatusCode, response.Headers, responseBody);
        }

        /// <summary>
        /// Log info using LoggerMessage.Define based high performance logging
        /// </summary>
        /// <param name="logger">AnthropicClient logger instance</param>
        /// <param name="message">message to log</param>

        internal static void Info(this ILogger<AnthropicClient> logger, string message)
        {
            _logInfo(logger, message, null);
        }

        /// <summary>
        /// Log warning using LoggerMessage.Define based high performance logging
        /// </summary>
        /// <param name="logger">AnthropicClient logger instance</param>
        /// <param name="message">message to log</param>
        /// <param name="ex">Exception to log</param>
        internal static void Warn(this ILogger<AnthropicClient> logger, string message, Exception? ex)
        {
            _logWarning(logger, message, ex);
        }

        /// <summary>
        /// Log error using LoggerMessage.Define based high performance logging
        /// </summary>
        /// <param name="logger">AnthropicClient logger instance</param>
        /// <param name="message">message to log</param>
        /// <param name="ex">Exception to log</param>
        internal static void Error(this ILogger<AnthropicClient> logger, string message, Exception ex)
        {
            _logError(logger, message, ex);
        }
    }
}