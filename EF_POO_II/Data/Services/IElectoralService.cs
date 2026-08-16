using EF_POO_II.Models;

namespace EF_POO_II.Data.Services;

public interface IElectoralService
{
    Task<IReadOnlyList<Eleccion>> ListarEleccionesAsync();

    Task<IReadOnlyList<MesaElectoral>> ListarMesasAsync();

    Task<IReadOnlyList<MesaElectoral>> ListarMesasParaUsuarioAsync(string username, bool esAdministrador);

    Task<IReadOnlyList<ActaResumenDto>> ListarActasAsync();

    Task<ActaElectoral?> ObtenerActaDetalleAsync(int id);

    Task<IReadOnlyList<Auditoria>> ListarAuditoriaAsync();

    Task<IReadOnlyList<CandidatoResultadoDto>> ListarResultadosPorEleccionAsync(int eleccionId);

    Task<(bool Ok, string Mensaje, int? ActaId)> ProcesarActaAsync(RegistrarActaRequest request, string usuario, bool esAdministrador);
}
