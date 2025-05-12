using FRECELABK.Models;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioTiposProduct
    {

        //Tipo Producto
        Task<ResponseModel> ObtenerTiposProduct();
        Task<ResponseModel> AgregarTipoProduct(TipoProducto producto);
        Task<ResponseModel> EliminarTipoProduct(int idtipo);

        //Subtipo Producto
        Task<ResponseModel> ObtenerTiposSubproduct();
        //Task<ResponseModel> AgregarTiposSubproduct(TipoProducto producto);
        //Task<ResponseModel> EliminarTiposSubproductt(int id, int idtipo);

    }
}
