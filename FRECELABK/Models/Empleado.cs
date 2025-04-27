using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models;

[Table("empleado")]
[Index("Cedula", Name = "cedula", IsUnique = true)]
[Index("IdRol", Name = "id_rol")]
public partial class Empleado
{
    [Key]
    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("nombres")]
    [StringLength(100)]
    public string Nombres { get; set; } = null!;

    [Column("apellidos")]
    [StringLength(100)]
    public string Apellidos { get; set; } = null!;

    [Column("cedula")]
    [StringLength(20)]
    public string Cedula { get; set; } = null!;

    [Column("fecha_nacimiento")]
    public DateTime FechaNacimiento { get; set; }

    [Column("genero")]
    [StringLength(50)]
    public string Genero { get; set; } = null!;

    [Column("foto", TypeName = "blob")]
    public byte[]? Foto { get; set; }

    [Column("id_rol")]
    public int IdRol { get; set; }

    [ForeignKey("IdRol")]
    [InverseProperty("Empleados")]
    public virtual Rol IdRolNavigation { get; set; } = null!;
}
