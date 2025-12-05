using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;

namespace DocuMind.Infrastructure.Authorization
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // In development, allow all access
            if (httpContext.Request.Host.Host.Contains("localhost"))
            {
                return true;
            }

            // In production, require authentication and Admin role
            return httpContext.User.Identity?.IsAuthenticated == true
                && httpContext.User.IsInRole("Admin");
        }
    }
}