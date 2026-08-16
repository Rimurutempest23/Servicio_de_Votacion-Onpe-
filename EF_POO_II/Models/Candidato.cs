using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("Candidatos")]
public partial class Candidato
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("Nombre", TypeName = "varchar(100)")]
    [StringLength(100, MinimumLength = 2)]
    public string Nombre { get; set; } = null!;

    [Column("ImagenUrl", TypeName = "varchar(300)")]
    [StringLength(300)]
    public string? ImagenUrl { get; set; }

    [Column("TotalVotos")]
    [Range(0, int.MaxValue)]
    public int TotalVotos { get; set; }

    public bool IsActivo { get; set; } = true;

    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}
