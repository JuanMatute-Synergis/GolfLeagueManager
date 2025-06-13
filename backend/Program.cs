using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;

namespace GolfLeagueManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddOpenApi();
              // Register services
            builder.Services.AddScoped<PlayerService>();
            builder.Services.AddScoped<FlightService>();
            
            // Register EF Core with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Register repository for EF Core
            builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();// Add controllers
            builder.Services.AddControllers();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp", policy =>
                {
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            // Use CORS
            app.UseCors("AllowAngularApp");

            app.UseHttpsRedirection();
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

            app.MapControllers();

            // Serve Angular index.html for client-side routes
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
