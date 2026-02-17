using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Infrastructure.Configuration;
using SalesAssistant.Infrastructure.Persistence;
using SalesAssistant.Infrastructure.Repositories;
using SalesAssistant.Infrastructure.Services;

namespace SalesAssistant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres")));

        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));

        services.AddHttpClient("openai", client =>
        {
            var baseUrl = configuration[$"{OpenAiOptions.SectionName}:BaseUrl"] ?? "https://api.openai.com/v1";
            client.BaseAddress = new Uri(baseUrl);
        });
        services.AddHttpClient("telegram");

        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IConversationService, ConversationService>();
        services.AddScoped<IOpenAiService, OpenAiService>();
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<ITelegramService, TelegramService>();

        return services;
    }
}
