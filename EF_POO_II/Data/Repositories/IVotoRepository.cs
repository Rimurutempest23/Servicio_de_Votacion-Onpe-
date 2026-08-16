using EF_POO_II.Models;

namespace EF_POO_II.Data.Repositories;

public interface IVotoRepository
{
    Task<bool> ExisteCandidatoAsync(int candidatoId);

    Task<IReadOnlyList<CandidatoResultado>> ListarResultadosAsync();

    Task<IReadOnlyList<CandidatoResultado>> ListarResultadosOficialesAsync();

    Task<PagedResult<CandidatoResultado>> ListarResultadosOficialesPaginadosAsync(string? filtro, int page, int pageSize);

    Task<IReadOnlyList<CandidatoResultado>> ListarVotosPendientesAsync();

    Task<PagedResult<CandidatoResultado>> ListarResultadosPaginadosAsync(string? filtro, int page, int pageSize);

    Task RegistrarVotoConTransaccionAsync(int candidatoId, int cantidad);
}
