using EF_POO_II.Data.Services;
using EF_POO_II.Grpc;
using Grpc.Core;

namespace EF_POO_II.Data.Grpc;

public class ResultadosGrpcService : ResultadosGrpc.ResultadosGrpcBase
{
    private readonly IVotacionService _votacionService;

    public ResultadosGrpcService(IVotacionService votacionService)
    {
        _votacionService = votacionService;
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
}
