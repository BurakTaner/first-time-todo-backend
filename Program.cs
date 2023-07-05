using TodoBackend.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using TodoBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TodoBackend.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TodosDbContext>(a =>
        a.UseNpgsql(builder.Configuration.GetConnectionString("TodoBackend"))
    );

builder.Services.AddControllers().AddJsonOptions(a => a.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Healthcheck
builder.Services.AddHealthChecks().AddNpgSql(builder.Configuration.GetConnectionString("TodoBackend"), tags: new string[] { "db" }).AddCheck("TodoBackend", () =>
        HealthCheckResult.Healthy()
        , new string[] { "main" });

// Serilog
builder.Host.UseSerilog();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Caching
builder.Services.AddScoped<ICacheService, CacheService>();


// JWT
builder.Services.AddAuthentication(a =>
{
    a.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    a.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(a =>
    {
        byte[] key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:SecretKey").Value);

        a.SaveToken = true;
        a.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true
        };
    });
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
builder.Services.AddScoped<IJwtService, JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseCors(a =>
{
    a.AllowAnyOrigin();
    a.AllowAnyHeader();
    a.AllowAnyMethod();
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// map healthcheck
app.MapHealthChecks("/healthcheck", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
