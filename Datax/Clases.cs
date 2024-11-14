public class Log
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Endpoint { get; set; }
    public string Parametros { get; set; }
    public DateTime FechaConsulta { get; set; }
}





public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; }
    public string Apellido { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
    public string AuthToken { get; set; }  // Columna para guardar el token
}



public class Horario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }  // ID del estudiante
    public string Materia { get; set; }
    public string Aula { get; set; }
    public DateTime FechaHora { get; set; } // Fecha y hora de la clase
    public string Ubicacion { get; set; } // Ubicación específica en el campus
}


public class Preseleccion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }  // ID del estudiante
    public string Materia { get; set; }
    public bool Confirmada { get; set; }  // Indica si fue confirmada
    public DateTime FechaSeleccion { get; set; }
}


public class Deuda
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public bool Pagada { get; set; }
    public DateTime FechaActualizacion { get; set; } // Última actualización de deuda
}


public class Solicitud
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Tipo { get; set; }  // Tipo de solicitud
    public string Estado { get; set; }  // Estado de la solicitud (ej., "Pendiente", "Aprobada", "Rechazada")
    public DateTime FechaSolicitud { get; set; }
    public DateTime? FechaRespuesta { get; set; }
    public string Respuesta { get; set; } // Respuesta o nota sobre la solicitud
}


public class Tarea
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public bool Completada { get; set; }
}


public class Evento
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }
    public DateTime FechaEvento { get; set; }
    public string Lugar { get; set; }
}


public class Video
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Url { get; set; }
    public DateTime FechaPublicacion { get; set; }
}



public class LoginData
{
    public string Username { get; set; }
    public string Password { get; set; }
}


public class ServerResult<T>
{
    public bool Success { get; set; }      // Indicates if the operation was successful
    public string Message { get; set; }     // Custom message
    public T Data { get; set; }             // Result data, e.g., the token or user info
    public string Error { get; set; }       // Error message if any (optional)

    public ServerResult(bool success, string message, T data = default, string error = null)
    {
        Success = success;
        Message = message;
        Data = data;
        Error = error;
    }
}


