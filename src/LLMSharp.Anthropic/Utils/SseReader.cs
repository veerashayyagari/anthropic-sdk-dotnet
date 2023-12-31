﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Reference Code : https://raw.githubusercontent.com/Azure/azure-sdk-for-net/Azure.AI.OpenAI_1.0.0-beta.7/sdk/openai/Azure.AI.OpenAI/src/Helpers/SseReader.cs

using System;
using System.IO;
using System.Threading.Tasks;

namespace LLMSharp.Anthropic.Utils
{
    // SSE specification: https://html.spec.whatwg.org/multipage/server-sent-events.html#parsing-an-event-stream

    /// <summary>
    /// SSE Reader implementation
    /// </summary>
    internal sealed class SseReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly StreamReader _reader;
        private bool _disposedValue;

        public SseReader(Stream stream)
        {
            _stream = stream;
            _reader = new StreamReader(stream);
        }

        public SseLine? TryReadSingleFieldEvent()
        {
            while (true)
            {
                var line = TryReadLine();
                if (line == null)
                    return null;
                if (line.Value.IsEmpty)
                    throw new InvalidDataException("event expected.");
                var empty = TryReadLine();
                if (empty != null && !empty.Value.IsEmpty)
                    throw new NotSupportedException("Multi-filed events not supported.");
                if (!line.Value.IsComment)
                    return line; // skip comment lines
            }
        }

        // TODO: we should support cancellation tokens, but StreamReader does not in NS2
        public async Task<SseLine?> TryReadSingleFieldEventAsync()
        {
            while (true)
            {
                var line = await TryReadLineAsync().ConfigureAwait(false);
                if (line == null)
                    return null;
                if (line.Value.IsEmpty)
                    throw new InvalidDataException("event expected.");
                var empty = await TryReadLineAsync().ConfigureAwait(false);
                if (empty != null && !empty.Value.IsEmpty)
                    throw new NotSupportedException("Multi-filed events not supported.");
                if (!line.Value.IsComment)
                    return line; // skip comment lines
            }
        }

        public SseLine? TryReadLine()
        {
            if (_reader.EndOfStream)
                return null;
            string lineText = _reader.ReadLine();
            if (lineText.Length == 0)
                return SseLine.Empty;
            if (TryParseLine(lineText, out SseLine line))
                return line;
            return null;
        }

        // TODO: we should support cancellation tokens, but StreamReader does not in NS2
        public async Task<SseLine?> TryReadLineAsync()
        {
            if (_reader.EndOfStream)
                return null;
            string lineText = await _reader.ReadLineAsync().ConfigureAwait(false);
            if (lineText.Length == 0)
                return SseLine.Empty;
            if (TryParseLine(lineText, out SseLine line))
                return line;
            return null;
        }

        private static bool TryParseLine(string lineText, out SseLine line)
        {
            if (lineText.Length == 0)
            {
                line = default;
                return false;
            }

            ReadOnlySpan<char> lineSpan = lineText.AsSpan();
            int colonIndex = lineSpan.IndexOf(':');
            ReadOnlySpan<char> fieldValue = lineSpan.Slice(colonIndex + 1);

            bool hasSpace = false;
            if (fieldValue.Length > 0 && fieldValue[0] == ' ')
                hasSpace = true;
            line = new SseLine(lineText, colonIndex, hasSpace);
            return true;
        }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _reader.Dispose();
                    _stream.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
