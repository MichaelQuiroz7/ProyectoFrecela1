using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models;

[Table("imagen")]
[Index("IdProducto", Name = "id_producto")]
public partial class Imagen
{
    [Key]
    [Column("id_imagen")]
    public int IdImagen { get; set; }

    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("imagen")]
    public string ImagenUrl { get; set; } = null!;

    [ForeignKey("IdProducto")]
    [InverseProperty("Imagens")]
    public virtual Producto IdProductoNavigation { get; set; } = null!;
}
