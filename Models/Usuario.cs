namespace MiPresupuesto.Api.Models;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? UltimoLogin { get; set; }
}
