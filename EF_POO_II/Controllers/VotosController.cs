using EF_POO_II.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Operador")]
public class VotosController : Controller
{
    private readonly IVotacionService _votacionService;

    public VotosController(IVotacionService votacionService)
    {
        _votacionService = votacionService;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index(string? filtro, int page = 1)
    {
        const int pageSize = 5;
        var data = await _votacionService.ListarVotosPendientesAsync();
        var filtrados = data
            .Where(c => string.IsNullOrWhiteSpace(filtro) || c.Nombre.Contains(filtro, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Total)
            .ThenBy(c => c.Nombre)
            .ToList();
        var totalPages = (int)Math.Ceiling(filtrados.Count / (double)pageSize);
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));
        var items = filtrados
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        ViewBag.Candidatos = data.OrderBy(c => c.Nombre).ToList();
        ViewBag.Page = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalRegistros = filtrados.Count;
        ViewBag.Filtro = filtro;
        ViewBag.TotalVotosGeneral = data.Sum(c => c.Total);

        return View(items);
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(int candidatoId, int cantidad)
    {
        var mensaje = await _votacionService.RegistrarVotoAsync(candidatoId, cantidad);

        if (mensaje.Contains("correctamente"))
        {
            TempData["Success"] = mensaje;
            TempData["Scope"] = "Votos";
        }
        else
        {
            TempData["Error"] = mensaje;
            TempData["Scope"] = "Votos";
        }

        return RedirectToAction(nameof(Index));
    }
}
