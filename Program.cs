using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, ingrese el token JWT con Bearer [Token]",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// For MySQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 31))
    ));

// Add Authentication and Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Login Endpoint
app.MapPost("/login", async (AppDbContext db, [FromBody] LoginData usuario) =>
{
    var user = await db.Usuarios
        .FirstOrDefaultAsync(u => u.Username == usuario.Username && u.Password == usuario.Password);

    if (user == null){
        return Results.Ok(new ServerResult<string>(false, "Login failed", error: "Usuario o contraseña incorrectos"));
    }

    await RegistrarLog(db, user, "/login", $"username={usuario.Username}");

    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new Claim[]
        {
            new Claim("id", user.Id.ToString())
        }),
        Expires = DateTime.UtcNow.AddHours(1),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    // Guarda el token en la base de datos
    user.AuthToken = tokenString;
    await db.SaveChangesAsync();

    user.Password = "🤣😁👍";
    user.Id = 0;

    return Results.Ok(new ServerResult<Usuario>(true, "Login successful", user));
});


//informacion de usuario
app.MapGet("/info_usuario", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }
    user.Password = "🤣😁👍";
    user.Id = 0;
    return Results.Ok(new ServerResult<Usuario>(true, "Usuario cargado", user));
});


// Cambiar contraseña Endpoint
app.MapPost("/cambiar_password", async (AppDbContext db, HttpContext context, [FromBody] PasswordResetData data) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }

    if (data.NewPassword != data.ConfirmPassword)
    {
        return Results.Ok(new ServerResult<string>(false, "Error", error: "Las contraseñas no coinciden"));
    }

    if (data.OldPassword != user.Password)
    {
        return Results.Ok(new ServerResult<string>(false, "Error", error: "La contraseña actual es incorrecta"));
    }

    user.Password = data.NewPassword;
    await db.SaveChangesAsync();

    return Results.Ok(new ServerResult<string>(true, "Contraseña cambiada exitosamente"));
});


// Noticias Endpoint (requires Authorization)
app.MapGet("/noticias", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }

    // Continuar con la lógica del endpoint si el token es válido
    var urlx = "https://uasd.edu.do/apiuasd/";
    var client = new HttpClient();
    var response = await client.GetAsync(urlx);
    var content = await response.Content.ReadAsStringAsync();
    var noticias = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Noticia>>(content);

    return Results.Ok(new ServerResult<List<Noticia>>(true, "Noticias cargadas", noticias));
});


// Eventos Endpoint
app.MapGet("/eventos", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }
    var eventos = await db.Eventos.ToListAsync();
    return Results.Ok(eventos);
});

// Videos Endpoint
app.MapGet("/videos", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }
    var videos = await db.Videos.ToListAsync();
    return Results.Ok(videos);
});

//Deudas Endpoint
app.MapGet("/deudas", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }
    var deudas = await db.Deudas.ToListAsync();
    return Results.Ok(deudas);
});


// Horarios Endpoint
app.MapGet("/horarios", async (AppDbContext db, HttpContext context) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var user = await db.Usuarios.FirstOrDefaultAsync(u => u.AuthToken == token);
    if (user == null || user.AuthToken != token)
    {
        return Results.Ok(new ServerResult<string>(false, "Unauthorized", error: "Invalid token"));
    }
    var horarios = await db.Horarios.ToListAsync();
    return Results.Ok(horarios);
});

app.Run();

async Task RegistrarLog(AppDbContext db, Usuario user, string endpoint, string parametros)
{
    var log = new Log
    {
        UsuarioId = user.Id,
        Endpoint = endpoint,
        Parametros = parametros,
        FechaConsulta = DateTime.Now
    };
    db.Logs.Add(log);
    await db.SaveChangesAsync();
}


public class PasswordResetData
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }

    public string ConfirmPassword { get; set; }
}