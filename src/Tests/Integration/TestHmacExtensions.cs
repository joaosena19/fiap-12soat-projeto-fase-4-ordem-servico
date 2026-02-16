using System.Text;
using System.Text.Json;

namespace Tests.Integration;

/// <summary>
/// Extension methods para adicionar assinatura HMAC a requisições HTTP em testes
/// </summary>
public static class TestHmacExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Envia uma requisição POST com assinatura HMAC válida para endpoints webhook
    /// </summary>
    public static async Task<HttpResponseMessage> PostAsJsonWithHmacAsync<T>(
        this HttpClient client,
        string requestUri,
        T value,
        JsonSerializerOptions? options = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        // Serializar o objeto para JSON
        var serializerOptions = options ?? JsonOptions;
        
        var json = JsonSerializer.Serialize(value, serializerOptions);

        // Gerar assinatura HMAC
        var signature = TestHmacUtils.ComputeHmacSignature(json);

        // Configurar headers e body
        request.Headers.Add("X-Signature", signature);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        return await client.SendAsync(request);
    }

    /// <summary>
    /// Adiciona assinatura HMAC a uma requisição HTTP existente
    /// </summary>
    public static void AddHmacSignature(this HttpRequestMessage request, string body)
    {
        var signature = TestHmacUtils.ComputeHmacSignature(body);
        request.Headers.Add("X-Signature", signature);
    }
}
