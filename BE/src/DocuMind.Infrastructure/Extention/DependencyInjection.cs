
using DocuMind.Application.Interface.IAuth;
using DocuMind.Application.Interface.IUser;
using DocuMind.Application.Services.AuthService;
using DocuMind.Application.Services.UserService;
using DocuMind.Core.Interfaces.IAuth;
using DocuMind.Core.Interfaces.IBackgroundJob;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IVectorDb;
using DocuMind.Infrastructure.Data;
using DocuMind.Infrastructure.Repositories;
using DocuMind.Infrastructure.Services;
using iText.StyledXmlParser.Jsoup.Parser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DocuMind.Infrastructure.Extention
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
           this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<SqlServer>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

           

            // Register repositories
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));


            // JWT & Password Services
 
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            //User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserDashboardService, DashBoardService>();

            //Background Job Services   
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            // PDF Pre-Processor Service
            services.AddScoped<IPdfProcessorService, PdfProcessorService>();

            // Embedding Service With Gemini
            services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();

            // Configure HttpClient for Ollama
            services.AddHttpClient("Ollama", client =>
            {
                client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"] ?? "http://localhost:11434");
                client.Timeout = TimeSpan.FromSeconds(int.TryParse(configuration["Ollama:TimeoutSeconds"], out var timeout) ? timeout : 60);

                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // Vector DB Service
            services.AddScoped<IVectorDbService,QdrantService>();
            return services;
        }
    }
}
