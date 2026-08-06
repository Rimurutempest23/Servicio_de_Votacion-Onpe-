using EF_POO_II.Models;

namespace EF_POO_II.Data.Services;

public interface IVotacionService
{
    Task<IReadOnlyList<CandidatoResultadoDto>> ListarResultadosAsync();

    Task<PagedResult<CandidatoResultadoDto>> ListarResultadosPaginadosAsync(string? filtro, int page, int pageSize);

    Task<string> RegistrarVotoAsync(int candidatoId, int cantidad);
}
