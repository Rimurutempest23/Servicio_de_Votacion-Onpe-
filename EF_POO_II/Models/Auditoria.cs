using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("Auditorias")]
public class Auditoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Column(TypeName = "varchar(80)")]
    public string Accion { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    [Column(TypeName = "varchar(80)")]
    public string TablaAfectada { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    [Required]
    [StringLength(300)]
    [Column(TypeName = "varchar(300)")]
    public string Descripcion { get; set; } = string.Empty;
}
