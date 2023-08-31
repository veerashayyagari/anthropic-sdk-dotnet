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
            return new StringContent(jsonString, Encoding.UTF8);
        }
    }
}