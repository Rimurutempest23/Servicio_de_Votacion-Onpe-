using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("DetalleActas")]
public class DetalleActa
{
    [Key]
    public int Id { get; set; }

    public int ActaElectoralId { get; set; }

    public int CandidatoId { get; set; }

    [Range(0, int.MaxValue)]
    public int Votos { get; set; }

    public ActaElectoral? ActaElectoral { get; set; }

    public Candidato? Candidato { get; set; }
}
