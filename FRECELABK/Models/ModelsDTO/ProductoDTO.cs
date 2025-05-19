using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace FRECELABK.Models.ModelsDTO
{
    public class ProductoDTO
    {

        public int IdProducto { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public string? Descripcion { get; set; }
        public int Stock { get; set; }
        public int IdTipoProducto { get; set; }
        public int IdTipoSubproducto { get; set; }

    }

    public class ProductoStock
    {
        
        public int IdProducto { get; set; }

        public bool aumentar { get; set; }

        public int cantidad { get; set; }

    }

}
