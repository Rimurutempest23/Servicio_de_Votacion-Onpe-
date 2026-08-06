using EF_POO_II.Data.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Operador,Administrador")]
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
        var data = await _votacionService.ListarResultadosAsync();
        var resultado = await _votacionService.ListarResultadosPaginadosAsync(filtro, page, pageSize);

        ViewBag.Candidatos = data.OrderBy(c => c.Nombre).ToList();
        ViewBag.Page = resultado.Page;
        ViewBag.TotalPages = resultado.TotalPages;
        ViewBag.TotalRegistros = resultado.TotalRegistros;
        ViewBag.Filtro = filtro;
        ViewBag.TotalVotosGeneral = data.Sum(c => c.Total);

        return View(resultado.Items);
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
