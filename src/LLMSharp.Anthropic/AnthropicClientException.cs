using LLMSharp.Anthropic.Models;
using System;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace LLMSharp.Anthropic
{
    /// <summary>
    /// Base exception class for representing Anthropic API exceptions
    /// </summary>
    public class AnthropicClientException : Exception
    {
        /// <summary>
        /// Anthropic API HTTP status code
        /// </summary>
        public HttpStatusCode? HttpStatusCode { get; private set; }

        /// <summary>
        /// Anthropic API HTTP headers
        /// </summary>
        public HttpHeaders? Headers { get; private set; }    
        
        /// <summary>
        /// Anthropic API completion error
        /// </summary>
        public AnthropicCompletionError? CompletionError { get; private set; }

        /// <summary>
        /// Anthropic Client Exception Constructor
        /// </summary>
        /// <param name="message">exception message</param>
        /// <param name="innerException">inner exception</param>
        internal AnthropicClientException(string message, Exception? innerException = null) : base(message, innerException) { }

        /// <summary>
        /// Anthropic Client Exception Constructor
        /// </summary>
        /// <param name="message">client exception message with format place holders</param>
        /// <param name="args">arguments to format the string</param>
        internal AnthropicClientException(string message, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message, args))
        { }

        internal AnthropicClientException(HttpStatusCode httpStatusCode, HttpHeaders? headers = null, string? message = null, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message ?? string.Empty, args))
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
        }

        internal AnthropicClientException(HttpStatusCode httpStatusCode, HttpHeaders? headers = null, AnthropicCompletionError? completionError = null)
            : base(completionError?.Error?.ErrorMessage ?? string.Empty)
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
            CompletionError = completionError;
        }
    }

    /// <summary>
    /// Custom exception class for representing Anthropic API connection exception
    /// </summary>
    public class AnthropicClientConnectionException: AnthropicClientException 
    {
        internal AnthropicClientConnectionException(Exception? exception = null, string? message = null) : base(message ?? "Connection error.", exception) { }
    }

    /// <summary>
    /// Custom exception class for representing Anthropic API connection timeout
    /// </summary>
    public class AnthropicClientConnectionTimeoutException : AnthropicClientConnectionException
    {
        internal AnthropicClientConnectionTimeoutException() : base(message: "Request timed out.") { }
    }
}
