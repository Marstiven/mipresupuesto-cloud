using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    private Guid? ObtenerUsuarioId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (claim == null || !Guid.TryParse(claim.Value, out var usuarioId))
        {
            return null;
        }
        return usuarioId;
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
            Nombre = string.IsNullOrWhiteSpace(dto.Nombre) ? "Usuario" : dto.Nombre.Trim(),
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
            return Unauthorized(new { mensaje = "Contraseña incorrecta." });
        }

        usuario.UltimoLogin = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var (token, expira) = _jwtService.GenerarToken(usuario);

        return Ok(new AuthResponseDto(token, usuario.Email, usuario.Nombre, usuario.Id, expira));
    }


    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        var usuario = await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        return Ok(new UserProfileDto(usuario.Id, usuario.Email, usuario.Nombre, usuario.FechaCreacion, usuario.UltimoLogin));
    }

 
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, usuario.PasswordHash))
        {
            return BadRequest(new { mensaje = "La contraseña actual es incorrecta." });
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Contraseña actualizada exitosamente." });
    }


    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == usuarioId);
        if (usuario == null)
        {
            return NotFound(new { mensaje = "Usuario no encontrado." });
        }

        // Eliminación en cascada de los datos relacionados
        var transacciones = await _db.Transacciones.Where(t => t.UsuarioId == usuarioId).ToListAsync();
        _db.Transacciones.RemoveRange(transacciones);

        var presupuestos = await _db.PresupuestosMensuales.Where(p => p.UsuarioId == usuarioId).ToListAsync();
        _db.PresupuestosMensuales.RemoveRange(presupuestos);

        var pagos = await _db.PagosRecurrentes.Where(p => p.UsuarioId == usuarioId).ToListAsync();
        _db.PagosRecurrentes.RemoveRange(pagos);

        var categorias = await _db.Categorias.Where(c => c.UsuarioId == usuarioId).ToListAsync();
        _db.Categorias.RemoveRange(categorias);

        _db.Usuarios.Remove(usuario);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Cuenta y todos los datos asociados han sido eliminados permanentemente." });
    }
}
