using FRECELABK.Models;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioImagen : IRepositorioImagen
    {

        private readonly string? cadenaConexion;

        public RepositorioImagen(IConfiguration conf)
        {

            cadenaConexion = conf.GetConnectionString("Conexion");

        }

        #region Obtener Imagenes

        public async Task<ResponseModel> ObtenerImagenes()
        {
            ResponseModel response = new ResponseModel();
            List<Imagen> imagenes = new List<Imagen>();

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT id_imagen, id_producto, imagen FROM imagen";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Imagen imagen = new Imagen
                                {
                                    IdImagen = reader.GetInt32(reader.GetOrdinal("id_imagen")),
                                    IdProducto = reader.GetInt32(reader.GetOrdinal("id_producto")),
                                    ImagenUrl = reader.IsDBNull(reader.GetOrdinal("imagen")) ? null : reader.GetString(reader.GetOrdinal("imagen"))
                                };
                                imagenes.Add(imagen);
                            }
                        }
                    }
                    response.Message = "Imágenes obtenidas correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = imagenes;
                }
                catch (Exception ex)
                {
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }

        #endregion


        #region Agregar Imagen

        public async Task<ResponseModel> AgregarImagen(int idProducto, string image)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO imagen (id_producto, imagen) VALUES (@idProducto, @imagen); SELECT LAST_INSERT_ID();";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", idProducto);
                        command.Parameters.AddWithValue("@imagen", image);

                        var result = await command.ExecuteScalarAsync();
                        int idImagen = Convert.ToInt32(result);

                        response.Message = "Imagen agregada correctamente";
                        response.Code = ResponseType.Success;
                        response.Data = new Imagen
                        {
                            IdImagen = idImagen,
                            IdProducto = idProducto,
                            ImagenUrl = image
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.Code = ResponseType.Error;
            }

            return response;
        }

        public async Task<ResponseModel> AgregarImagenes(int idProducto, string image)
        {
            ResponseModel response = new ResponseModel();

            try
            {
                using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO imagen (id_producto, imagen) VALUES (@idProducto, @imagen); SELECT LAST_INSERT_ID();";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", idProducto);
                        command.Parameters.AddWithValue("@imagen", image);

                        var result = await command.ExecuteScalarAsync();
                        int idImagen = Convert.ToInt32(result);

                        response.Message = "Imagen agregada correctamente";
                        response.Code = ResponseType.Success;
                        response.Data = new Imagen
                        {
                            IdImagen = idImagen,
                            IdProducto = idProducto,
                            ImagenUrl = image
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                response.Message = "Error: " + ex.Message;
                response.Code = ResponseType.Error;
            }

            return response;
        }

        #endregion


        #region Eliminar Imagen

        public async Task<ResponseModel> EliminarImagen(int idImagen)
        {
            ResponseModel response = new ResponseModel();

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();

                    // Verificar si la imagen existe
                    string checkQuery = "SELECT COUNT(*) FROM imagen WHERE id_imagen = @idImagen";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@idImagen", idImagen);
                        long count = (long)await checkCommand.ExecuteScalarAsync();
                        if (count == 0)
                        {
                            response.Message = "La imagen no existe";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                    }

                    // Eliminar la imagen
                    string deleteQuery = "DELETE FROM imagen WHERE id_imagen = @idImagen";
                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@idImagen", idImagen);
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            response.Message = "Imagen eliminada correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = idImagen;
                        }
                        else
                        {
                            response.Message = "No se pudo eliminar la imagen";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }

        #endregion


        #region Obtener Imagenes por Producto

        public async Task<ResponseModel> ObtenerImagenesPorProducto(int idProducto)
        {
            ResponseModel response = new ResponseModel();
            List<Imagen> imagenes = new List<Imagen>();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT id_imagen, id_producto, imagen FROM imagen WHERE id_producto = @idProducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", idProducto);
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Imagen imagen = new Imagen
                                {
                                    IdImagen = reader.GetInt32("id_imagen"),
                                    IdProducto = reader.GetInt32("id_producto"),
                                    ImagenUrl = reader.GetString("imagen")
                                };
                                imagenes.Add(imagen);
                            }
                        }
                    }
                    response.Message = imagenes.Count > 0 ? "Imágenes obtenidas correctamente" : "No se encontraron imágenes para este producto";
                    response.Code = ResponseType.Success;
                    response.Data = imagenes;
                }
                catch (Exception ex)
                {
                    response.Data = null;
                    response.Code = ResponseType.Error;
                    response.Message = ex.Message;
                }
            }
            return response;
        }

        #endregion

    }
}
