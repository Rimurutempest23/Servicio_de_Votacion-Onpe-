using EF_POO_II.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
public class ErroresController : Controller
{
    [HttpGet("Status/{statusCode}")]
    public IActionResult Status(int statusCode)
    {
        var model = new ErrorPageViewModel
        {
            StatusCode = statusCode,
            Title = statusCode switch
            {
                401 => "Sesion requerida",
                403 => "Acceso restringido",
                404 => "Pagina no encontrada",
                405 => "Metodo no permitido",
                _ => "Solicitud no procesada"
            },
            Message = statusCode switch
            {
                401 => "Debe iniciar sesion para acceder a este modulo.",
                403 => "Su perfil no tiene permisos para realizar esta operacion.",
                404 => "La ruta solicitada no existe o fue movida.",
                405 => "La accion enviada no coincide con la ruta del controlador.",
                _ => "Revise la ruta o vuelva a intentarlo."
            },
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View("Error", model);
    }

    [HttpGet("[action]")]
    public IActionResult Exception()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var model = new ErrorPageViewModel
        {
            StatusCode = 500,
            Title = "Error interno",
            Message = exceptionFeature?.Error.Message ?? "Ocurrio un problema inesperado.",
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        };

        return View("Error", model);
    }
}
