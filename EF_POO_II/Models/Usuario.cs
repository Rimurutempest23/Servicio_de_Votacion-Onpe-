using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("Usuarios")]
public partial class Usuario
{
    [Key]
    [Column("Id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [Column("Username", TypeName = "varchar(50)")]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = null!;

    [Required]
    [Column("PasswordHash", TypeName = "varchar(255)")]
    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Required]
    [Column("RolId")]
    public int RolId { get; set; }

    public bool IsActivo { get; set; } = true;

    [ForeignKey("RolId")]
    public virtual Role Rol { get; set; } = null!;

    public virtual ICollection<MesaElectoral> MesasAsignadas { get; set; } = new List<MesaElectoral>();

    public virtual ICollection<OperadorMesaAsignacion> AsignacionesMesa { get; set; } = new List<OperadorMesaAsignacion>();
}
