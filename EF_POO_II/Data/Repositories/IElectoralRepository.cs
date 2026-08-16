using EF_POO_II.Models;

namespace EF_POO_II.Data.Repositories;

public interface IElectoralRepository
{
    Task<IReadOnlyList<Eleccion>> ListarEleccionesAsync();

    Task<IReadOnlyList<MesaElectoral>> ListarMesasAsync();

    Task<IReadOnlyList<MesaElectoral>> ListarMesasPorUsuarioAsync(string username);

    Task<MesaElectoral?> ObtenerMesaAsync(int mesaId);

    Task<IReadOnlyList<ActaResumenDto>> ListarActasAsync();

    Task<ActaElectoral?> ObtenerActaDetalleAsync(int id);

    Task<IReadOnlyList<Auditoria>> ListarAuditoriaAsync();

    Task<IReadOnlyList<CandidatoResultado>> ListarResultadosPorEleccionAsync(int eleccionId);

    Task<int> ProcesarActaAsync(RegistrarActaRequest request, string usuario);
}
