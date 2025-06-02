namespace FRECELABK.Models.ModelsDTO
{
    public class EmpleadoDTO
    {
        public string Cedula { get; set; }
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Genero { get; set; }
        public int Edad { get; set; } 
        public string Telefono { get; set; }
    }

    public class EmpleadoRequest
    {
      
        public string Nombres { get; set; }
        public string Apellidos { get; set; }
        public string Cedula { get; set; }
        public DateOnly FechaNacimiento { get; set; }
        public string Genero { get; set; }
        //public int Id_Rol { get; set; }
        public string Telefono { get; set; }
        public string contrasenia { get; set; }

    }

    public class descuentoEmpleado { 

        public string Cedula { get; set; }
        public decimal Descuento { get; set; }
    }


}
