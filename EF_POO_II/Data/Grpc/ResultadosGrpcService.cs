using EF_POO_II.Data.Services;
using EF_POO_II.Grpc;
using Grpc.Core;

namespace EF_POO_II.Data.Grpc;

public class ResultadosGrpcService : ResultadosGrpc.ResultadosGrpcBase
{
    private readonly IVotacionService _votacionService;
    private readonly IElectoralService _electoralService;

    public ResultadosGrpcService(IVotacionService votacionService, IElectoralService electoralService)
    {
        _votacionService = votacionService;
        _electoralService = electoralService;
    }

    public override async Task<ResultadosReply> ObtenerResultados(ResultadosRequest request, ServerCallContext context)
    {
        var resultados = await _votacionService.ListarResultadosAsync();

        var reply = new ResultadosReply
        {
            TotalVotos = resultados.Sum(c => c.Total),
            FechaConsulta = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        reply.Candidatos.AddRange(resultados.Select(c => new CandidatoGrpc
        {
            Id = c.Id,
            Nombre = c.Nombre,
            ImagenUrl = c.ImagenUrl ?? string.Empty,
            Votos = c.Total,
            Porcentaje = c.Porcentaje
        }));

        return reply;
    }

    public override async Task<ResultadosReply> ObtenerResultadosPorEleccion(ResultadosPorEleccionRequest request, ServerCallContext context)
    {
        var resultados = await _electoralService.ListarResultadosPorEleccionAsync(request.EleccionId);

        var reply = new ResultadosReply
        {
            TotalVotos = resultados.Sum(c => c.Total),
            FechaConsulta = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        reply.Candidatos.AddRange(resultados.Select(c => new CandidatoGrpc
        {
            Id = c.Id,
            Nombre = c.Nombre,
            ImagenUrl = c.ImagenUrl ?? string.Empty,
            Votos = c.Total,
            Porcentaje = c.Porcentaje
        }));

        return reply;
    }

    public override async Task<EleccionesReply> ListarElecciones(EleccionesRequest request, ServerCallContext context)
    {
        var elecciones = await _electoralService.ListarEleccionesAsync();
        var reply = new EleccionesReply();

        reply.Elecciones.AddRange(elecciones.Select(e => new EleccionGrpc
        {
            Id = e.Id,
            Nombre = e.Nombre,
            FechaInicio = e.FechaInicio.ToString("yyyy-MM-dd HH:mm:ss"),
            FechaFin = e.FechaFin.ToString("yyyy-MM-dd HH:mm:ss"),
            Estado = e.Estado
        }));

        return reply;
    }

    public override async Task<MesasReply> ListarMesas(MesasRequest request, ServerCallContext context)
    {
        var mesas = await _electoralService.ListarMesasAsync();
        var reply = new MesasReply();

        reply.Mesas.AddRange(mesas.Select(m => new MesaGrpc
        {
            Id = m.Id,
            CodigoMesa = m.CodigoMesa,
            LocalVotacion = m.LocalVotacion,
            Distrito = m.Distrito,
            Estado = m.Estado
        }));

        return reply;
    }
}
