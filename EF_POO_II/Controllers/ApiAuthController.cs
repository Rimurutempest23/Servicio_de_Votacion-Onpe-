using EF_POO_II.Data.Repositories;
using EF_POO_II.Data.Services;
using EF_POO_II.Helpers;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[ApiController]
[Route("api/auth")]
public class ApiAuthController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IApiTokenService _apiTokenService;
    private readonly IConfiguration _configuration;

    public ApiAuthController(
        IUsuarioRepository usuarioRepository,
        IApiTokenService apiTokenService,
        IConfiguration configuration)
    {
        _usuarioRepository = usuarioRepository;
        _apiTokenService = apiTokenService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] ApiLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { mensaje = "Ingrese usuario y contrasena." });
        }

        var usuario = await _usuarioRepository.GetByUsernameAsync(request.Username);
        if (usuario == null || usuario.PasswordHash != PasswordHelper.HashPassword(request.Password))
        {
            return Unauthorized(new { mensaje = "Credenciales invalidas." });
        }

        var tokenMinutes = _configuration.GetValue<int>("Security:ApiTokenMinutes", 20);
        var expiresAt = DateTime.UtcNow.AddMinutes(tokenMinutes);
        var token = _apiTokenService.CreateToken(usuario.Username, usuario.Rol.Nombre, expiresAt);

        return Ok(new
        {
            tokenType = "Bearer",
            accessToken = token,
            expiresAt,
            usuario = usuario.Username,
            rol = usuario.Rol.Nombre
        });
    }
}
