using System.Security.Cryptography;
using System.Text;

namespace Tests.Integration;

/// <summary>
/// Utilitários para computação de assinatura HMAC-SHA256 em testes
/// </summary>
public static class TestHmacUtils
{
    /// <summary>
    /// Secret HMAC para testes - deve ser o mesmo configurado no TestWebApplicationFactory
    /// </summary>
    public const string TestHmacSecret = "test-hmac-secret-key-for-webhooks-min-32-chars-long";

    /// <summary>
    /// Computa assinatura HMAC-SHA256 para o payload fornecido
    /// </summary>
    /// <param name="payload">Payload (geralmente JSON) a ser assinado</param>
    /// <returns>Assinatura HMAC no formato "sha256={hex}"</returns>
    public static string ComputeHmacSignature(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(TestHmacSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var hex = Convert.ToHexString(hash).ToLowerInvariant();
        return $"sha256={hex}";
    }
}
