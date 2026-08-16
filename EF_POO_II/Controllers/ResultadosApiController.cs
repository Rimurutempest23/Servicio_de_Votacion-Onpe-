using EF_POO_II.Data.Services;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[ApiController]
[Route("api/resultados")]
public class ResultadosApiController : ControllerBase
{
    private readonly IVotacionService _votacionService;
    private readonly IApiTokenService _apiTokenService;

    public ResultadosApiController(IVotacionService votacionService, IApiTokenService apiTokenService)
    {
        _votacionService = votacionService;
        _apiTokenService = apiTokenService;
    }

    [HttpGet("oficiales")]
    public async Task<IActionResult> Oficiales(string? filtro, int page = 1)
    {
        const int pageSize = 5;
        var data = await _votacionService.ListarResultadosOficialesPaginadosAsync(filtro, page, pageSize);
        var totalGeneral = (await _votacionService.ListarResultadosOficialesAsync()).Sum(c => c.Total);

        return Ok(new
        {
            totalVotos = totalGeneral,
            totalRegistros = data.TotalRegistros,
            page = data.Page,
            totalPages = data.TotalPages,
            fechaConsulta = DateTime.Now,
            items = data.Items.Select(c => new
            {
                candidato = c.Nombre,
                imagenUrl = c.ImagenUrl,
                votos = c.Total
            })
        });
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var token = _apiTokenService.ValidateToken(Request.Headers.Authorization.ToString());
        if (!token.IsValid)
        {
            return Unauthorized(new
            {
                mensaje = "Token invalido o expirado. Inicie sesion en /api/auth/login."
            });
        }

        var resultados = await _votacionService.ListarResultadosOficialesAsync();

        return Ok(new
        {
            totalVotos = resultados.Sum(c => c.Total),
            fechaConsulta = DateTime.Now,
            consultadoPor = token.Username,
            candidatos = resultados
        });
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var token = _apiTokenService.ValidateToken(Request.Headers.Authorization.ToString());
        if (!token.IsValid)
        {
            return Unauthorized(new
            {
                mensaje = "Token invalido o expirado. Inicie sesion en /api/auth/login."
            });
        }

        var resultados = await _votacionService.ListarResultadosOficialesAsync();
        var totalVotos = resultados.Sum(c => c.Total);
        var ganador = resultados.OrderByDescending(c => c.Total).FirstOrDefault();

        return Ok(new
        {
            totalCandidatos = resultados.Count,
            totalVotos,
            ganador,
            fechaConsulta = DateTime.Now
        });
    }
}
