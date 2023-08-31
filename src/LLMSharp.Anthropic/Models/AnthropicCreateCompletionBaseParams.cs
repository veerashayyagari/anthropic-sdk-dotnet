using LLMSharp.Anthropic.Utils;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LLMSharp.Anthropic.Models
{
    public abstract class AnthropicCreateCompletionBaseParams
    {
        /// <summary>
        /// The maximum number of tokens to generate before stopping.
        /// Note: that anthropic models may stop _before_ reaching this maximum. This parameter
        /// only specifies the absolute maximum number of tokens to generate. 
        /// 
        /// Default : 2048 tokens
        /// </summary>
        [JsonPropertyName("max_tokens_to_sample")]
        public int MaxTokensToSample { get; set; } = 2048;

        /// <summary>
        /// The model that will complete your prompt.
        /// As Anthropic improve Claude, we develop new versions of it that you can query.
        /// This parameter controls which version of Claude answers your request.
        /// Right now anthropic is offering two model families: Claude, and Claude Instant.
        /// You can use them by setting `model` to `"claude2"` or `"claudeinstant1"`, respectively.
        ///
        /// Default : Claude2
        /// See [models] (https://docs.anthropic.com/claude/reference/selecting-a-model) for additional details.
        /// </summary>
        [JsonPropertyName("model")]
        [JsonConverter(typeof(LanguageModelJsonConverter))]
        public AnthropicLanguageModel Model { get; set; } = AnthropicLanguageModel.Claude2;

        /// <summary>
        /// The prompt that you want Claude to complete.
        /// 
        /// For proper response generation you will need to format your prompt as follows:
        /// ```csharp
        /// var userQuestion = r"Why is the sky blue?";
        /// var prompt = $"\n\nHuman: {userQuestion}\n\nAssistant:";
        /// ```
        /// Default : "\n\nHuman:Hello,Claude. \n\nAssistant:"
        /// See [comments on prompts](https://docs.anthropic.com/claude/docs/introduction-to-prompt-design)
        /// for more context.
        /// </summary>
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = $"{Constants.HUMAN_PROMPT}Hello, Claude. {Constants.AI_PROMPT}";

        /// <summary>
        /// Sequences that will cause the model to stop generating completion text.
        /// 
        /// Anthropic models stop on `"\n\nHuman:"`, and may include additional built-in stop
        /// sequences in the future. By providing the stop_sequences parameter, you may
        /// include additional strings that will cause the model to stop generating.
        /// </summary>
        [JsonPropertyName("stop_sequences")]
        public IEnumerable<string>? StopSequences { get; set; }

        /// <summary>
        /// Amount of randomness injected into the response.
        /// 
        /// Defaults to 1.
        /// Ranges from 0 to 1. Use temp closer to 0 for analytical / multiple choice,
        /// and closer to 1 for creative and generative tasks.
        /// </summary>
        [JsonPropertyName("temperature")]
        public float Temperature { get; set; } = 1;

        /// <summary>
        /// Only sample from the top K options for each subsequent token.
        /// 
        /// Used to remove "long tail" low probability responses.
        /// [Learn more technical details here](https://towardsdatascience.com/how-to-sample-from-language-models-682bceb97277)
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }

        /// <summary>
        /// Use nucleus sampling.
        /// In nucleus sampling, we compute the cumulative distribution over all the options
        /// for each subsequent token in decreasing probability order and cut it off once it
        /// reaches a particular probability specified by `TopP`. You should either alter
        /// Temperature or TopP , but not both
        /// </summary>
        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }

        /// <summary>
        /// An object describing metadata about the request.
        /// </summary>
        [JsonPropertyName("metadata")]
        public AnthropicCreateCompletionMetadata? Metadata { get; set; }
    }
}
