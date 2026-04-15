using System;
using System.Collections.Generic;

namespace PETADOPCION_FINAL.Models;

public partial class MascotaFoto
{
    public int IdFoto { get; set; }

    public int IdMascota { get; set; }

    public string UrlFoto { get; set; } = null!;

    public bool? EsPrincipal { get; set; }

    public virtual Mascota IdMascotaNavigation { get; set; } = null!;
}
