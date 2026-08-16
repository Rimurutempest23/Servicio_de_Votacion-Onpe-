using System.ComponentModel.DataAnnotations;

namespace EF_POO_II.Models;

public class RegistrarActaRequest
{
    [Range(1, int.MaxValue)]
    public int EleccionId { get; set; }

    [Range(1, int.MaxValue)]
    public int MesaElectoralId { get; set; }

    [StringLength(250)]
    public string? Observaciones { get; set; }

    [MinLength(1)]
    public List<DetalleActaRequest> Detalles { get; set; } = new();
}

public class DetalleActaRequest
{
    [Range(1, int.MaxValue)]
    public int CandidatoId { get; set; }

    [Range(0, int.MaxValue)]
    public int Votos { get; set; }
}
