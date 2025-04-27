using FRECELABK.Models;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioProducto : IRepositorioProducto
    {
        private readonly string? cadenaConexion;
        ResponseModel response = new ResponseModel();

        public RepositorioProducto(IConfiguration conf)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
        }


        #region ObtenerProductos
        public async Task<ResponseModel> ObtenerProductos()
        {
            ResponseModel response = new ResponseModel();
            List<Producto> productos = new List<Producto>();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT id_producto, nombre, precio, descripcion, stock FROM producto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Producto producto = new Producto
                                {
                                    IdProducto = reader.GetInt32(reader.GetOrdinal("id_producto")),
                                    Nombre = reader.GetString(reader.GetOrdinal("nombre")),
                                    Precio = reader.GetDecimal(reader.GetOrdinal("precio")),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion")),
                                    Stock = reader.GetInt32(reader.GetOrdinal("stock"))
                                };
                                productos.Add(producto);
                            }
                        }
                    }
                    response.Message = "Productos obtenidos correctamente";
                    response.Code = ResponseType.Success;
                    response.Data = productos;
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



        #region Agregar Producto

        public async Task<ResponseModel> AgregarProducto(Producto producto)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "INSERT INTO producto (nombre, precio, descripcion, stock) VALUES (@nombre, @precio, @descripcion, @stock); SELECT LAST_INSERT_ID();";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@precio", producto.Precio);
                        command.Parameters.AddWithValue("@descripcion", producto.Descripcion as object ?? DBNull.Value);
                        command.Parameters.AddWithValue("@stock", producto.Stock);

                        int idProducto = Convert.ToInt32(await command.ExecuteScalarAsync());
                        producto.IdProducto = idProducto;

                        response.Message = "Producto agregado correctamente";
                        response.Code = ResponseType.Success;
                        response.Data = producto;
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



        #region Editar Producto

        public async Task<ResponseModel> EditarProducto(int idProducto, Producto producto)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "UPDATE producto SET nombre = @nombre, precio = @precio, descripcion = @descripcion, stock = @stock WHERE id_producto = @idProducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@precio", producto.Precio);
                        command.Parameters.AddWithValue("@descripcion", producto.Descripcion as object ?? DBNull.Value);
                        command.Parameters.AddWithValue("@stock", producto.Stock);
                        command.Parameters.AddWithValue("@idProducto", idProducto);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = "Producto actualizado correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = producto;
                        }
                        else
                        {
                            response.Message = "Producto no encontrado";
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



        #region Modificar Stock

        public async Task<ResponseModel> ModificarStock(int idProducto, int cantidad, bool aumentar)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    // Obtener el stock actual
                    string selectQuery = "SELECT stock FROM producto WHERE id_producto = @idProducto";
                    int stockActual;
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@idProducto", idProducto);
                        var result = await selectCommand.ExecuteScalarAsync();
                        if (result == null)
                        {
                            response.Message = "Producto no encontrado";
                            response.Code = ResponseType.Error;
                            response.Data = null;
                            return response;
                        }
                        stockActual = Convert.ToInt32(result);
                    }

                    // Calcular el nuevo stock
                    int nuevoStock = aumentar ? stockActual + cantidad : stockActual - cantidad;
                    if (nuevoStock < 0)
                    {
                        response.Message = "El stock no puede ser negativo";
                        response.Code = ResponseType.Error;
                        response.Data = null;
                        return response;
                    }

                    // Actualizar el stock
                    string updateQuery = "UPDATE producto SET stock = @nuevoStock WHERE id_producto = @idProducto";
                    using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@nuevoStock", nuevoStock);
                        updateCommand.Parameters.AddWithValue("@idProducto", idProducto);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = aumentar ? "Stock aumentado correctamente" : "Stock disminuido correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = new { IdProducto = idProducto, NuevoStock = nuevoStock };
                        }
                        else
                        {
                            response.Message = "Producto no encontrado";
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



        #region Eliminar Producto

        public async Task<ResponseModel> EliminarProducto(int idProducto, int rolUsuario)
        {
            ResponseModel response = new ResponseModel();
            if (rolUsuario != 1)
            {
                response.Message = "No tienes permisos para eliminar productos";
                response.Code = ResponseType.Error;
                response.Data = null;
                return response;
            }

            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "DELETE FROM producto WHERE id_producto = @idProducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", idProducto);

                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            response.Message = "Producto eliminado correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = null;
                        }
                        else
                        {
                            response.Message = "Producto no encontrado";
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




    }

}
