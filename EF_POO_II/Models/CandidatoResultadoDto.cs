namespace EF_POO_II.Models;

public class CandidatoResultadoDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string? ImagenUrl { get; set; }

    public int Total { get; set; }

    public double Porcentaje { get; set; }
}
