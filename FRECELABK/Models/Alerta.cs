using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace FRECELABK.Models
{
    [Table("alerta")]
    [Index("id_producto", Name = "id_producto")]
    public partial class Alerta
    {
        [Column("id_producto")]
        [Required]
        public int IdProducto { get; set; }

        [Column("nombre_producto")]
        [StringLength(100)]
        [Required]
        public string NombreProducto { get; set; } = null!;

        [Column("mensaje", TypeName = "text")]
        [Required]
        public string Mensaje { get; set; } = null!;

        [Column("activo")]
        [Required]
        public bool Activo { get; set; } = true;

        [ForeignKey("IdProducto")]
        [InverseProperty("Alertas")]
        public virtual Producto IdProductoNavigation { get; set; } = null!;
    }
}
