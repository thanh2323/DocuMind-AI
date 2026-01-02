using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.Interface.IIntentClassifier;
using DocuMind.Core.Enum;
using DocuMind.Core.Interfaces.ILLM;
using Microsoft.Extensions.Logging;

namespace DocuMind.Application.Services.IntentClassifier
{
    public class IntentClassifierService : IIntentClassifierService
    {
        private readonly ILlmService _llmService;
        private readonly ILogger<IntentClassifierService> _logger;

        public IntentClassifierService(ILlmService llmService, ILogger<IntentClassifierService> logger)
        {
            _llmService = llmService;
            _logger = logger;
        }

        public async Task<IntentType> ClassifyIntentAsync(string question, CancellationToken cancellationToken = default)
        {
            var prompt = $@"
                You are a classifier system. Classify the following user question into one of these categories:
                - QA: Fact-based questions asking for specific information (e.g., 'What is...', 'Who is...', 'When did...').
                - SUMMARY: Requests for summaries, overviews, or main points (e.g., 'Summarize...', 'Give me an overview...', 'What is this document about?').
                - EXPLANATION: Requests for explanations of concepts, workflows, reasons, or how things work (e.g., 'Explain...', 'How does X work?', 'Why is...').

                Respond ONLY with the category name (QA, SUMMARY, or EXPLANATION). Do not add any other text.

                Question: ""{question}""
                Category:";

            try
            {
                var response = await _llmService.AskAsync(prompt, cancellationToken);
                var cleanedResponse = response.Trim().ToUpper();

                if (System.Enum.TryParse<IntentType>(cleanedResponse, out var intent))
                {
                    _logger.LogInformation("Classified intent for '{Question}' as {Intent}", question, intent);
                    return intent;
                }

                _logger.LogWarning("Failed to parse intent: '{Response}'. Defaulting to QA.", cleanedResponse);
                return IntentType.QA;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying intent. Defaulting to QA.");
                return IntentType.QA;
            }
        }
    }
}
