using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models.ModelsDTO
{
    public class TipoSubproductoDTO
    {

        [Key]
        [Column("id_tipo_subproducto")]
        public int IdTipoSubproducto { get; set; }

        [Column("nombre_subtipo")]
        [StringLength(50)]
        [Required]
        public string NombreSubtipo { get; set; } = null!;

    }
}
