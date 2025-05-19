using FRECELABK.Models;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioImagen
    {
        Task<ResponseModel> ObtenerImagenes();
        Task<ResponseModel> ObtenerImagenesPorProducto(int idProducto);
        Task<ResponseModel> AgregarImagen(int idProducto, string image);
        Task<ResponseModel> EliminarImagen(int idImagen);
    }
}
