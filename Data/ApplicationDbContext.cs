using Microsoft.EntityFrameworkCore;
using MiPresupuesto.Api.Models;

namespace MiPresupuesto.Api.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<Transaccion> Transacciones { get; set; } = null!;
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<PresupuestoMensual> PresupuestosMensuales { get; set; } = null!;
    public DbSet<PagoRecurrente> PagosRecurrentes { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Indices
        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Transaccion>()
            .HasIndex(t => new { t.UsuarioId, t.Fecha });

        modelBuilder.Entity<PresupuestoMensual>()
            .HasIndex(p => new { p.UsuarioId, p.Mes, p.Anio })
            .IsUnique();

        // Categorías por defecto del sistema
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { Id = 1, UsuarioId = null, Nombre = "Vivienda y Servicios", Icono = "🏠", ColorHex = "#3498DB", Tipo = "Gasto" },
            new Categoria { Id = 2, UsuarioId = null, Nombre = "Alimentación y Súper", Icono = "🛒", ColorHex = "#E67E22", Tipo = "Gasto" },
            new Categoria { Id = 3, UsuarioId = null, Nombre = "Transporte", Icono = "🚌", ColorHex = "#9B59B6", Tipo = "Gasto" },
            new Categoria { Id = 4, UsuarioId = null, Nombre = "Salud y Medicinas", Icono = "🩺", ColorHex = "#E74C3C", Tipo = "Gasto" },
            new Categoria { Id = 5, UsuarioId = null, Nombre = "Préstamos y Tarjetas", Icono = "💳", ColorHex = "#C0392B", Tipo = "Gasto" },
            new Categoria { Id = 6, UsuarioId = null, Nombre = "Suscripciones", Icono = "📱", ColorHex = "#1ABC9C", Tipo = "Gasto" },
            new Categoria { Id = 7, UsuarioId = null, Nombre = "Ocio y Salidas", Icono = "🎬", ColorHex = "#F39C12", Tipo = "Gasto" },
            new Categoria { Id = 8, UsuarioId = null, Nombre = "Café y Restaurantes", Icono = "☕", ColorHex = "#D35400", Tipo = "Gasto" },
            new Categoria { Id = 9, UsuarioId = null, Nombre = "Imprevistos", Icono = "⚠️", ColorHex = "#7F8C8D", Tipo = "Gasto" },
            new Categoria { Id = 10, UsuarioId = null, Nombre = "Salario / Sueldo", Icono = "💼", ColorHex = "#2ECC71", Tipo = "Ingreso" },
            new Categoria { Id = 11, UsuarioId = null, Nombre = "Bono / Prima", Icono = "🎁", ColorHex = "#27AE60", Tipo = "Ingreso" },
            new Categoria { Id = 12, UsuarioId = null, Nombre = "Horas Extras / Extra", Icono = "⚡", ColorHex = "#16A085", Tipo = "Ingreso" }
        );
    }
}
