using EF_POO_II.Data.Repositories;
using EF_POO_II.Models;

namespace EF_POO_II.Data.Services;

public class VotacionService : IVotacionService
{
    private readonly IVotoRepository _votoRepository;

    public VotacionService(IVotoRepository votoRepository)
    {
        _votoRepository = votoRepository;
    }

    public Task<IReadOnlyList<CandidatoResultadoDto>> ListarResultadosAsync()
    {
        return ListarResultadosInternoAsync();
    }

    public async Task<IReadOnlyList<CandidatoResultadoDto>> ListarVotosPendientesAsync()
    {
        var pendientes = await _votoRepository.ListarVotosPendientesAsync();
        var totalVotos = pendientes.Sum(c => c.Total);

        return pendientes
            .Select(c => new CandidatoResultadoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                ImagenUrl = c.ImagenUrl,
                Total = c.Total,
                Porcentaje = totalVotos > 0 ? Math.Round(c.Total * 100.0 / totalVotos, 2) : 0
            })
            .ToList();
    }

    public async Task<PagedResult<CandidatoResultadoDto>> ListarResultadosPaginadosAsync(string? filtro, int page, int pageSize)
    {
        var resultados = await _votoRepository.ListarResultadosPaginadosAsync(filtro, page, pageSize);
        var totalVotos = resultados.Items.Sum(c => c.Total);

        return new PagedResult<CandidatoResultadoDto>
        {
            Page = resultados.Page,
            PageSize = resultados.PageSize,
            TotalRegistros = resultados.TotalRegistros,
            Items = resultados.Items.Select(c => new CandidatoResultadoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                ImagenUrl = c.ImagenUrl,
                Total = c.Total,
                Porcentaje = totalVotos > 0 ? Math.Round(c.Total * 100.0 / totalVotos, 2) : 0
            }).ToList()
        };
    }

    private async Task<IReadOnlyList<CandidatoResultadoDto>> ListarResultadosInternoAsync()
    {
        var resultados = await _votoRepository.ListarResultadosAsync();

        var totalVotos = resultados.Sum(c => c.Total);

        return resultados
            .Select(c => new CandidatoResultadoDto
            {
                Id = c.Id,
                Nombre = c.Nombre,
                ImagenUrl = c.ImagenUrl,
                Total = c.Total,
                Porcentaje = totalVotos > 0 ? Math.Round(c.Total * 100.0 / totalVotos, 2) : 0
            })
            .ToList();
    }

    public async Task<string> RegistrarVotoAsync(int candidatoId, int cantidad)
    {
        if (cantidad <= 0)
        {
            return "La cantidad de votos debe ser mayor a cero.";
        }

        var candidatoExiste = await _votoRepository.ExisteCandidatoAsync(candidatoId);
        if (!candidatoExiste)
        {
            return "Candidato no encontrado.";
        }

        await _votoRepository.RegistrarVotoConTransaccionAsync(candidatoId, cantidad);
        return $"Se registraron {cantidad} votos correctamente.";
    }
}
