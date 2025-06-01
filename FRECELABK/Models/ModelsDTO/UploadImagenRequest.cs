using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Models.ModelsDTO
{
    public class UploadImagenRequest
    {

        [FromForm]
        public int IdProducto { get; set; }

        [FromForm]
        public List<IFormFile> Imagenes { get; set; }

    }


    public class UploadImagenRequest1
    {

        [FromForm]
        public int IdProducto { get; set; }

        [FromForm]
        public IFormFile Imagen { get; set; }

    }


}
