using FRECELABK.Models;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiposProductoController : ControllerBase
    {

        private readonly IRepositorioTiposProduct _repository;

        public TiposProductoController(IRepositorioTiposProduct repositorio)
        {
            _repository = repositorio;
        }


        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetTiposProduct()
        {
            ResponseModel response = await _repository.ObtenerTiposProduct();

            return Ok(response);

        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> AgregarTipoProducto(TipoProducto tipoProducto)
        {

            ResponseModel response = await _repository.AgregarTipoProduct(tipoProducto);
            return CreatedAtAction(nameof(GetTiposProduct), new { id = ((TipoProducto)response.Data).IdTipoProducto }, response);

        }

    }
}
