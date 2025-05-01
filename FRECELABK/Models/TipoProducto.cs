using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models
{
    [Table("tipo_producto")]
    public class TipoProducto
    {
        [Key]
        [Column("id_tipo_producto")]
        public int IdTipoProducto { get; set; }

        [Column("nombre_tipo")]
        [StringLength(50)]
        [Required]
        public string NombreTipo { get; set; } = null!;

        [InverseProperty("IdTipoProductoNavigation")]
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    
    }
}
