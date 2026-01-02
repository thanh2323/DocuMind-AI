using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IPdf;
using Microsoft.Extensions.Logging;
using SharpToken;

using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System.Text.RegularExpressions;

namespace DocuMind.Infrastructure.Services
{
    class PdfProcessorService : IPdfProcessorService
    {
        private readonly ILogger<PdfProcessorService> _logger;

        public PdfProcessorService(ILogger<PdfProcessorService> logger)
        {
            _logger = logger;
        }

        public string ExtractText(Stream pdfStream, CancellationToken cancellationToken = default)
        {
            try
            {
                var textBuilder = new StringBuilder();

                using var reader = new PdfReader(pdfStream);
                using var pdfDoc = new PdfDocument(reader);

                var totalPages = pdfDoc.GetNumberOfPages();

                _logger.LogInformation("Processing PDF: {Pages} pages", totalPages);

                for (int i = 1; i <= totalPages; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var page = pdfDoc.GetPage(i);

                    var pageText = PdfTextExtractor.GetTextFromPage(page);
                    
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        textBuilder.AppendLine(pageText);
                        textBuilder.AppendLine(); // Add extra line between pages
                    }
                }
                var extractedText = textBuilder.ToString();
                if (string.IsNullOrWhiteSpace(extractedText))
                    throw new InvalidOperationException("No text could be extracted from PDF");

                _logger.LogInformation("Extracted {Characters} characters from PDF", extractedText.Length);

                return extractedText;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF: {pdfStream}", pdfStream);
                throw;
            }
        }

        public string ExtractCleanText(Stream pdfStream)
        {
            var raw = ExtractText(pdfStream);

            // 1. Normalize newline
            raw = Regex.Replace(raw, @"\r\n|\r", "\n");
            
            // 2.  Remove "Table border"
            raw = raw.Replace("|", " ");
           
            // 2. Fix hyphenated words
            raw = Regex.Replace(raw, @"(\w+)-\s*\n(\w+)", "$1$2");

            // 3. Detect & mark headings
            raw = Regex.Replace(
                raw,
                @"\n([A-Z][A-Za-z0-9 \-:]{5,60})\n",
                "\n\n## $1\n\n"
            );

            // 4. Remove page numbers (full line only)
            raw = Regex.Replace(
                 raw,
                 @"^\s*([\|\-\.]\s*)?(Page\s*)?\d+(\s*of\s*\d+)?(\s*[\|\-\.])?\s*$",
                 "",
                 RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // 5. Remove common noise lines
            raw = Regex.Replace(
                raw,
                @"^\s*(Confidential|Draft)\s*$",
                "",
                RegexOptions.Multiline | RegexOptions.IgnoreCase
            );

            // 6. Normalize bullet characters
            raw = Regex.Replace(raw,
                @"[•‧◦‣●\u25AA\u25CF\u25E6\u2022\u2023\u2027\u2043\u2219\uF0B7]",
                "-");
            // Merge lines that were broken in the middle of sentences
            raw = Regex.Replace(raw, @"(?<=\w)\n(?=[a-z])", " ");

            // Remove excessive dots (e.g., from table of contents)
            raw = Regex.Replace(raw, @"\.{4,}\s*\d*", " ");

            // 7. Normalize whitespace slightly (DON’T merge real line breaks)
            raw = Regex.Replace(raw, @"[ \t]+", " ");
            raw = Regex.Replace(raw, @"\n{3,}", "\n\n");

            return raw.Trim();  
        }
        private string SafeOverlap(string text, int maxChars)
        {
            if (text.Length <= maxChars)
                return text;

            var cut = text.Length - maxChars;
            var sub = text.Substring(cut);

            var firstSpace = sub.IndexOf(' ');
            if (firstSpace > 0)
                return sub.Substring(firstSpace + 1);

            return sub;
        }
        public List<string> ChunkSemantic(string text, int chunkSize = 4000, int overlap = 200)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            if (overlap >= chunkSize)
                throw new ArgumentException("overlap must be smaller than chunkSize");

            var chunks = new List<string>();

            // Split by paragraphs first (semantic boundary)
            var paragraphs = Regex.Split(text,@"(?<=\n\n)|(?=## )");

            var buffer = new StringBuilder();
            var bufferLength = 0;

            foreach (var paragraph in paragraphs)
            {
                if (string.IsNullOrWhiteSpace(paragraph))
                    continue;

                // If paragraph too long → split by sentences
                if (paragraph.Length > chunkSize)
                {
                    var sentences = Regex.Split(paragraph, @"(?<=[a-z0-9][\.!\?])\s+(?=[A-Z])");

                    foreach (var sentence in sentences)
                    {
                        if (bufferLength + sentence.Length > chunkSize && bufferLength > 0)
                        {
                            // Flush current chunk
                            chunks.Add(buffer.ToString().Trim());

                            // Create overlap
                            var overlapText = SafeOverlap(buffer.ToString(), overlap);

                            buffer.Clear();
                            buffer.Append(overlapText);
                            bufferLength = overlapText.Length;
                        }

                        buffer.Append(sentence).Append(" ");
                        bufferLength += sentence.Length + 1;
                    }
                }
                else
                {
                    if (bufferLength + paragraph.Length > chunkSize && bufferLength > 0)
                    {
                        chunks.Add(buffer.ToString().Trim());

                        var overlapText = SafeOverlap(buffer.ToString(), overlap);

                        buffer.Clear();
                        buffer.Append(overlapText);
                        bufferLength = overlapText.Length;
                    }

                    buffer.Append(paragraph).Append("\n\n");
                    bufferLength += paragraph.Length + 2;
                }
            }

            // Add remaining content
            if (buffer.Length > 0)
                chunks.Add(buffer.ToString().Trim());

            return chunks;
        }

 
    }
}