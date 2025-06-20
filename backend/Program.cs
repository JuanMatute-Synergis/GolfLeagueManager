using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using GolfLeagueManager.Business;
using GolfLeagueManager.Converters;

namespace GolfLeagueManager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
              // Register OpenAPI services
            builder.Services.AddOpenApi();
              // Configure JSON serialization to handle circular references
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            });
            
            // Register EF Core with PostgreSQL
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            
            // Register repositories for EF Core
            builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
            builder.Services.AddScoped<IFlightRepository, FlightRepository>();
            builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();
            builder.Services.AddScoped<IPlayerFlightAssignmentRepository, PlayerFlightAssignmentRepository>();
            builder.Services.AddScoped<IWeekRepository, WeekRepository>();
            builder.Services.AddScoped<IScoreEntryRepository, ScoreEntryRepository>();
            builder.Services.AddScoped<IMatchupRepository, MatchupRepository>();
            builder.Services.AddScoped<ICourseRepository, CourseRepository>();

            // Register business services
            builder.Services.AddScoped<PlayerService>();
            builder.Services.AddScoped<FlightService>();
            builder.Services.AddScoped<SeasonService>();
            builder.Services.AddScoped<PlayerFlightAssignmentService>();
            builder.Services.AddScoped<WeekService>();
            builder.Services.AddScoped<ScoreEntryService>();
            builder.Services.AddScoped<MatchupService>();
            builder.Services.AddScoped<CourseService>();
            builder.Services.AddScoped<DataSeeder>();
            builder.Services.AddScoped<ScorecardService>();
            builder.Services.AddScoped<MatchPlayService>();
            builder.Services.AddScoped<MatchPlayScoringService>();
            builder.Services.AddScoped<PdfScorecardService>();
            builder.Services.AddScoped<AverageScoreService>();
            builder.Services.AddScoped<ScoreImportService>();
            builder.Services.AddScoped<JsonImportService>();
            builder.Services.AddScoped<DatabaseCleanupService>();            // Add controllers with JSON options
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                });// Add CORS
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
