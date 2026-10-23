namespace MiPresupuesto.Api.Models;

public class Categoria
{
    public int Id { get; set; }
    public Guid? UsuarioId { get; set; } // Null si es categoría predeterminada del sistema
    public string Nombre { get; set; } = string.Empty;
    public string Icono { get; set; } = "tag";
    public string ColorHex { get; set; } = "#512BD4";
    public string Tipo { get; set; } = "Gasto"; // Gasto o Ingreso
    public bool Activa { get; set; } = true;
}
