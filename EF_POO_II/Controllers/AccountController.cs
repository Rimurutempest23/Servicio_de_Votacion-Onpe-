using EF_POO_II.Helpers;
using EF_POO_II.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly SistemaVotacionContext _context;
    private readonly IConfiguration _configuration;

    public AccountController(SistemaVotacionContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("[action]")]
    [AllowAnonymous]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost("[action]")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Ingrese usuario y contraseña";
            return View();
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Username == username && u.IsActivo);

        if (usuario == null)
        {
            ViewBag.Error = "Usuario no existe";
            return View();
        }

        if (!PasswordHelper.VerifyPassword(usuario.PasswordHash, password))
        {
            ViewBag.Error = "Contrasena incorrecta";
            return View();
        }

        if (PasswordHelper.NeedsRehash(usuario.PasswordHash, password))
        {
            usuario.PasswordHash = PasswordHelper.HashPassword(password);
            await _context.SaveChangesAsync();
        }

        var sessionMinutes = _configuration.GetValue<int>("Security:SessionTimeoutMinutes", 20);
        var expiresAt = DateTime.UtcNow.AddMinutes(sessionMinutes);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
            new Claim("SessionExpiresAt", expiresAt.ToString("O"))
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = expiresAt
            });

        if (usuario.Rol.Nombre == "Administrador")
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return RedirectToAction("Index", "Votos");
    }

    [HttpGet("[action]")]
    [Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("[action]")]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
