using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Noticia> Noticias { get; set; }
    public DbSet<Horario> Horarios { get; set; }
    public DbSet<Preseleccion> Preselecciones { get; set; }
    public DbSet<Deuda> Deudas { get; set; }
    public DbSet<Solicitud> Solicitudes { get; set; }
    public DbSet<Tarea> Tareas { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Video> Videos { get; set; }
    public DbSet<Log> Logs { get; set; }
}
