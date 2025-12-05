using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IBackgroundJob;

namespace DocuMind.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        public string EnqueueDocumentProcessing(int documentId)
        {
            throw new NotImplementedException();
        }

        public void RecurringDocumentCleanup()
        {
            throw new NotImplementedException();
        }

        public void ScheduleDocumentProcessing(int documentId, TimeSpan delay)
        {
            throw new NotImplementedException();
        }
    }
}
