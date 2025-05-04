using FRECELABK.Models;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioAlerta : IRepositorioAlerta
    {
        private readonly string? cadenaConexion;
        public RepositorioAlerta(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }

        #region Obtener Alertas
        public async Task<ResponseModel> ObtenerAlertas()
        {
            ResponseModel response = new ResponseModel();
            List<Alerta> alertas = new List<Alerta>();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT id_producto, nombre_producto, mensaje, activo FROM alerta";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Alerta alerta = new Alerta
                                {
                                    IdProducto = reader.GetInt32("id_producto"),
                                    NombreProducto = reader.GetString("nombre_producto"),
                                    Mensaje = reader.GetString("mensaje"),
                                    Activo = reader.GetBoolean("activo")
                                };
                                alertas.Add(alerta);
                            }
                        }
                    }
                    response.Message = alertas.Count > 0 ? "Alertas obtenidas correctamente" : "No se encontraron alertas";
                    response.Code = ResponseType.Success;
                    response.Data = alertas;
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


        #region Agregar Alerta
        public async Task<ResponseModel> AgregarAlerta(Alerta alerta)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO alerta (id_producto, nombre_producto, mensaje, activo) VALUES (@idProducto, @nombreProducto, @mensaje, @activo)";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", alerta.IdProducto);
                        command.Parameters.AddWithValue("@nombreProducto", alerta.NombreProducto);
                        command.Parameters.AddWithValue("@mensaje", alerta.Mensaje);
                        command.Parameters.AddWithValue("@activo", alerta.Activo);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = "Alerta agregada correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = alerta;
                        }
                        else
                        {
                            response.Message = "No se pudo agregar la alerta";
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


        #region Cambiar Estado Alerta
        public async Task<ResponseModel> CambiarEstadoAlerta(int idProducto, string nombreProducto, string mensaje)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    // Verificar si ya existe una alerta para este producto con el mismo mensaje
                    string selectQuery = "SELECT activo FROM alerta WHERE id_producto = @idProducto AND mensaje = @mensaje";
                    bool? estadoActual = null;
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@idProducto", idProducto);
                        selectCommand.Parameters.AddWithValue("@mensaje", mensaje);
                        var result = await selectCommand.ExecuteScalarAsync();
                        if (result != null)
                        {
                            estadoActual = Convert.ToBoolean(result);
                        }
                    }

                    if (estadoActual.HasValue)
                    {
                        // Si la alerta existe, cambiar su estado
                        bool nuevoEstado = !estadoActual.Value; // Cambiar de activo a inactivo o viceversa
                        string updateQuery = "UPDATE alerta SET activo = @activo WHERE id_producto = @idProducto AND mensaje = @mensaje";
                        using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@activo", nuevoEstado);
                            updateCommand.Parameters.AddWithValue("@idProducto", idProducto);
                            updateCommand.Parameters.AddWithValue("@mensaje", mensaje);

                            int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                            if (rowsAffected > 0)
                            {
                                response.Message = nuevoEstado ? "Alerta activada correctamente" : "Alerta desactivada correctamente";
                                response.Code = ResponseType.Success;
                                response.Data = new { IdProducto = idProducto, Mensaje = mensaje, Activo = nuevoEstado };
                            }
                            else
                            {
                                response.Message = "No se pudo actualizar el estado de la alerta";
                                response.Code = ResponseType.Error;
                                response.Data = null;
                            }
                        }
                    }
                    else
                    {
                        // Si no existe, agregar una nueva alerta con estado activo
                        Alerta nuevaAlerta = new Alerta
                        {
                            IdProducto = idProducto,
                            NombreProducto = nombreProducto,
                            Mensaje = mensaje,
                            Activo = true
                        };
                        return await AgregarAlerta(nuevaAlerta);
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


    }
}
