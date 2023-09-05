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
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public HttpHeaders? Headers { get; private set; }    
        
        public AnthropicCompletionError? CompletionError { get; private set; }

        public AnthropicClientException(string message, Exception? innerException = null) : base(message, innerException) { }

        public AnthropicClientException(string message, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message, args))
        { }

        public AnthropicClientException(HttpStatusCode httpStatusCode, HttpHeaders? headers = null, string? message = null, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message ?? string.Empty, args))
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
        }

        public AnthropicClientException(HttpStatusCode httpStatusCode, HttpHeaders? headers = null, AnthropicCompletionError? completionError = null)
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
        public AnthropicClientConnectionException(Exception? exception = null, string? message = null) : base(message ?? "Connection error.", exception) { }
    }

    /// <summary>
    /// Custom exception class for representing Anthropic API connection timeout
    /// </summary>
    public class AnthropicClientConnectionTimeoutException : AnthropicClientConnectionException
    {
        public AnthropicClientConnectionTimeoutException() : base(message: "Request timed out.") { }
    }
}
