using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MiPresupuesto.Api.Models;

namespace MiPresupuesto.Api.Services;

public interface IJwtService
{
    (string Token, DateTime Expira) GenerarToken(Usuario usuario);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime Expira) GenerarToken(Usuario usuario)
    {
        var secret = _config["Jwt:Key"] ?? "MiPresupuestoSuperClaveSecretaDeAltaSeguridad2026_Key123456";
        var issuer = _config["Jwt:Issuer"] ?? "MiPresupuestoApi";
        var audience = _config["Jwt:Audience"] ?? "MiPresupuestoMobile";
        var diasValidez = int.TryParse(_config["Jwt:DurationDays"], out var dias) ? dias : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expira = DateTime.UtcNow.AddDays(diasValidez);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim("nombre", usuario.Nombre),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expira,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expira);
    }
}
