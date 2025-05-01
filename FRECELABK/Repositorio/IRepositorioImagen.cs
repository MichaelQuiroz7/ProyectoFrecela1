using FRECELABK.Models;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioImagen
    {
        Task<ResponseModel> ObtenerImagenes();
        Task<ResponseModel> ObtenerImagenesPorProducto(int idProducto);
        Task<ResponseModel> AgregarImagen(Imagen imagen);
        Task<ResponseModel> EliminarImagen(int idImagen);
    }
}
