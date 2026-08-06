using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("ReportesExportados")]
public class ReporteExportado
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Column(TypeName = "varchar(150)")]
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaCreacion { get; set; }

    [Required]
    [StringLength(300)]
    [Column(TypeName = "varchar(300)")]
    public string RutaGuardado { get; set; } = string.Empty;
}
