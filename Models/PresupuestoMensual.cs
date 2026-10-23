namespace MiPresupuesto.Api.Models;

public class PresupuestoMensual
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public int Mes { get; set; }
    public int Anio { get; set; }
    public decimal IngresoEstimado { get; set; }
    public decimal MetaAhorro { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
