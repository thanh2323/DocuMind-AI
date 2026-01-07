
using System.Net.Http.Headers;
using DocuMind.Application.Factories;
using DocuMind.Application.Interface.IAdmin;
using DocuMind.Application.Interface.IAuth;
using DocuMind.Application.Interface.IChat;
using DocuMind.Application.Interface.IDocument;
using DocuMind.Application.Interface.IIntentClassifier;
using DocuMind.Application.Interface.IPrompt;
using DocuMind.Application.Interface.IRag;
using DocuMind.Application.Interface.IUser;
using DocuMind.Application.Services.AdminService;
using DocuMind.Application.Services.AuthService;
using DocuMind.Application.Services.ChatService;
using DocuMind.Application.Services.DocumentService;
using DocuMind.Application.Services.IntentClassifier;
using DocuMind.Application.Services.RagService;
using DocuMind.Application.Services.UserService;
using DocuMind.Core.Interfaces.IAuth;
using DocuMind.Core.Interfaces.IBackgroundJob;
using DocuMind.Core.Interfaces.IEmbedding;
using DocuMind.Core.Interfaces.ILLM;
using DocuMind.Core.Interfaces.IPdf;
using DocuMind.Core.Interfaces.IRepo;
using DocuMind.Core.Interfaces.IStorage;
using DocuMind.Core.Interfaces.IVectorDb;
using DocuMind.Infrastructure.Data;
using DocuMind.Infrastructure.Repositories;
using DocuMind.Infrastructure.Services;
using Google.GenAI;
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
            services.AddScoped<ISessionDocumentRepository, SessionDocumentRepository>();
            services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));


            // JWT & Password Services
 
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            //User Services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUserDashboardService, DashBoardService>();
            services.AddScoped<IAdminService, AdminService>();

            //Background Job Services   
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            // PDF Pre-Processor Service
            services.AddScoped<IPdfProcessorService, PdfProcessorService>();

            /*  // Embedding Service With Model Loaded from Ollama
              services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();

              // Configure HttpClient for Ollama
              services.AddHttpClient("Ollama", client =>
              {
                  client.BaseAddress = new Uri(configuration["Ollama:BaseUrl"]!);
                  client.Timeout = TimeSpan.FromSeconds(int.TryParse(configuration["Ollama:TimeoutSeconds"], out var timeout) ? timeout : 60);

                  client.DefaultRequestHeaders.Add("Accept", "application/json");
              });*/

            // Embedding Service With Model Loaded from Gemini
            services.AddScoped<IEmbeddingService, GeminiEmbeddingService>();
            // Configure HttpClient for Gemini Embedding and LLM
            services.AddHttpClient("Gemini", client =>
            {
                client.BaseAddress =
                    new Uri(configuration["Gemini:BaseUrl"]!);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            });

            // LLM Service
            services.AddScoped<ILlmService, GeminiLlmService>();



            // Vector DB Service
            services.AddScoped<IVectorDbService,QdrantService>();

            // Document Service
            services.AddScoped<IDocumentService, DocumentService>();

            //Store Service (Superbase)
            services.AddScoped<IStorageService, SupabaseStorageService>();
        
            services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IConfiguration>();

                return new Supabase.Client(
                    config["Supabase:Url"]!,
                    config["Supabase:ApiKey"],
                    new Supabase.SupabaseOptions
                    {
                        AutoConnectRealtime = false
                    });
            });
            
            // RAG Service
            services.AddScoped<IRagService, RagService>();
            services.AddScoped<IIntentClassifierService, IntentClassifierService>();
            services.AddScoped<IPromptFactory, PromptFactory>();

            // Document Service
            services.AddScoped<IDocumentService, DocumentService>();

            // Chat Service
            services.AddScoped<IChatService, ChatService>();
            return services;
        }
    }
}
