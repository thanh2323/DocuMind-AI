using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Enum;

namespace DocuMind.Application.Interface.IPrompt
{
    public interface IPromptFactory
    {
        string GetPrompt(IntentType intent, string question, string context, List<string>? conversationHistory);
    }
}
