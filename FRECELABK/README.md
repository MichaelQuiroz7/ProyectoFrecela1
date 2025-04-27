# FRECELABK

Una API RESTful para gestionar productos y empleados de una empresa.

## Tecnologías
- .NET Core
- MySQL
- Swagger para documentación de API

## Requisitos previos
- Tener MySQL instalado y configurado.
- Crear una base de datos llamada `FrecelaDatabase` y ejecutar los scripts SQL proporcionados.

## Instalación
1. Clona el repositorio: `git clone https://github.com/tu-usuario/FRECELABK.git`
2. Restaura los paquetes: `dotnet restore`
3. Configura la cadena de conexión en `appsettings.json`. Reemplaza `{MYSQL_PASSWORD}` con tu contraseña de MySQL:
   ```json
   {
     "ConnectionStrings": {
       "Conexion": "Server=localhost;Database=empresa;User Id=root;Password={MYSQL_PASSWORD};"
     }
   }
   ```
4. Ejecuta la aplicación: `dotnet run`

