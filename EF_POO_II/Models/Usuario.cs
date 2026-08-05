using System;
using System.Collections.Generic;

namespace EF_POO_II.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RolId { get; set; }

    public virtual Role Rol { get; set; } = null!;
}
