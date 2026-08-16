using EF_POO_II.Data.Repositories;
using EF_POO_II.Data.Hubs;
using EF_POO_II.Models;
using Microsoft.AspNetCore.SignalR;

namespace EF_POO_II.Data.Services;

public class ElectoralService : IElectoralService
{
    private readonly IElectoralRepository _electoralRepository;
    private readonly IHubContext<ResultadosHub> _resultadosHub;

    public ElectoralService(IElectoralRepository electoralRepository, IHubContext<ResultadosHub> resultadosHub)
    {
        _electoralRepository = electoralRepository;
        _resultadosHub = resultadosHub;
    }

    public Task<IReadOnlyList<Eleccion>> ListarEleccionesAsync()
    {
        return _electoralRepository.ListarEleccionesAsync();
    }

    public Task<IReadOnlyList<MesaElectoral>> ListarMesasAsync()
    {
        return _electoralRepository.ListarMesasAsync();
    }

    public Task<IReadOnlyList<MesaElectoral>> ListarMesasParaUsuarioAsync(string username, bool esAdministrador)
    {
        return esAdministrador
            ? _electoralRepository.ListarMesasAsync()
            : _electoralRepository.ListarMesasPorUsuarioAsync(username);
    }

    public Task<IReadOnlyList<ActaResumenDto>> ListarActasAsync()
    {
        return _electoralRepository.ListarActasAsync();
    }

    public Task<ActaElectoral?> ObtenerActaDetalleAsync(int id)
    {
        return _electoralRepository.ObtenerActaDetalleAsync(id);
    }

    public Task<IReadOnlyList<Auditoria>> ListarAuditoriaAsync()
    {
        return _electoralRepository.ListarAuditoriaAsync();
    }

    public async Task<IReadOnlyList<CandidatoResultadoDto>> ListarResultadosPorEleccionAsync(int eleccionId)
    {
        var resultados = await _electoralRepository.ListarResultadosPorEleccionAsync(eleccionId);
        var totalVotos = resultados.Sum(r => r.Total);

        return resultados.Select(r => new CandidatoResultadoDto
        {
            Id = r.Id,
            Nombre = r.Nombre,
            ImagenUrl = r.ImagenUrl,
            Total = r.Total,
            Porcentaje = totalVotos > 0 ? Math.Round(r.Total * 100.0 / totalVotos, 2) : 0
        }).ToList();
    }

    public async Task<(bool Ok, string Mensaje, int? ActaId)> ProcesarActaAsync(RegistrarActaRequest request, string usuario, bool esAdministrador)
    {
        if (request.Detalles.Count == 0)
        {
            return (false, "Debe enviar al menos un candidato en el detalle del acta.", null);
        }

        if (request.Detalles.Sum(d => d.Votos) <= 0)
        {
            return (false, "El acta debe contener al menos un voto.", null);
        }

        if (!esAdministrador)
        {
            var mesas = await _electoralRepository.ListarMesasPorUsuarioAsync(usuario);
            if (!mesas.Any(m => m.Id == request.MesaElectoralId))
            {
                return (false, "El operador solo puede procesar una mesa activa asignada.", null);
            }
        }

        var actaId = await _electoralRepository.ProcesarActaAsync(request, usuario);
        await _resultadosHub.Clients.All.SendAsync("ResultadosActualizados", new
        {
            actaId,
            mesaId = request.MesaElectoralId,
            fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        });

        return (true, $"Acta electoral {actaId} procesada correctamente.", actaId);
    }
}
