using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Enum;

namespace DocuMind.Application.Interface.IIntentClassifier
{
    public interface IIntentClassifierService
    {
        Task<IntentType> ClassifyIntentAsync(string question, CancellationToken cancellationToken = default);
    }
}
