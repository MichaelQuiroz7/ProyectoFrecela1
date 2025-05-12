using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models.ModelsDTO
{
    public class TipoProductoDTO
    {

        [Key]
        [Column("id_tipo_producto")]
        public int IdTipoProducto { get; set; }

        [Column("nombre_tipo")]
        [StringLength(50)]
        [Required]
        public string NombreTipo { get; set; } = null!;


    }
}
