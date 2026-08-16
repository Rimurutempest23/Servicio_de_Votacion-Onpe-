using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("MesasElectorales")]
public class MesaElectoral
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string CodigoMesa { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    [Column(TypeName = "varchar(120)")]
    public string LocalVotacion { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Column(TypeName = "varchar(80)")]
    public string Distrito { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Estado { get; set; } = "Pendiente";

    public int? UsuarioId { get; set; }

    public Usuario? Usuario { get; set; }

    public ICollection<ActaElectoral> Actas { get; set; } = new List<ActaElectoral>();
}
