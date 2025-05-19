using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioProducto
    {

        Task<ResponseModel> ObtenerProductos();
        Task<ResponseModel> AgregarProducto(ProductoDTO producto);
        Task<ResponseModel> EditarProducto(int idProducto, ProductoDTO producto);
        Task<ResponseModel> ModificarStock(ProductoStock producto);
        Task<ResponseModel> EliminarProducto(int idProducto, int idUsuario);
        Task<ResponseModel> ObtenerStockPorProducto(int idProducto);

    }
}
