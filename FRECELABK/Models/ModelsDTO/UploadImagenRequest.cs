using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Models.ModelsDTO
{
    public class UploadImagenRequest
    {

        [FromForm]
        public int IdProducto { get; set; }

        [FromForm]
        public IFormFile Imagen { get; set; }

    }
}
