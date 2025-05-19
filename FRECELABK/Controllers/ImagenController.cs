using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagenController : ControllerBase
    {

        private readonly IRepositorioImagen _repository;

        public ImagenController(IRepositorioImagen repositorioImagen, IConfiguration conf)
        {
            _repository = repositorioImagen;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetImagenes()
        {
            var response = await _repository.ObtenerImagenes();
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }


        [HttpPost("SubirImagen")]
        public async Task<IActionResult> SubirImagen([FromForm] UploadImagenRequest request)
        {
            if (request.Imagen == null || request.Imagen.Length == 0)
            {
                return BadRequest("No se recibió ninguna imagen.");
            }

            var nombreArchivo = $"{Guid.NewGuid()}_{Path.GetFileName(request.Imagen.FileName)}";
            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await request.Imagen.CopyToAsync(stream);
            }

            var urlImagen = $"{Request.Scheme}://{Request.Host}/uploads/{nombreArchivo}";

            var resultado = await _repository.AgregarImagen(request.IdProducto, urlImagen);


                return Ok(new
                {
                    mensaje = "Imagen subida y registrada correctamente",
                    data = resultado.Data
                });
           

        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel>> DeleteImagen(int id)
        {
            var response = await _repository.EliminarImagen(id);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

    }
}
