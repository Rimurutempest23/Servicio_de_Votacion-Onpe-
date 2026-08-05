using EF_POO_II.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;


namespace EF_POO_II.Controllers
{
    public class DashboardController : Controller
    {
        private readonly SistemaVotacionContext _context;

        public DashboardController(SistemaVotacionContext context)
        {
            _context = context;
        }

        
        public IActionResult Index()
        {
            return View();
        }

        
        public IActionResult ObtenerResultados()
        {
            var data = _context.Candidatos
                .Select(c => new
                {
                    candidato = c.Nombre,
                    votos = c.TotalVotos
                })
                .ToList();

            return Json(data);
        }



        public IActionResult ExportarPDF()
        {
            var data = _context.Candidatos
                .Select(c => new ReporteViewModel
                {
                    Nombre = c.Nombre,
                    Votos = c.TotalVotos
                })
                .ToList();

            return new ViewAsPdf("ReportePDF", data)
            {
                FileName = "ReporteVotacion.pdf"
            };
        }
    }
}