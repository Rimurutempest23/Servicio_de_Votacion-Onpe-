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

        var resultados = await _votacionService.ListarResultadosAsync();

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

        var resultados = await _votacionService.ListarResultadosAsync();
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
