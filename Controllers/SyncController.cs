using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiPresupuesto.Api.Data;
using MiPresupuesto.Api.DTOs;

namespace MiPresupuesto.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<SyncController> _logger;

    public SyncController(ApplicationDbContext db, ILogger<SyncController> logger)
    {
        _db = db;
        _logger = logger;
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

    /// <summary>
    /// Descarga los datos del usuario. Si se especifica 'since', solo retorna cambios posteriores a dicha fecha (Sincronización Delta).
    /// </summary>
    [HttpGet("pull")]
    public async Task<ActionResult<SyncPullResponseDto>> Pull([FromQuery] DateTime? since = null)
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        var queryTransacciones = _db.Transacciones.Where(t => t.UsuarioId == usuarioId);
        var queryCategorias = _db.Categorias.Where(c => c.UsuarioId == null || c.UsuarioId == usuarioId);
        var queryPresupuestos = _db.PresupuestosMensuales.Where(p => p.UsuarioId == usuarioId);
        var queryPagos = _db.PagosRecurrentes.Where(p => p.UsuarioId == usuarioId);

        if (since.HasValue)
        {
            var sinceUtc = since.Value.ToUniversalTime();
            queryTransacciones = queryTransacciones.Where(t => t.UpdatedAt >= sinceUtc);
            queryPresupuestos = queryPresupuestos.Where(p => p.UpdatedAt >= sinceUtc);
            queryPagos = queryPagos.Where(p => p.UpdatedAt >= sinceUtc);
        }

        var transacciones = await queryTransacciones.OrderByDescending(t => t.Fecha).ToListAsync();
        var categorias = await queryCategorias.ToListAsync();
        var presupuestos = await queryPresupuestos.ToListAsync();
        var pagos = await queryPagos.ToListAsync();

        return Ok(new SyncPullResponseDto
        {
            Transacciones = transacciones,
            Categorias = categorias,
            Presupuestos = presupuestos,
            PagosRecurrentes = pagos,
            ServerTime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Recibe un lote de movimientos nuevos/modificados en local y aplica un Upsert con resolución de conflictos por timestamp.
    /// </summary>
    [HttpPost("push")]
    public async Task<ActionResult<SyncPushResponseDto>> Push([FromBody] SyncPushRequestDto dto)
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        int procesadas = 0;

        // 1. Upsert Transacciones (Batch query para evitar N+1)
        if (dto.Transacciones != null && dto.Transacciones.Count > 0)
        {
            var ids = dto.Transacciones.Select(t => t.Id).ToList();
            var existentes = await _db.Transacciones
                .Where(x => x.UsuarioId == usuarioId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var t in dto.Transacciones)
            {
                t.UsuarioId = usuarioId;

                if (!existentes.TryGetValue(t.Id, out var existente))
                {
                    _db.Transacciones.Add(t);
                }
                else if (t.UpdatedAt >= existente.UpdatedAt)
                {
                    existente.Monto = t.Monto;
                    existente.Descripcion = t.Descripcion;
                    existente.Fecha = t.Fecha;
                    existente.Tipo = t.Tipo;
                    existente.CategoriaId = t.CategoriaId;
                    existente.EsGastoFijo = t.EsGastoFijo;
                    existente.IsDeleted = t.IsDeleted;
                    existente.UpdatedAt = t.UpdatedAt;
                }
                procesadas++;
            }
        }

        // 2. Upsert Presupuestos (Batch query con resolución de conflictos por marca temporal)
        if (dto.Presupuestos != null && dto.Presupuestos.Count > 0)
        {
            var existentes = await _db.PresupuestosMensuales
                .Where(x => x.UsuarioId == usuarioId)
                .ToListAsync();
            var dictExistentes = existentes.ToDictionary(x => (x.Mes, x.Anio));

            foreach (var p in dto.Presupuestos)
            {
                p.UsuarioId = usuarioId;
                if (!dictExistentes.TryGetValue((p.Mes, p.Anio), out var existente))
                {
                    _db.PresupuestosMensuales.Add(p);
                }
                else if (p.UpdatedAt >= existente.UpdatedAt)
                {
                    existente.IngresoEstimado = p.IngresoEstimado;
                    existente.MetaAhorro = p.MetaAhorro;
                    existente.UpdatedAt = p.UpdatedAt;
                }
                procesadas++;
            }
        }

        // 3. Upsert Pagos Recurrentes (Batch query)
        if (dto.PagosRecurrentes != null && dto.PagosRecurrentes.Count > 0)
        {
            var ids = dto.PagosRecurrentes.Select(p => p.Id).ToList();
            var existentes = await _db.PagosRecurrentes
                .Where(x => x.UsuarioId == usuarioId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var p in dto.PagosRecurrentes)
            {
                p.UsuarioId = usuarioId;

                if (!existentes.TryGetValue(p.Id, out var existente))
                {
                    _db.PagosRecurrentes.Add(p);
                }
                else if (p.UpdatedAt >= existente.UpdatedAt)
                {
                    existente.Concepto = p.Concepto;
                    existente.Monto = p.Monto;
                    existente.DiaDelMes = p.DiaDelMes;
                    existente.CategoriaId = p.CategoriaId;
                    existente.Activo = p.Activo;
                    existente.UpdatedAt = p.UpdatedAt;
                }
                procesadas++;
            }
        }

        // 4. Upsert Categorías personalizadas del usuario
        if (dto.Categorias != null && dto.Categorias.Count > 0)
        {
            var categoriasUsuario = dto.Categorias.Where(c => c.UsuarioId == null || c.UsuarioId == usuarioId).ToList();
            var ids = categoriasUsuario.Where(c => c.Id > 0).Select(c => c.Id).ToList();

            var existentes = await _db.Categorias
                .Where(x => x.UsuarioId == usuarioId && ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id);

            foreach (var c in categoriasUsuario)
            {
                c.UsuarioId = usuarioId; // Proteger las categorías maestras del sistema
                if (c.Id <= 0 || !existentes.TryGetValue(c.Id, out var existente))
                {
                    c.Id = 0; // Para que PostgreSQL genere el ID automático si es nueva
                    _db.Categorias.Add(c);
                }
                else
                {
                    existente.Nombre = c.Nombre;
                    existente.Icono = c.Icono;
                    existente.ColorHex = c.ColorHex;
                    existente.Tipo = c.Tipo;
                    existente.Activa = c.Activa;
                }
                procesadas++;
            }
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Sincronización push completada para usuario {UsuarioId}. Registros procesados: {Total}", usuarioId, procesadas);

        return Ok(new SyncPushResponseDto
        {
            Exito = true,
            Mensaje = "Sincronización completada exitosamente.",
            RegistrosProcesados = procesadas,
            ServerTime = DateTime.UtcNow
        });
    }
}
