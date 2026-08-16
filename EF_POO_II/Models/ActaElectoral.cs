using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("ActasElectorales")]
public class ActaElectoral
{
    [Key]
    public int Id { get; set; }

    public int EleccionId { get; set; }

    public int MesaElectoralId { get; set; }

    public DateTime FechaRegistro { get; set; }

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Estado { get; set; } = "Procesada";

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string UsuarioRegistro { get; set; } = string.Empty;

    [StringLength(250)]
    [Column(TypeName = "varchar(250)")]
    public string? Observaciones { get; set; }

    public Eleccion? Eleccion { get; set; }

    public MesaElectoral? MesaElectoral { get; set; }

    public ICollection<DetalleActa> Detalles { get; set; } = new List<DetalleActa>();
}
