using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MiPresupuesto.Api.Data;
using MiPresupuesto.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Base de datos PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Servicios
builder.Services.AddScoped<IJwtService, JwtService>();

// 3. Autenticación con JWT Bearer
var jwtKey = builder.Configuration["Jwt:Key"] ?? "MiPresupuestoSuperClaveSecretaDeAltaSeguridad2026_Key123456";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "MiPresupuestoApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "MiPresupuestoMobile";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

// 4. Controladores y CORS
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 5. Swagger con soporte Bearer Token
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MiPresupuesto API",
        Version = "v1",
        Description = "API de sincronización en la nube para MiPresupuesto"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese su token JWT (no es necesario anteponer 'Bearer ', Swagger lo agrega automáticamente)"
    };

    options.AddSecurityDefinition("Bearer", securityScheme);

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:EnableInProduction", true))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MiPresupuesto API v1");
    });
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
