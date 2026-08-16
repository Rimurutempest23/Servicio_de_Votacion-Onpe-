using EF_POO_II.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Administrador")]
public class EleccionesController : Controller
{
    private readonly SistemaVotacionContext _context;

    public EleccionesController(SistemaVotacionContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        var elecciones = await _context.Elecciones
            .OrderByDescending(e => e.FechaInicio)
            .ToListAsync();

        return View(elecciones);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string nombre, DateTime fechaInicio, DateTime fechaFin)
    {
        if (string.IsNullOrWhiteSpace(nombre) || fechaFin <= fechaInicio)
        {
            TempData["Error"] = "Ingrese una eleccion valida.";
            TempData["Scope"] = "Elecciones";
            return RedirectToAction(nameof(Index));
        }

        _context.Elecciones.Add(new Eleccion
        {
            Nombre = nombre,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Estado = "Programada"
        });

        await _context.SaveChangesAsync();
        TempData["Success"] = "Eleccion creada correctamente.";
        TempData["Scope"] = "Elecciones";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abrir(int id)
    {
        var eleccion = await _context.Elecciones.FindAsync(id);
        if (eleccion == null) return NotFound();

        eleccion.Estado = "Abierta";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Eleccion abierta para procesamiento de actas.";
        TempData["Scope"] = "Elecciones";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("[action]/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cerrar(int id)
    {
        var eleccion = await _context.Elecciones.FindAsync(id);
        if (eleccion == null) return NotFound();

        eleccion.Estado = "Cerrada";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Eleccion cerrada.";
        TempData["Scope"] = "Elecciones";
        return RedirectToAction(nameof(Index));
    }
}
