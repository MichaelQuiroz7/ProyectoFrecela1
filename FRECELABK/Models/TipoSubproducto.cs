using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models
{
    [Table("tipo_subproducto")]
    public class TipoSubproducto
    {

        [Key]
        [Column("id_tipo_subproducto")]
        public int IdTipoSubproducto { get; set; }

        [Column("nombre_subtipo")]
        [StringLength(50)]
        [Required]
        public string NombreSubtipo { get; set; } = null!;

        [InverseProperty("IdTipoSubproductoNavigation")]
        public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}
