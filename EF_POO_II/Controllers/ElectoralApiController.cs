using EF_POO_II.Data.Services;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[ApiController]
[Route("api/electoral")]
public class ElectoralApiController : ControllerBase
{
    private readonly IElectoralService _electoralService;
    private readonly IApiTokenService _apiTokenService;

    public ElectoralApiController(IElectoralService electoralService, IApiTokenService apiTokenService)
    {
        _electoralService = electoralService;
        _apiTokenService = apiTokenService;
    }

    [HttpGet("elecciones")]
    public async Task<IActionResult> ListarElecciones()
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        return Ok(await _electoralService.ListarEleccionesAsync());
    }

    [HttpGet("mesas")]
    public async Task<IActionResult> ListarMesas()
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        return Ok(await _electoralService.ListarMesasAsync());
    }

    [HttpGet("actas")]
    public async Task<IActionResult> ListarActas()
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        return Ok(await _electoralService.ListarActasAsync());
    }

    [HttpGet("auditoria")]
    public async Task<IActionResult> ListarAuditoria()
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        return Ok(await _electoralService.ListarAuditoriaAsync());
    }

    [HttpGet("elecciones/{eleccionId:int}/resultados")]
    public async Task<IActionResult> ResultadosPorEleccion(int eleccionId)
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        var resultados = await _electoralService.ListarResultadosPorEleccionAsync(eleccionId);

        return Ok(new
        {
            eleccionId,
            totalVotos = resultados.Sum(r => r.Total),
            fechaConsulta = DateTime.Now,
            candidatos = resultados
        });
    }

    [HttpPost("actas/procesar")]
    public async Task<IActionResult> ProcesarActa([FromBody] RegistrarActaRequest request)
    {
        var token = ValidarToken();
        if (!token.IsValid)
        {
            return Unauthorized(new { mensaje = "Token invalido o expirado." });
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (token.Role != "Operador")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                mensaje = "Solo el operador puede procesar actas electorales."
            });
        }

        try
        {
            var resultado = await _electoralService.ProcesarActaAsync(request, token.Username, esAdministrador: false);
            if (!resultado.Ok)
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }

            return Ok(new
            {
                mensaje = resultado.Mensaje,
                actaId = resultado.ActaId,
                procesadoPor = token.Username
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    private ApiTokenValidationResult ValidarToken()
    {
        return _apiTokenService.ValidateToken(Request.Headers.Authorization.ToString());
    }
}
