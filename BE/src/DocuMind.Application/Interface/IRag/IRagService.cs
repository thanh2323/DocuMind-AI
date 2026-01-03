using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Application.DTOs.Common;
using DocuMind.Application.DTOs.Rag;

namespace DocuMind.Application.Interface.IRag
{
    public interface IRagService
    {
        Task<ServiceResult<RagDto>> AskQuestionAsync(string question,List<int> documentIds,int sessionId,CancellationToken cancellationToken = default);
    }
}
