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
            var response = await _repository.ObtenerProductos();
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel>> AgregarProducto(Producto producto)
        {
            var response = await _repository.AgregarProducto(producto);
            if (response.Code == ResponseType.Success)
            {
                return CreatedAtAction(nameof(GetProductos), new { id = ((Producto)response.Data).IdProducto }, response);
            }
            return StatusCode(500, response);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseModel>> EditarProducto(int id, Producto producto)
        {
            var response = await _repository.EditarProducto(id, producto);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpPatch("{id}/stock")]
        public async Task<ActionResult<ResponseModel>> ModificarStock(int id, [FromQuery] bool aumentar, [FromQuery] int cantidad)
        {
            var response = await _repository.ModificarStock(id, cantidad, aumentar);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseModel>> EliminarProducto(int id, [FromQuery] int idUsuario)
        {
            var response = await _repository.EliminarProducto(id, idUsuario);
            if (response.Code == ResponseType.Success)
            {
                return Ok(response);
            }
            return StatusCode(500, response);
        }

    }
}
