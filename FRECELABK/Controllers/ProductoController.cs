using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using FRECELABK.Repositorio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IRepositorioProducto _repository;

        public ProductoController(IRepositorioProducto repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel>> GetProductos()
        {
            ResponseModel response = await _repository.ObtenerProductos();

            return Ok(response);

        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> AgregarProducto(ProductoDTO producto)
        {
            ResponseModel response = await _repository.AgregarProducto(producto);

            return CreatedAtAction(nameof(GetProductos), new { id = ((Producto)response.Data).IdProducto }, response);

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseModel>> EditarProducto(int id, ProductoDTO producto)
        {
            ResponseModel response = await _repository.EditarProducto(id, producto);
                return Ok(response);
        }

        [HttpPut("/stock")]
        public async Task<ActionResult<ResponseModel>> ModificarStock(ProductoStock producto)
        {
            ResponseModel response = await _repository.ModificarStock(producto);
            return Ok(response);
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel>> EliminarProducto(int id, [FromQuery] int idrol)
        {
            ResponseModel response = await _repository.EliminarProducto(id, idrol);
                return Ok(response);
        }

    }
}
