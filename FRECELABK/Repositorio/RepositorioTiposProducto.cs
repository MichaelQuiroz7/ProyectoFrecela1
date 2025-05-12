using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioTiposProducto : IRepositorioTiposProduct
    {

        private readonly string? cadenaConexion;
        ResponseModel response = new ResponseModel();

        public RepositorioTiposProducto(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        #region ObtenerTiposProduct

        public async Task<ResponseModel> ObtenerTiposProduct()
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM tipo_producto";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    List<TipoProductoDTO> tiposProducto = new List<TipoProductoDTO>();
                    while (await reader.ReadAsync())
                    {
                        TipoProductoDTO tipoProducto = new TipoProductoDTO
                        {
                            IdTipoProducto = reader.GetInt32("id_tipo_producto"),
                            NombreTipo = reader.GetString("nombre_tipo")
                        };
                        tiposProducto.Add(tipoProducto);
                    }
                    response.Code = ResponseType.Success;
                    response.Message = "Tipos de producto obtenidos correctamente";
                    response.Data = tiposProducto;
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al obtener los tipos de producto: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion


        #region AgregarTipoProducto

        public async Task<ResponseModel> AgregarTipoProduct(TipoProducto tipoProducto)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO tipo_producto (nombre_tipo) VALUES (@nombre_tipo)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nombre_tipo", tipoProducto.NombreTipo);
                    await command.ExecuteNonQueryAsync();
                    response.Code = ResponseType.Success;
                    response.Message = "Tipo de producto agregado correctamente";
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al agregar el tipo de producto: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion


        #region EliminarTipoProducto

        public async Task<ResponseModel> EliminarTipoProduct(int idtipo)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM tipo_producto WHERE id_tipo_producto = @id_tipo_producto";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_tipo_producto", idtipo);
                    await command.ExecuteNonQueryAsync();
                    response.Code = ResponseType.Success;
                    response.Message = "Tipo de producto eliminado correctamente";
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al eliminar el tipo de producto: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion


        #region ObtenerSubTiposProduct

        public async Task<ResponseModel> ObtenerTiposSubproduct()
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT * FROM tipo_subproducto";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync();
                    List<TipoSubproductoDTO> subTiposProducto = new List<TipoSubproductoDTO>();
                    while (await reader.ReadAsync())
                    {
                        TipoSubproductoDTO subTipoProducto = new TipoSubproductoDTO
                        {
                            IdTipoSubproducto = reader.GetInt32("id_tipo_subproducto"),
                            NombreSubtipo = reader.GetString("nombre_subtipo")
                        };
                        subTiposProducto.Add(subTipoProducto);
                    }
                    response.Code = ResponseType.Success;
                    response.Message = "Tipos de Subproducto obtenidos correctamente";
                    response.Data = subTiposProducto;
                }
                catch (Exception ex)
                {
                    response.Code = ResponseType.Error;
                    response.Message = "Error al obtener los subtipos de producto: " + ex.Message;
                }
                finally
                {
                    await connection.CloseAsync();
                }
                return response;
            }
        }

        #endregion




    }
}
