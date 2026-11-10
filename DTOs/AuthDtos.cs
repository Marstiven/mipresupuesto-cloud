using System.ComponentModel.DataAnnotations;

namespace MiPresupuesto.Api.DTOs;

public record RegisterDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    string Password,

    string? Nombre
);

public record LoginDto(
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password
);

public record AuthResponseDto(
    string Token, 
    string Email, 
    string Nombre, 
    Guid UsuarioId, 
    DateTime Expira
);

public record UserProfileDto(
    Guid Id,
    string Email,
    string Nombre,
    DateTime FechaCreacion,
    DateTime? UltimoLogin
);

public record ChangePasswordDto(
    [Required(ErrorMessage = "La contraseña actual es obligatoria.")]
    string CurrentPassword,

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
    string NewPassword
);
