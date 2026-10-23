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

    public SyncController(ApplicationDbContext db)
    {
        _db = db;
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
    /// Descarga todo el histórico del usuario para restaurar una app limpia o nuevo celular.
    /// </summary>
    [HttpGet("pull")]
    public async Task<ActionResult<SyncPullResponseDto>> Pull()
    {
        if (ObtenerUsuarioId() is not { } usuarioId)
        {
            return Unauthorized(new { mensaje = "Usuario no autenticado o token inválido." });
        }

        var transacciones = await _db.Transacciones
            .Where(t => t.UsuarioId == usuarioId)
            .OrderByDescending(t => t.Fecha)
            .ToListAsync();

        var categorias = await _db.Categorias
            .Where(c => c.UsuarioId == null || c.UsuarioId == usuarioId)
            .ToListAsync();

        var presupuestos = await _db.PresupuestosMensuales
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

        var pagos = await _db.PagosRecurrentes
            .Where(p => p.UsuarioId == usuarioId)
            .ToListAsync();

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
    /// Recibe un lote de movimientos nuevos/modificados en local y los guarda o actualiza (Upsert).
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

        // 2. Upsert Presupuestos (Batch query)
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
                else
                {
                    existente.IngresoEstimado = p.IngresoEstimado;
                    existente.MetaAhorro = p.MetaAhorro;
                    existente.UpdatedAt = DateTime.UtcNow;
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

        await _db.SaveChangesAsync();

        return Ok(new SyncPushResponseDto
        {
            Exito = true,
            Mensaje = "Sincronización completada exitosamente.",
            RegistrosProcesados = procesadas,
            ServerTime = DateTime.UtcNow
        });
    }
}
