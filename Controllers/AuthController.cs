using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiPresupuesto.Api.Data;
using MiPresupuesto.Api.DTOs;
using MiPresupuesto.Api.Models;
using MiPresupuesto.Api.Services;

namespace MiPresupuesto.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IJwtService _jwtService;

    public AuthController(ApplicationDbContext db, IJwtService jwtService)
    {
        _db = db;
        _jwtService = jwtService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { mensaje = "Email y contraseña son obligatorios." });
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var existe = await _db.Usuarios.AnyAsync(u => u.Email == normalizedEmail);
        if (existe)
        {
            return Conflict(new { mensaje = "El correo electrónico ya se encuentra registrado." });
        }

        // Hashing seguro con BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var usuario = new Usuario
        {
            Email = normalizedEmail,
            PasswordHash = passwordHash,
            Nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? "Empleado" : dto.Nombre.Trim(),
            FechaCreacion = DateTime.UtcNow
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        var (token, expira) = _jwtService.GenerarToken(usuario);

        return Ok(new AuthResponseDto(token, usuario.Email, usuario.Nombre, usuario.Id, expira));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { mensaje = "Email y contraseña son obligatorios." });
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
        if (usuario == null)
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });
        }

        var passwordValido = BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash);
        if (!passwordValido)
        {
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });
        }

        usuario.UltimoLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var (token, expira) = _jwtService.GenerarToken(usuario);

        return Ok(new AuthResponseDto(token, usuario.Email, usuario.Nombre, usuario.Id, expira));
    }
}
