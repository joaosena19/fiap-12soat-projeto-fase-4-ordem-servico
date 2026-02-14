using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tests.Integration;

/// <summary>
/// Constantes JWT para testes
/// </summary>
public static class JwtTestConstants
{
    public const string Key = "test-secret-key-min-32-characters-long-for-jwt-validation";
    public const string Issuer = "TestIssuer";
    public const string Audience = "TestAudience";
}

/// <summary>
/// Extension methods para criação de clientes autenticados em testes
/// </summary>
public static class TestAuthExtensions
{
    /// <summary>
    /// Gera um token JWT válido para os testes
    /// </summary>
    public static string CreateJwtToken(bool isAdmin = true, Guid? clienteId = null, Guid? usuarioId = null)
    {
        var user = usuarioId ?? Guid.NewGuid();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtTestConstants.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("userId", user.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (clienteId.HasValue)
        {
            claims.Add(new("clienteId", clienteId.Value.ToString()));
        }

        // Adicionar roles
        if (isAdmin)
        {
            claims.Add(new(ClaimTypes.Role, "Administrador"));
        }
        else
        {
            claims.Add(new(ClaimTypes.Role, "Cliente"));
        }

        var token = new JwtSecurityToken(
            issuer: JwtTestConstants.Issuer,
            audience: JwtTestConstants.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
