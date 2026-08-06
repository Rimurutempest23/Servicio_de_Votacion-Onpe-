using EF_POO_II.Data.Repositories;
using EF_POO_II.Data.Services;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Mvc;
using Rotativa.AspNetCore;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
public class DashboardController : Controller
{
    private readonly IVotacionService _votacionService;
    private readonly IReporteExportadoRepository _reporteExportadoRepository;

    public DashboardController(
        IVotacionService votacionService,
        IReporteExportadoRepository reporteExportadoRepository)
    {
        _votacionService = votacionService;
        _reporteExportadoRepository = reporteExportadoRepository;
    }

    [HttpGet("/")]
    [HttpGet("[action]")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> ObtenerResultados(string? filtro, int page = 1)
    {
        const int pageSize = 5;
        var data = await _votacionService.ListarResultadosPaginadosAsync(filtro, page, pageSize);
        var totalGeneral = (await _votacionService.ListarResultadosAsync()).Sum(c => c.Total);

        return Json(new
        {
            totalVotos = totalGeneral,
            totalRegistros = data.TotalRegistros,
            page = data.Page,
            totalPages = data.TotalPages,
            items = data.Items.Select(c => new
            {
                candidato = c.Nombre,
                imagenUrl = c.ImagenUrl,
                votos = c.Total
            })
        });
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> ExportarPDF()
    {
        var data = await _votacionService.ListarResultadosAsync();
        var fileName = $"ReporteVotacion_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
        var rutaBase = Url.Action("ExportarPDF", "Dashboard", null, Request.Scheme) ?? "/Dashboard/ExportarPDF";
        var rutaDescarga = $"{rutaBase}?archivo={Uri.EscapeDataString(fileName)}";

        var reporte = data.Select(c => new ReporteViewModel
        {
            Nombre = c.Nombre,
            ImagenUrl = c.ImagenUrl,
            Votos = c.Total
        }).ToList();

        await _reporteExportadoRepository.RegistrarAsync(new ReporteExportado
        {
            Nombre = fileName,
            RutaGuardado = rutaDescarga
        });

        return new ViewAsPdf("ReportePDF", reporte)
        {
            FileName = fileName
        };
    }
}
