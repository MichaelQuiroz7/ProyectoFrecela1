namespace FRECELABK.Models
{
    public class ClienteRequest
    {

        public string? Cedula { get; set; }
        public string? Nombres { get; set; }
        public string? Apellidos { get; set; }
        public string?  Direccion { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? Clave { get; set; }
        public string? Telefono { get; set; }

    }

    public class ApiResponse
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
    }

    public class LoginCliente
    {
        public string CorreoElectronico { get; set; }
        public string Clave { get; set; }
    }

    

}
