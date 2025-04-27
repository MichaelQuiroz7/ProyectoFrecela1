using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Models;

[Table("producto")]
public partial class Producto
{
    [Key]
    [Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("nombre")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    [Column("precio")]
    [Precision(10, 2)]
    public decimal Precio { get; set; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; set; }

    [Column("stock")]
    public int Stock { get; set; }

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<Imagen> Imagens { get; set; } = new List<Imagen>();
}
