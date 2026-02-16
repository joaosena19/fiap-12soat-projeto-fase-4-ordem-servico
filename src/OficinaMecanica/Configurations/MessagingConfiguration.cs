using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Filters;
using MassTransit;

namespace API.Configurations;

public static class MessagingConfiguration
{
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            // Registrar consumer de resultado de redução de estoque
            x.AddConsumer<ReducaoEstoqueResultadoConsumer>();

            // Configurar Amazon SQS como transport
            x.UsingAmazonSqs((context, cfg) =>
            {
                var region = configuration["AWS:Region"] ?? "us-east-1";

                cfg.Host(region, h =>
                {
                    var accessKey = configuration["AWS:AccessKeyId"];
                    var secretKey = configuration["AWS:SecretAccessKey"];

                    if (!string.IsNullOrEmpty(accessKey) && !string.IsNullOrEmpty(secretKey))
                    {
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);
                    }
                });

                // Registrar filtros globais de correlação
                cfg.UseConsumeFilter(typeof(ConsumeCorrelationIdFilter<>), context);
                cfg.UseSendFilter(typeof(SendCorrelationIdFilter<>), context);
                cfg.UsePublishFilter(typeof(PublishCorrelationIdFilter<>), context);

                // Configurar endpoint para receber mensagens de resultado
                cfg.ReceiveEndpoint("fase4-estoque-reducao-estoque-resultado", e =>
                {
                    e.ConfigureConsumer<ReducaoEstoqueResultadoConsumer>(context);
                });

                // Configurar formato de mensagens
                cfg.ConfigureJsonSerializerOptions(options =>
                {
                    options.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                    return options;
                });
            });
        });

        // Registrar publisher de mensagens de estoque
        services.AddScoped<IEstoqueMessagePublisher, EstoqueMessagePublisher>();

        return services;
    }
}
