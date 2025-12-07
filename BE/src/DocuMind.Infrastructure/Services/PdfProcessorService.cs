using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IPdf;
using Microsoft.Extensions.Logging;


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

        public string ExtractText(string filePath, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("PDF file not found.", filePath);

                var textBuilder = new StringBuilder();

                using var reader = new PdfReader(filePath);
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
                        textBuilder.AppendLine();
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
                _logger.LogError(ex, "Error extracting text from PDF: {FilePath}", filePath);
                throw;
            }
        }

        public string ExtractCleanText(string filePath)
        {
            var raw = ExtractText(filePath);

            // 1. Fix hyphen broken words
            raw = Regex.Replace(raw, @"(\w+)-\s*\n(\w+)", "$1$2");

            // 2. Normalize whitespace
            raw = Regex.Replace(raw, @"[ \t]+", " ");
            raw = Regex.Replace(raw, @"\n{3,}", "\n\n");

            // 3. Normalize bullets
            raw = raw.Replace("•", "-");

            return raw.Trim();
        }


        public List<string> ChunkText(string text, int chunkSize = 500, int overlap = 50)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new List<string>();
            }

            var chunks = new List<string>();

            // Split by sentences first (better semantic boundaries)
            var sentences = SplitIntoSentences(text);

            var currentChunk = new StringBuilder();
            var currentLength = 0;

            foreach (var sentence in sentences)
            {
                var sentenceLength = sentence.Length;

                // If adding this sentence exceeds chunk size and we have content, save chunk
                if (currentLength + sentenceLength > chunkSize && currentLength > 0)
                {
                    chunks.Add(currentChunk.ToString().Trim());

                    // Create overlap: keep last overlap characters
                    var chunkText = currentChunk.ToString();
                    if (chunkText.Length > overlap)
                    {
                        var overlapText = chunkText.Substring(chunkText.Length - overlap);
                        currentChunk = new StringBuilder(overlapText);
                        currentLength = overlap;
                    }
                    else
                    {
                        currentChunk.Clear();
                        currentLength = 0;
                    }
                }

                // Add sentence to current chunk
                currentChunk.Append(sentence);
                currentChunk.Append(" ");
                currentLength += sentenceLength + 1;
            }

            // Add remaining text
            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString().Trim());
            }

            _logger.LogInformation("Text chunked into {ChunkCount} chunks", chunks.Count);
            return chunks;
        }


        private List<string> SplitIntoSentences(string text)
        {
            // Simple sentence splitting - can be improved with NLP libraries
            var sentences = new List<string>();
            var sentenceEndings = new[] { '.', '!', '?' };

            var currentSentence = new StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                currentSentence.Append(text[i]);

                if (sentenceEndings.Contains(text[i]))
                {
                    // Check if next char is whitespace (end of sentence)
                    if (i + 1 >= text.Length || char.IsWhiteSpace(text[i + 1]))
                    {
                        var sentence = currentSentence.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(sentence))
                        {
                            sentences.Add(sentence);
                        }
                        currentSentence.Clear();
                    }
                }
            }

            // Add remaining text
            if (currentSentence.Length > 0)
            {
                var sentence = currentSentence.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(sentence))
                {
                    sentences.Add(sentence);
                }
            }

            return sentences;
        }




        public bool ValidatePdf(string filePath)
        {
            try
            {
                // 1. File existence
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning("PDF file not found: {FilePath}", filePath);
                    return false;
                }

                // 2. Size limit (50MB)
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 50 * 1024 * 1024)
                {
                    _logger.LogWarning("PDF too large: {Size}", fileInfo.Length);
                    return false;
                }

                // 3. Deep validation
                using var reader = new PdfReader(filePath);
                using var pdfDoc = new PdfDocument(reader);

                if (reader.IsEncrypted())
                {
                    _logger.LogWarning("PDF encrypted: {FilePath}", filePath);
                    return false;
                }

                var pages = pdfDoc.GetNumberOfPages();
                if (pages <= 0)
                {
                    _logger.LogWarning("PDF has no pages: {FilePath}", filePath);
                    return false;
                }

                return true;
            }
            catch
            {
                _logger.LogWarning("PDF validation failed: {FilePath}", filePath);
                return false;
            }
        }
    }
}