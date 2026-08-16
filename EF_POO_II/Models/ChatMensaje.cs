using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EF_POO_II.Models;

[Table("ChatMensajes")]
public class ChatMensaje
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    [StringLength(240)]
    [Column(TypeName = "varchar(240)")]
    public string Mensaje { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }
}
