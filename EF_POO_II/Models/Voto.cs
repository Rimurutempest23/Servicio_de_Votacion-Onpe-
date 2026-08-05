using System;
using System.Collections.Generic;

namespace EF_POO_II.Models;

public partial class Voto
{
    public int Id { get; set; }

    public int CandidatoId { get; set; }

    public int Cantidad { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Candidato Candidato { get; set; } = null!;
}
