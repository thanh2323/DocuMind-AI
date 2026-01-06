using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IPdf
{
    public interface IPdfProcessorService
    {
        string ExtractText(Stream pdfStream, CancellationToken cancellationToken = default);
        string ExtractCleanText(Stream pdfStream);
        List<string> ChunkSemantic(string text, int chunkSize, int overlap );

    }
}
