using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models;

[Table("empleado")]
[Index("cedula", Name = "cedula", IsUnique = true)]
[Index("id_rol", Name = "id_rol")]
public partial class Empleado
{
    [Key]
    [Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("nombres")]
    [StringLength(100)]
    [Required]
    public string Nombres { get; set; } = null!;

    [Column("apellidos")]
    [StringLength(100)]
    [Required]
    public string Apellidos { get; set; } = null!;

    [Column("cedula")]
    [StringLength(20)]
    [Required]
    public string Cedula { get; set; } = null!;

    [Column("fecha_nacimiento")]
    [Required]
    public DateOnly FechaNacimiento { get; set; }

    [Column("genero")]
    [StringLength(50)]
    [Required]
    public string Genero { get; set; } = null!;

    [Column("telefono")]
    [StringLength(10)]
    public string? Telefono { get; set; }

    [Column("contrasenia")]
    [StringLength(10)]
    public string? Contrasenia { get; set; }

    [Column("id_rol")]
    public int IdRol { get; set; }

    [ForeignKey("IdRol")]
    [InverseProperty("Empleados")]
    public virtual Rol IdRolNavigation { get; set; } = null!;
}