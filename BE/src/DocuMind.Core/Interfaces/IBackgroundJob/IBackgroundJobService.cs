using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocuMind.Core.Interfaces.IBackgroundJob
{
    public interface IBackgroundJobService
    {
        string EnqueueDocumentProcessing(int documentId);
        void ScheduleDocumentProcessing(int documentId, TimeSpan delay);
        void RecurringDocumentCleanup();
    }

}
