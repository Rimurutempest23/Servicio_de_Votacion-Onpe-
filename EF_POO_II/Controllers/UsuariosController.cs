using EF_POO_II.Helpers;
using EF_POO_II.Models;
using EF_POO_II.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Administrador")]
public class UsuariosController : Controller
{
    private readonly IUsuarioRepository _repo;
    private readonly SistemaVotacionContext _context;

    public UsuariosController(IUsuarioRepository repo, SistemaVotacionContext context)
    {
        _repo = repo;
        _context = context;
    }

    [HttpGet("")]
    [HttpGet("[action]")]
    public async Task<IActionResult> Index(string? filtro, int page = 1)
    {
        const int pageSize = 5;
        var resultado = await _repo.BuscarPaginadoAsync(filtro, page, pageSize);

        ViewBag.Page = resultado.Page;
        ViewBag.TotalPages = resultado.TotalPages;
        ViewBag.TotalRegistros = resultado.TotalRegistros;
        ViewBag.Filtro = filtro;

        return View(resultado.Items);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
        return View();
    }

    [HttpPost("[action]")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string username, string password, int rolId)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Error = "Todos los campos son obligatorios";
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            return View();
        }

        var existe = await _repo.GetByUsernameAsync(username);
        if (existe != null)
        {
            ViewBag.Error = "El usuario ya existe";
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            return View();
        }

        var nuevoUsuario = new Usuario
        {
            Username = username,
            PasswordHash = PasswordHelper.HashPassword(password),
            RolId = rolId
        };

        await _repo.AddAsync(nuevoUsuario);

        TempData["Success"] = "Usuario creado correctamente";
        TempData["Scope"] = "Usuarios";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var usuario = await _repo.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
        return View(usuario);
    }

    [HttpGet("[action]/{id:int}")]
    public async Task<IActionResult> HistorialActas(int id)
    {
        var usuario = await _repo.GetByIdAsync(id);
        if (usuario == null) return NotFound();

        var actas = await _context.ActasElectorales
            .AsNoTracking()
            .Include(a => a.Eleccion)
            .Include(a => a.MesaElectoral)
            .Where(a => a.UsuarioRegistro == usuario.Username)
            .OrderByDescending(a => a.FechaRegistro)
            .ToListAsync();

        ViewBag.Usuario = usuario;
        return View(actas);
    }

    [HttpPost("[action]")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string username, string? password, int rolId)
    {
        var usuario = await _repo.GetByIdAsync(id);
        if (usuario == null) return NotFound();

        if (string.IsNullOrWhiteSpace(username))
        {
            ViewBag.Error = "El nombre de usuario es obligatorio";
            ViewBag.Roles = await _context.Roles.OrderBy(r => r.Nombre).ToListAsync();
            return View(usuario);
        }

        usuario.Username = username;
        usuario.RolId = rolId;
        if (!string.IsNullOrWhiteSpace(password))
        {
            usuario.PasswordHash = PasswordHelper.HashPassword(password);
        }

        await _repo.UpdateAsync(usuario);
        TempData["Success"] = "Usuario actualizado";
        TempData["Scope"] = "Usuarios";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var usuario = await _repo.GetByIdAsync(id);
        if (usuario == null) return NotFound();
        return View(usuario);
    }

    [HttpPost("[action]")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.DeleteAsync(id);
        TempData["Success"] = "Usuario eliminado";
        TempData["Scope"] = "Usuarios";
        return RedirectToAction(nameof(Index));
    }
}
