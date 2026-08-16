using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("OperadorMesaAsignaciones")]
public class OperadorMesaAsignacion
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int MesaElectoralId { get; set; }

    public DateTime FechaAsignacion { get; set; }

    public DateTime? FechaCierre { get; set; }

    public bool IsActiva { get; set; } = true;

    [StringLength(120)]
    [Column(TypeName = "varchar(120)")]
    public string? Observacion { get; set; }

    public Usuario? Usuario { get; set; }

    public MesaElectoral? MesaElectoral { get; set; }
}
