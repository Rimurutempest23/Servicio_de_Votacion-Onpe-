namespace EF_POO_II.Models;

public class ActaResumenDto
{
    public int Id { get; set; }

    public string Eleccion { get; set; } = string.Empty;

    public string CodigoMesa { get; set; } = string.Empty;

    public string Distrito { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public string UsuarioRegistro { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; }

    public int TotalVotos { get; set; }
}
