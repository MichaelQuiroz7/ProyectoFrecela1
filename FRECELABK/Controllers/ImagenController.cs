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
        public async Task<IActionResult> SubirImagen([FromForm] UploadImagenRequest1 request)
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


        [HttpPost("SubirImagenes")]
        public async Task<IActionResult> SubirImagenes([FromForm] UploadImagenRequest request)
        {
            if (request.Imagenes == null || !request.Imagenes.Any())
            {
                return BadRequest("No se recibieron imágenes.");
            }

            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var resultados = new List<object>();
            foreach (var imagen in request.Imagenes)
            {
                if (imagen == null || imagen.Length == 0)
                {
                    continue; // Skip invalid images
                }

                var nombreArchivo = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                var rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                var urlImagen = $"{Request.Scheme}://{Request.Host}/uploads/{nombreArchivo}";
                var resultado = await _repository.AgregarImagenes(request.IdProducto, urlImagen);
                resultados.Add(resultado.Data);
            }

            if (!resultados.Any())
            {
                return BadRequest("Ninguna imagen válida fue procesada.");
            }

            return Ok(new
            {
                mensaje = "Imágenes subidas y registradas correctamente",
                data = resultados
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


        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseModel>> GetImagenProducto(int id)
        {
            ResponseModel response = await _repository.ObtenerImagenesPorProducto(id);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

    }
}
