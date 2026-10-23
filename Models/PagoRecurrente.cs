namespace MiPresupuesto.Api.Models;

public class PagoRecurrente
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UsuarioId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public int DiaDelMes { get; set; }
    public int CategoriaId { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
