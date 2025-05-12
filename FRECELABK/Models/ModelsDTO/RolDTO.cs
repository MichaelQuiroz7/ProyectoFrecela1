using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models.ModelsDTO
{
    public class RolDTO
    {
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre_rol")]
        [StringLength(50)]
        public string NombreRol { get; set; } = null!;
    }
}
