namespace MiPresupuesto.Api.DTOs;

public record RegisterDto(string Email, string Password, string Nombre);

public record LoginDto(string Email, string Password);

public record AuthResponseDto(
    string Token, 
    string Email, 
    string Nombre, 
    Guid UsuarioId, 
    DateTime Expira
);
