using FRECELABK.Models;
using Microsoft.EntityFrameworkCore;

namespace FRECELABK.Repositorio
{
    public interface IRepositorioImagen
    {
        Task<ResponseModel> ObtenerImagenes();
        Task<ResponseModel> AgregarImagen(Imagen imagen);
        Task<ResponseModel> EliminarImagen(int idImagen);
    }
}
