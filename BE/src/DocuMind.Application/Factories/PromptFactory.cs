using System.Collections.Generic;
using System.Text;
using DocuMind.Application.Interface.IPrompt;
using DocuMind.Core.Enum;

namespace DocuMind.Application.Factories
{
    public class PromptFactory : IPromptFactory
    {
        public string GetPrompt(IntentType intent, string question, string context, List<string>? conversationHistory)
        {
            var sb = new StringBuilder();

            // COMMON SYSTEM INSTRUCTIONS
            sb.AppendLine("=== SYSTEM ===");
            sb.AppendLine("You are an advanced AI assistant for document analysis.");
            sb.AppendLine();

            // INTENT-SPECIFIC INSTRUCTIONS
            switch (intent)
            {
                case IntentType.QA:
                    sb.AppendLine("GOAL: Answer the user's specific question using only the provided context.");
                    sb.AppendLine("RULES:");
                    sb.AppendLine("1. Be precise and concise.");
                    sb.AppendLine("2. Strictly strictly stick to the provided context.");
                    sb.AppendLine("3. If the answer is not in the context, say 'I cannot find the answer in the provided documents'.");
                    break;
                case IntentType.SUMMARY:
                    sb.AppendLine("GOAL: Provide a comprehensive summary of the provided content.");
                    sb.AppendLine("RULES:");
                    sb.AppendLine("1. Extract key points and main ideas.");
                    sb.AppendLine("2. Structure the summary with bullet points.");
                    sb.AppendLine("3. Ignore minor details.");
                    break;
                case IntentType.EXPLANATION:
                    sb.AppendLine("GOAL: Explain the concept or workflow described in the context.");
                    sb.AppendLine("RULES:");
                    sb.AppendLine("1. Provide a detailed explanation.");
                    sb.AppendLine("2. Use simple language where possible.");
                    sb.AppendLine("3. Synthesize information from multiple chunks if necessary.");
                    sb.AppendLine("4. If the context is partial, explain what is available and mention what might be missing.");
                    break;
            }
            sb.AppendLine();

            // CONVERSATION HISTORY
            if (conversationHistory != null && conversationHistory.Count > 0)
            {
                sb.AppendLine("=== CONVERSATION HISTORY ===");
                foreach (var message in conversationHistory)
                {
                    sb.AppendLine(message);
                }
                sb.AppendLine();
            }

            // CONTEXT
            sb.AppendLine("=== CONTEXT (Retrieved from documents) ===");
            sb.AppendLine(string.IsNullOrWhiteSpace(context) ? "No relevant context found." : context);
            sb.AppendLine();

            // QUESTION
            sb.AppendLine("=== QUESTION ===");
            sb.AppendLine(question);
            sb.AppendLine();

            // FINAL INSTRUCTION
            sb.AppendLine("=== RESPONSE ===");

            return sb.ToString();
        }
    }
}
