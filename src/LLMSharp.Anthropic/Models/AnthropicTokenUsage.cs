namespace LLMSharp.Anthropic.Models
{
    /// <summary>
    /// Anthropic Request Token Usage info
    /// </summary>
    public class AnthropicTokenUsage
    {
        /// <summary>
        /// number of tokens for the prompt
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// number of tokens for the completion response
        /// </summary>
        public int CompletionTokens { get; set; }       
    }
}
