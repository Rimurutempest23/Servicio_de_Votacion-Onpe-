using EF_POO_II.Models;

namespace EF_POO_II.Data.Repositories;

public interface ICandidatoRepository
{
    Task<List<Candidato>> GetAllAsync();
    Task<PagedResult<Candidato>> BuscarPaginadoAsync(string? filtro, int page, int pageSize);
    Task<Candidato?> GetByIdAsync(int id);
    Task AddAsync(Candidato candidato);
    Task UpdateAsync(Candidato candidato);
    Task DeleteAsync(int id);
}
