using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagenController : ControllerBase
    {

        private readonly IRepositorioImagen _repository;

        public ImagenController(IRepositorioImagen repositorioImagen)
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


        [HttpPost]
        public async Task<ActionResult<ResponseModel>> PostImagen(Imagen imagen)
        {
            var response = await _repository.AgregarImagen(imagen);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
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
