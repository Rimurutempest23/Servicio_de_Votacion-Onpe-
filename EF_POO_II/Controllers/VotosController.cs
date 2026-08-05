using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using EF_POO_II.Models;

namespace EF_POO_II.Controllers
{
    // 🔐 Solo Operador y Administrador
    [Authorize(Roles = "Operador,Administrador")]
    public class VotosController : Controller
    {
        private readonly SistemaVotacionContext _context;

        public VotosController(SistemaVotacionContext context)
        {
            _context = context;
        }

        // =========================
        // GET: Votos
        // =========================
        public async Task<IActionResult> Index()
        {
            var data = await _context.Candidatos
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    Total = _context.Votos
                        .Where(v => v.CandidatoId == c.Id)
                        .Sum(v => (int?)v.Cantidad) ?? 0
                })
                .ToListAsync();

            return View(data);
        }

        // =========================
        // POST: Votos/Registrar
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(int candidatoId, int cantidad)
        {
            // 🔴 Validación básica
            if (cantidad <= 0)
            {
                TempData["Error"] = "La cantidad de votos debe ser mayor a cero.";
                return RedirectToAction(nameof(Index));
            }

            var candidato = await _context.Candidatos.FindAsync(candidatoId);

            if (candidato == null)
            {
                TempData["Error"] = "Candidato no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ Registrar voto (auditoría)
            var nuevoVoto = new Voto
            {
                CandidatoId = candidatoId,
                Cantidad = cantidad,
                Fecha = DateTime.Now
            };

            _context.Votos.Add(nuevoVoto);

            // 💡 (OPCIONAL - PRO)
            // Si agregas campo TotalVotos en Candidato:
            candidato.TotalVotos += cantidad;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Se registraron {cantidad} votos correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}