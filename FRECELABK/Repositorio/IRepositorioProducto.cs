using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioProducto
    {

        Task<ResponseModel> ObtenerProductos();
        Task<ResponseModel> AgregarProducto(Producto producto);
        Task<ResponseModel> EditarProducto(int idProducto, Producto producto);
        Task<ResponseModel> ModificarStock(int idProducto, int cantidad, bool aumentar);
        Task<ResponseModel> EliminarProducto(int idProducto, int idUsuario);
        Task<ResponseModel> ObtenerStockPorProducto(int idProducto);

    }
}
