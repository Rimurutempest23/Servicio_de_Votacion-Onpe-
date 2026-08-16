using EF_POO_II.Data.Services;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Operador,Administrador")]
public class ActasController : Controller
{
    private readonly IElectoralService _electoralService;
    private readonly IVotacionService _votacionService;

    public ActasController(IElectoralService electoralService, IVotacionService votacionService)
    {
        _electoralService = electoralService;
        _votacionService = votacionService;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index()
    {
        await CargarCombosAsync();
        ViewBag.Actas = await _electoralService.ListarActasAsync();
        return View(new RegistrarActaRequest());
    }

    [HttpGet("[action]/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var acta = await _electoralService.ObtenerActaDetalleAsync(id);
        if (acta == null)
        {
            TempData["Error"] = "No se encontro el acta solicitada.";
            TempData["Scope"] = "Actas";
            return RedirectToAction(nameof(Index));
        }

        return View(acta);
    }

    [HttpPost("[action]")]
    [Authorize(Roles = "Operador")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Procesar(RegistrarActaRequest model)
    {
        if (!ModelState.IsValid)
        {
            await CargarCombosAsync();
            ViewBag.Actas = await _electoralService.ListarActasAsync();
            return View("Index", model);
        }

        try
        {
            var usuario = User.Identity?.Name ?? "web";
            var resultado = await _electoralService.ProcesarActaAsync(model, usuario, esAdministrador: false);

            if (resultado.Ok)
            {
                TempData["Success"] = resultado.Mensaje;
            }
            else
            {
                TempData["Error"] = resultado.Mensaje;
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        TempData["Scope"] = "Actas";
        return RedirectToAction(nameof(Index));
    }

    private async Task CargarCombosAsync()
    {
        var usuario = User.Identity?.Name ?? string.Empty;
        var esAdministrador = User.IsInRole("Administrador");
        ViewBag.Elecciones = await _electoralService.ListarEleccionesAsync();
        ViewBag.Mesas = await _electoralService.ListarMesasParaUsuarioAsync(usuario, esAdministrador);
        ViewBag.Candidatos = await _votacionService.ListarVotosPendientesAsync();
        ViewBag.EsAdministrador = esAdministrador;
    }
}
