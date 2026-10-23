using MiPresupuesto.Api.Models;

namespace MiPresupuesto.Api.DTOs;

public class SyncPullResponseDto
{
    public List<Transaccion> Transacciones { get; set; } = new();
    public List<Categoria> Categorias { get; set; } = new();
    public List<PresupuestoMensual> Presupuestos { get; set; } = new();
    public List<PagoRecurrente> PagosRecurrentes { get; set; } = new();
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}

public class SyncPushRequestDto
{
    public List<Transaccion> Transacciones { get; set; } = new();
    public List<PresupuestoMensual> Presupuestos { get; set; } = new();
    public List<PagoRecurrente> PagosRecurrentes { get; set; } = new();
}

public class SyncPushResponseDto
{
    public bool Exito { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public int RegistrosProcesados { get; set; }
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
