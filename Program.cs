using System;
using RaceDay.API.Data;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.Services.Interfaces;
using RaceDay.API.Services.Implementations;
using RaceDay.API.DTOs;

var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Services
builder.Services.AddScoped<IAuthService, AuthService>();

// 3. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 4. Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".RaceDay.Session";
});

// 5. CORS (for Part 3 MVC)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7001", "http://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 6. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "API for the RaceDay event management system"
    });
});

var app = builder.Build();

// 7. Middleware pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowMVC");
app.UseSession();
app.UseAuthorization();
app.MapControllers();

// 8. Auto-apply migrations & seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
    NewMethod(db);
}

app.Run();

static void NewMethod(RaceDayDbContext db)
{
    db.Database.Migrate();
}