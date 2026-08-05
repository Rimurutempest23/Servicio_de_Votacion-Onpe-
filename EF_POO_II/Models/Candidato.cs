using System;
using System.Collections.Generic;

namespace EF_POO_II.Models;

public partial class Candidato
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public int TotalVotos { get; set; }
    public virtual ICollection<Voto> Votos { get; set; } = new List<Voto>();
}
