using EF_POO_II.Models;
using EF_POO_II.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EF_POO_II.Controllers;

[Route("[controller]")]
[Authorize(Roles = "Administrador")]
public class CandidatosController : Controller
{
    private readonly ICandidatoRepository _repo;

    public CandidatosController(ICandidatoRepository repo)
    {
        _repo = repo;
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
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost("[action]")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Candidato model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _repo.AddAsync(model);
        TempData["Success"] = "Candidato creado correctamente";
        TempData["Scope"] = "Candidatos";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost("[action]")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Candidato model)
    {
        if (!ModelState.IsValid) return View(model);
        await _repo.UpdateAsync(model);
        TempData["Success"] = "Candidato actualizado";
        TempData["Scope"] = "Candidatos";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("[action]/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity == null) return NotFound();
        return View(entity);
    }

    [HttpPost("[action]")]
    [HttpPost("[action]/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        await _repo.DeleteAsync(id);
        TempData["Success"] = "Candidato eliminado";
        TempData["Scope"] = "Candidatos";
        return RedirectToAction(nameof(Index));
    }
}
