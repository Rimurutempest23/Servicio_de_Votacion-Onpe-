using EF_POO_II.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Administrador")]
public class MesasController : Controller
{
    private readonly SistemaVotacionContext _context;

    public MesasController(SistemaVotacionContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        await SincronizarMesasProcesadasAsync();

        ViewBag.Operadores = await _context.Usuarios
            .Include(u => u.Rol)
            .Where(u => u.Rol.Nombre == "Operador" && u.IsActivo)
            .OrderBy(u => u.Username)
            .ToListAsync();

        var mesas = await _context.MesasElectorales
            .Include(m => m.Usuario)
            .Include(m => m.Actas)
            .OrderBy(m => m.Distrito)
            .ThenBy(m => m.CodigoMesa)
            .ToListAsync();

        return View(mesas);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string localVotacion, string distrito, int? usuarioId)
    {
        if (string.IsNullOrWhiteSpace(localVotacion) || string.IsNullOrWhiteSpace(distrito))
        {
            TempData["Error"] = "Complete los datos de la mesa.";
            TempData["Scope"] = "Mesas";
            return RedirectToAction(nameof(Index));
        }

        var codigoMesa = await GenerarCodigoMesaAsync();
        var mesa = new MesaElectoral
        {
            CodigoMesa = codigoMesa,
            LocalVotacion = localVotacion.Trim(),
            Distrito = distrito.Trim(),
            Estado = "Pendiente",
            UsuarioId = usuarioId
        };

        _context.MesasElectorales.Add(mesa);
        await _context.SaveChangesAsync();

        if (usuarioId.HasValue)
        {
            _context.OperadorMesaAsignaciones.Add(new OperadorMesaAsignacion
            {
                UsuarioId = usuarioId.Value,
                MesaElectoralId = mesa.Id,
                FechaAsignacion = DateTime.Now,
                IsActiva = true,
                Observacion = "Asignacion inicial"
            });
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = $"Mesa electoral {codigoMesa} creada.";
        TempData["Scope"] = "Mesas";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Asignar(int id, int? usuarioId)
    {
        var mesa = await _context.MesasElectorales.FindAsync(id);
        if (mesa == null) return NotFound();

        var tieneActaProcesada = await _context.ActasElectorales
            .AnyAsync(a => a.MesaElectoralId == id && a.Estado == "Procesada");

        if (mesa.Estado == "Procesada" || tieneActaProcesada)
        {
            if (mesa.Estado != "Procesada")
            {
                mesa.Estado = "Procesada";
                await _context.SaveChangesAsync();
            }

            TempData["Error"] = "Una mesa procesada no puede reasignarse.";
            TempData["Scope"] = "Mesas";
            return RedirectToAction(nameof(Index));
        }

        var asignacionesActivas = await _context.OperadorMesaAsignaciones
            .Where(a => a.MesaElectoralId == id && a.IsActiva)
            .ToListAsync();

        foreach (var asignacion in asignacionesActivas)
        {
            asignacion.IsActiva = false;
            asignacion.FechaCierre = DateTime.Now;
        }

        mesa.UsuarioId = usuarioId;
        if (usuarioId.HasValue)
        {
            _context.OperadorMesaAsignaciones.Add(new OperadorMesaAsignacion
            {
                UsuarioId = usuarioId.Value,
                MesaElectoralId = id,
                FechaAsignacion = DateTime.Now,
                IsActiva = true,
                Observacion = "Reasignacion operativa"
            });
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = "Asignacion de mesa actualizada.";
        TempData["Scope"] = "Mesas";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reiniciar(int id)
    {
        var mesa = await _context.MesasElectorales.FindAsync(id);
        if (mesa == null) return NotFound();

        mesa.Estado = "Pendiente";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Mesa marcada como pendiente.";
        TempData["Scope"] = "Mesas";
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerarCodigoMesaAsync()
    {
        var numero = await _context.MesasElectorales.CountAsync() + 1;
        string codigo;

        do
        {
            codigo = $"MESA-{numero:000}";
            numero++;
        }
        while (await _context.MesasElectorales.AnyAsync(m => m.CodigoMesa == codigo));

        return codigo;
    }

    private async Task SincronizarMesasProcesadasAsync()
    {
        await _context.Database.ExecuteSqlRawAsync(
            @"UPDATE m
              SET Estado = 'Procesada'
              FROM MesasElectorales m
              WHERE m.Estado <> 'Procesada'
                AND EXISTS (
                    SELECT 1
                    FROM ActasElectorales a
                    WHERE a.MesaElectoralId = m.Id
                      AND a.Estado = 'Procesada'
                );");
    }
}
