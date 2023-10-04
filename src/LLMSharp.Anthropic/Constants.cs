namespace LLMSharp.Anthropic
{
    /// <summary>
    /// AnthropicClient SDK constants    
    /// </summary>
    public class Constants
    {
#pragma warning disable CA1707 // Identifiers should not contain underscores
        /// <summary>
        /// Anthropic Completions API Human Prompt string
        /// </summary>
        public const string HUMAN_PROMPT = "\n\nHuman:";
        /// <summary>
        /// Anthropic Completions API AI Prompt string
        /// </summary>
        public const string AI_PROMPT = "\n\nAssistant:";
        /// <summary>
        /// Anthropic Completions API prompt format
        /// </summary>
        public const string PROMPT_FORMAT = "\n\nHuman:{0} \n\nAssistant:";
        /// <summary>
        /// User agent string for AnthropicClient SDK
        /// </summary>
        public const string LLMSHARP_USER_AGENT = "llmsharp-anthropic-client-sdk";
        /// <summary>
        /// Default Anthropic API endpoint version
        /// </summary>
        public const string ANTHROPIC_API_VERSION = "2023-06-01";
        /// <summary>
        /// Default Anthropic API completions endpoint
        /// </summary>
        public const string COMPLETIONS_ENDPOINT = "/v1/complete";
#pragma warning restore CA1707 // Identifiers should not contain underscores
    }
}
