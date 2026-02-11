using Application.Contracts.Messaging;
using Application.Contracts.Messaging.DTOs;
using Infrastructure.Messaging;
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
                // Configurar região AWS
                cfg.Host("us-east-1", h =>
                {
                    // Credenciais via IAM role do pod - sem necessidade de access key
                    // A role anexada ao node group do EKS já tem permissão
                });

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
