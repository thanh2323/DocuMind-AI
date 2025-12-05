using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using DocuMind.Infrastructure.Authorization;

namespace DocuMind.Infrastructure.Middleware
{
    public static class HangfireDashboardMiddleware
    {
        public static IApplicationBuilder UseHangfireDashboardConfigured(this IApplicationBuilder app)
        {
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAuthorizationFilter() },
                DashboardTitle = "DocuMind Background Jobs",
                IsReadOnlyFunc = (DashboardContext context) =>
                {
                    // Optional: Set read-only mode for non-admin users
                    var httpContext = context.GetHttpContext();
                    return !httpContext.User.IsInRole("Admin");
                }
            });

            return app;
        }
    }
}
