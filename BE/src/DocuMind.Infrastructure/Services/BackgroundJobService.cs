using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Interfaces.IBackgroundJob;
using Hangfire;

namespace DocuMind.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        public string EnqueueDocumentProcessing(int documentId)
        {
            return BackgroundJob.Enqueue<DocumentJob>(
                job => job.ProcessDocumentAsync(documentId, CancellationToken.None)
            );
        }

        public void ScheduleDocumentProcessing(int documentId, TimeSpan delay)
        {
            BackgroundJob.Schedule<DocumentJob>(
                job => job.ProcessDocumentAsync(documentId, CancellationToken.None),
                delay
            );
        }

        public void RecurringDocumentCleanup()
        {
            RecurringJob.AddOrUpdate<DocumentJob>(
                "cleanup-failed-documents",
                job => job.CleanupFailedDocumentsAsync(CancellationToken.None),
                Cron.Daily
            );
        }
    }
}
