using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuMind.Infrastructure.Extention
{
    public static class CORSConfiguration
    {
        public static IServiceCollection AddCORSPolicy(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("CORS:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:3000" }; ;
            services.AddCors(options =>
            {
                options.AddPolicy("DocuMindCORSPolicy", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials();
                });
            });
            return services;
        }
    }
}
