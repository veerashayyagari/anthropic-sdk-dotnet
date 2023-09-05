namespace LLMSharp.Anthropic
{
    /// <summary>
    /// AnthropicClient SDK constants
    /// </summary>
    public class Constants
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        public const string HUMAN_PROMPT = "\n\nHuman:";
        public const string AI_PROMPT = "\n\nAssistant:";
        public const string PROMPT_FORMAT = "\n\nHuman:{0} \n\nAssistant:";
        public const string LLMSHARP_USER_AGENT = "llmsharp-anthropic-client-sdk";
        public const string ANTHROPIC_API_VERSION = "2023-06-01";
        public const string COMPLETIONS_ENDPOINT = "/v1/complete";
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
