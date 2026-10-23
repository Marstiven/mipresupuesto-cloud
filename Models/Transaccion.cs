namespace MiPresupuesto.Api.Models;

public class Transaccion
{
    public Guid Id { get; set; } // Mismo GUID de ServidorTransaccionId en la app móvil
    public Guid UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Tipo { get; set; } = "Gasto"; // Gasto o Ingreso
    public int CategoriaId { get; set; }
    public bool EsGastoFijo { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
