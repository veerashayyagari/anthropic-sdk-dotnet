using System;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;

namespace LLMSharp.Anthropic
{
    /// <summary>
    /// Base exception class for representing Anthropic API exceptions
    /// </summary>
    public class ApiException : Exception
    {
        public HttpStatusCode? HttpStatusCode { get; private set; }

        public HttpHeaders? Headers { get; private set; }        

        public ApiException(string message, Exception? innerException = null) : base(message, innerException) { }

        public ApiException(string message, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message, args))
        { }

        public ApiException(HttpStatusCode httpStatusCode, HttpHeaders? headers = null, string? message = null, params object[] args) :
            base(string.Format(CultureInfo.CurrentCulture, message ?? string.Empty, args))
        {
            HttpStatusCode = httpStatusCode;
            Headers = headers;
        }
    }

    /// <summary>
    /// Custom exception class for representing Anthropic API connection exception
    /// </summary>
    public class ApiConnectionException: ApiException 
    {
        public ApiConnectionException(Exception? exception = null, string? message = null) : base(message ?? "Connection error.", exception) { }
    }

    /// <summary>
    /// Custom exception class for representing Anthropic API connection timeout
    /// </summary>
    public class ApiConnectionTimeoutException : ApiConnectionException
    {
        public ApiConnectionTimeoutException() : base(message: "Request timed out.") { }
    }
}
