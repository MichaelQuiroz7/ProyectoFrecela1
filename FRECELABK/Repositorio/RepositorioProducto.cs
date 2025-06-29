using FRECELABK.Models;
using FRECELABK.Models.ModelsDTO;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;

namespace FRECELABK.Repositorio
{
    public class RepositorioProducto : IRepositorioProducto
    {
        private readonly string? cadenaConexion;
        private readonly IRepositorioAlerta _repository;
        private const int UMBRAL_STOCK_BAJO = 20;
        ResponseModel response = new ResponseModel();

        public RepositorioProducto(IConfiguration conf, IRepositorioAlerta repositorioAlerta)
        {
            cadenaConexion = conf.GetConnectionString("Conexion");
            _repository = repositorioAlerta;
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
                    string query = @"
                    SELECT 
                        p.id_producto, p.nombre, p.precio, p.descripcion, p.stock, 
                        p.id_tipo_producto, p.id_tipo_subproducto,
                        tp.nombre_tipo, tsp.nombre_subtipo
                    FROM producto p
                    JOIN tipo_producto tp ON p.id_tipo_producto = tp.id_tipo_producto
                    JOIN tipo_subproducto tsp ON p.id_tipo_subproducto = tsp.id_tipo_subproducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Producto producto = new Producto
                                {
                                    IdProducto = reader.GetInt32("id_producto"),
                                    Nombre = reader.GetString("nombre"),
                                    Precio = reader.GetDecimal("precio"),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString("descripcion"),
                                    Stock = reader.GetInt32("stock"),
                                    IdTipoProducto = reader.GetInt32("id_tipo_producto"),
                                    IdTipoSubproducto = reader.GetInt32("id_tipo_subproducto")
                                };
                                productos.Add(producto);
                            }
                        }
                    }

                    // Opcional: Obtener imágenes y alertas para cada producto
                    foreach (var producto in productos)
                    {
                        // Obtener imágenes
                        string imagenQuery = "SELECT id_imagen, id_producto, imagen FROM imagen WHERE id_producto = @idProducto";
                        using (MySqlCommand imagenCommand = new MySqlCommand(imagenQuery, connection))
                        {
                            imagenCommand.Parameters.AddWithValue("@idProducto", producto.IdProducto);
                            using (MySqlDataReader imagenReader = (MySqlDataReader)await imagenCommand.ExecuteReaderAsync())
                            {
                                while (await imagenReader.ReadAsync())
                                {
                                    producto.Imagens.Add(new Imagen
                                    {
                                        IdImagen = imagenReader.GetInt32("id_imagen"),
                                        IdProducto = imagenReader.GetInt32("id_producto"),
                                        ImagenUrl = imagenReader.GetString("imagen")
                                    });
                                }
                            }
                        }

                        // Obtener alertas
                        string alertaQuery = "SELECT id_producto, nombre_producto, mensaje FROM alerta WHERE id_producto = @idProducto";
                        using (MySqlCommand alertaCommand = new MySqlCommand(alertaQuery, connection))
                        {
                            alertaCommand.Parameters.AddWithValue("@idProducto", producto.IdProducto);
                            using (MySqlDataReader alertaReader = (MySqlDataReader)await alertaCommand.ExecuteReaderAsync())
                            {
                                while (await alertaReader.ReadAsync())
                                {
                                    producto.Alertas.Add(new Alerta
                                    {
                                        IdProducto = alertaReader.GetInt32("id_producto"),
                                        NombreProducto = alertaReader.GetString("nombre_producto"),
                                        Mensaje = alertaReader.GetString("mensaje")
                                    });
                                }
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

        public async Task<ResponseModel> AgregarProducto(ProductoDTO producto)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    int idProducto;
                    string query = @"
                        INSERT INTO producto (nombre, precio, descripcion, stock, id_tipo_producto, id_tipo_subproducto) 
                        VALUES (@nombre, @precio, @descripcion, @stock, @idTipoProducto, @idTipoSubproducto);
                        SELECT LAST_INSERT_ID();";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@precio", producto.Precio);
                        command.Parameters.AddWithValue("@descripcion", producto.Descripcion ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@stock", producto.Stock);
                        command.Parameters.AddWithValue("@idTipoProducto", producto.IdTipoProducto);
                        command.Parameters.AddWithValue("@idTipoSubproducto", producto.IdTipoSubproducto);

                        idProducto = Convert.ToInt32(await command.ExecuteScalarAsync());

                        var productoCreado = new Producto
                        {
                            IdProducto = idProducto,
                            Nombre = producto.Nombre,
                            Precio = producto.Precio,
                            Descripcion = producto.Descripcion,
                            Stock = producto.Stock,
                            IdTipoProducto = producto.IdTipoProducto,
                            IdTipoSubproducto = producto.IdTipoSubproducto
                        };

                        response.Message = "Producto agregado correctamente";
                        response.Code = ResponseType.Success;
                        response.Data = productoCreado;
                    
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

        public async Task<ResponseModel> EditarProducto(int idProducto, ProductoDTO producto)
        {
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = @"
                    UPDATE producto 
                    SET nombre = @nombre, precio = @precio, descripcion = @descripcion, 
                        stock = @stock, id_tipo_producto = @idTipoProducto, id_tipo_subproducto = @idTipoSubproducto 
                    WHERE id_producto = @idProducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nombre", producto.Nombre);
                        command.Parameters.AddWithValue("@precio", producto.Precio);
                        command.Parameters.AddWithValue("@descripcion", producto.Descripcion as object ?? DBNull.Value);
                        command.Parameters.AddWithValue("@stock", producto.Stock);
                        command.Parameters.AddWithValue("@idTipoProducto", producto.IdTipoProducto);
                        command.Parameters.AddWithValue("@idTipoSubproducto", producto.IdTipoSubproducto);
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

        public async Task<ResponseModel> ModificarStock(ProductoStock producto)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    // Obtener el stock actual
                    string selectQuery = "SELECT stock, nombre FROM producto WHERE id_producto = @idProducto";
                    int stockActual;
                    string nombreProducto;
                    using (MySqlCommand selectCommand = new MySqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@idProducto", producto.IdProducto);
                        using (MySqlDataReader reader = (MySqlDataReader)await selectCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                stockActual = reader.GetInt32("stock");
                                nombreProducto = reader.GetString("nombre");
                            }
                            else
                            {
                                response.Message = "Producto no encontrado";
                                response.Code = ResponseType.Error;
                                response.Data = null;
                                return response;
                            }
                        }
                    }

                    // Calcular el nuevo stock
                    int nuevoStock = producto.aumentar ? stockActual + producto.cantidad : stockActual - producto.cantidad;
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
                        updateCommand.Parameters.AddWithValue("@idProducto", producto.IdProducto);

                        int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                        if (rowsAffected > 0)
                        {
                            // Gestionar alertas basadas en el nuevo stock
                            string mensajeAlerta = nuevoStock <= UMBRAL_STOCK_BAJO
                                ? $"Stock bajo: solo quedan {nuevoStock} unidades."
                                : $"Stock actualizado: ahora hay {nuevoStock} unidades.";

                            if (nuevoStock <= UMBRAL_STOCK_BAJO)
                            {
                                // Si el stock está por debajo del umbral, activar o crear una alerta
                                await _repository.CambiarEstadoAlerta(producto.IdProducto, nombreProducto, mensajeAlerta);
                            }
                            else
                            {
                                // Si el stock está por encima del umbral, desactivar la alerta si existe
                                await _repository.CambiarEstadoAlerta(producto.IdProducto, nombreProducto, mensajeAlerta);
                            }

                            response.Message = producto.aumentar ? "Stock aumentado correctamente" : "Stock disminuido correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = new { IdProducto = producto.IdProducto, NuevoStock = nuevoStock };
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


        #region Obtener Stock por Producto

        public async Task<ResponseModel> ObtenerStockPorProducto(int idProducto)
        {
            ResponseModel response = new ResponseModel();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = "SELECT stock FROM producto WHERE id_producto = @idProducto";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@idProducto", idProducto);
                        var result = await command.ExecuteScalarAsync();
                        if (result != null)
                        {
                            int stock = Convert.ToInt32(result);
                            response.Message = "Stock obtenido correctamente";
                            response.Code = ResponseType.Success;
                            response.Data = stock;
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




        #region Obtener productos stock bajo

        
        public async Task<ResponseModel> ObtenerProductosBajoStock()
        {
            ResponseModel response = new ResponseModel();
            List<Producto> productos = new List<Producto>();
            using (MySqlConnection connection = new MySqlConnection(cadenaConexion))
            {
                try
                {
                    await connection.OpenAsync();
                    string query = @"
            SELECT 
                p.id_producto, p.nombre, p.precio, p.descripcion, p.stock, 
                p.id_tipo_producto, p.id_tipo_subproducto,
                tp.nombre_tipo, tsp.nombre_subtipo
            FROM producto p
            JOIN tipo_producto tp ON p.id_tipo_producto = tp.id_tipo_producto
            JOIN tipo_subproducto tsp ON p.id_tipo_subproducto = tsp.id_tipo_subproducto
            WHERE p.stock < 40";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = (MySqlDataReader)await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                Producto producto = new Producto
                                {
                                    IdProducto = reader.GetInt32("id_producto"),
                                    Nombre = reader.GetString("nombre"),
                                    Precio = reader.GetDecimal("precio"),
                                    Descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString("descripcion"),
                                    Stock = reader.GetInt32("stock"),
                                    IdTipoProducto = reader.GetInt32("id_tipo_producto"),
                                    IdTipoSubproducto = reader.GetInt32("id_tipo_subproducto")
                                };
                                productos.Add(producto);
                            }
                        }
                    }

                    // Obtener imágenes y alertas para cada producto
                    foreach (var producto in productos)
                    {
                        // Obtener imágenes
                        string imagenQuery = "SELECT id_imagen, id_producto, imagen FROM imagen WHERE id_producto = @idProducto";
                        using (MySqlCommand imagenCommand = new MySqlCommand(imagenQuery, connection))
                        {
                            imagenCommand.Parameters.AddWithValue("@idProducto", producto.IdProducto);
                            using (MySqlDataReader imagenReader = (MySqlDataReader)await imagenCommand.ExecuteReaderAsync())
                            {
                                while (await imagenReader.ReadAsync())
                                {
                                    producto.Imagens.Add(new Imagen
                                    {
                                        IdImagen = imagenReader.GetInt32("id_imagen"),
                                        IdProducto = imagenReader.GetInt32("id_producto"),
                                        ImagenUrl = imagenReader.GetString("imagen")
                                    });
                                }
                            }
                        }
                    }

                    response.Message = "Productos con stock bajo obtenidos correctamente";
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


    }

}
