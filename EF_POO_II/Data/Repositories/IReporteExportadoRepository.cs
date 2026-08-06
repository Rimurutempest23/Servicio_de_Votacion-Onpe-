using EF_POO_II.Models;

namespace EF_POO_II.Data.Repositories;

public interface IReporteExportadoRepository
{
    Task RegistrarAsync(ReporteExportado reporte);
}
