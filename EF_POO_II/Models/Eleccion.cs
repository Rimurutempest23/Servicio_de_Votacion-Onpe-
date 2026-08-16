using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("Elecciones")]
public class Eleccion
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    [Column(TypeName = "varchar(120)")]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaInicio { get; set; }

    public DateTime FechaFin { get; set; }

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Estado { get; set; } = "Programada";

    public ICollection<ActaElectoral> Actas { get; set; } = new List<ActaElectoral>();
}
