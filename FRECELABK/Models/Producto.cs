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
    [Required]
    public string Nombre { get; set; } = null!;

    [Column("precio")]
    [Precision(10, 2)]
    [Required]
    public decimal Precio { get; set; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; set; }

    [Column("stock")]
    [Required]
    public int Stock { get; set; }

    [Column("id_tipo_producto")]
    [Required]
    public int IdTipoProducto { get; set; }

    [Column("id_tipo_subproducto")]
    [Required]
    public int IdTipoSubproducto { get; set; }

    [ForeignKey("IdTipoProducto")]
    [InverseProperty("Productos")]
    public virtual TipoProducto IdTipoProductoNavigation { get; set; } = null!;

    [ForeignKey("IdTipoSubproducto")]
    [InverseProperty("Productos")]
    public virtual TipoSubproducto IdTipoSubproductoNavigation { get; set; } = null!;

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<Imagen> Imagens { get; set; } = new List<Imagen>();

    [InverseProperty("IdProductoNavigation")]
    public virtual ICollection<Alerta> Alertas { get; set; } = new List<Alerta>();
}
