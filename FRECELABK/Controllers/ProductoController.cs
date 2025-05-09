using FRECELABK.Models;
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
        public async Task<ActionResult<ResponseModel>> AgregarProducto(Producto producto)
        {
            ResponseModel response = await _repository.AgregarProducto(producto);

            return CreatedAtAction(nameof(GetProductos), new { id = ((Producto)response.Data).IdProducto }, response);

        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseModel>> EditarProducto(int id, Producto producto)
        {
            ResponseModel response = await _repository.EditarProducto(id, producto);
                return Ok(response);
        }

        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<ResponseModel>> ModificarStock(int id, [FromQuery] bool aumentar, [FromQuery] int cantidad)
        {
            ResponseModel response = await _repository.ModificarStock(id, cantidad, aumentar);          
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
