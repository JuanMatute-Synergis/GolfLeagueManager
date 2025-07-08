using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
using GolfLeagueManager.Helpers;
using GolfLeagueManager.Services;
using GolfLeagueManager.Middleware;
using GolfLeagueManager.Data;

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
            // Add tenant services
            builder.Services.AddSingleton<ITenantService, TenantService>();
            builder.Services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();

            // Register EF Core with PostgreSQL - using factory for tenant-aware connections
            builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                var tenantService = serviceProvider.GetRequiredService<ITenantService>();
                var tenantId = tenantService.GetCurrentTenant();
                var connectionString = tenantService.GetConnectionString(tenantId);
                options.UseNpgsql(connectionString);
            });

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
            builder.Services.AddScoped<DataSeeder>(); builder.Services.AddScoped<ScorecardService>();
            builder.Services.AddScoped<MatchPlayService>();
            builder.Services.AddScoped<MatchPlayScoringService>();
            builder.Services.AddScoped<PdfScorecardService>(); builder.Services.AddScoped<AverageScoreService>();
            builder.Services.AddScoped<HandicapService>(); builder.Services.AddScoped<LeagueSettingsService>();
            builder.Services.AddScoped<ILeagueRulesService, LeagueRulesService>();
            builder.Services.AddScoped<PlayerSeasonStatsService>();
            builder.Services.AddScoped<ScoreImportService>();
            builder.Services.AddScoped<JsonImportService>();
            builder.Services.AddScoped<DatabaseCleanupService>();

            // Check if authorization should be disabled for debugging
            var disableAuth = builder.Configuration.GetValue<bool>("Debug:DisableAuthorization");
            if (disableAuth && builder.Environment.IsDevelopment())
            {
                Console.WriteLine("⚠️  WARNING: Authorization is DISABLED for debugging purposes!");
            }

            // Add controllers with JSON options
            builder.Services.AddControllers(options =>
            {
                // Only add authorization filter if not disabled for debugging
                if (!disableAuth || !builder.Environment.IsDevelopment())
                {
                    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter());
                }
            })
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
                });

            // Add CORS - Update to support multiple subdomains
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowTenantApps", policy =>
                {
                    policy.WithOrigins(
                            "http://localhost:4200",
                            "https://*.golfleaguemanager.app",
                            "http://localhost:4500"
                          )
                          .SetIsOriginAllowedToAllowWildcardSubdomains()
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials(); // Allow cookies to be sent
                });
            });
            // Add JWT authentication
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var publicKeyPath = builder.Configuration["Jwt:PublicKeyPath"] ?? "jwt_public_key.pem";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = RsaKeyHelper.GetPublicKey(publicKeyPath),
                    RequireSignedTokens = true,
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSha256 }
                };

                // Configure to read JWT from cookies instead of Authorization header
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        // Try to get token from cookie first
                        if (context.Request.Cookies.ContainsKey("golf_jwt_token"))
                        {
                            context.Token = context.Request.Cookies["golf_jwt_token"];
                        }
                        // Fallback to Authorization header for backward compatibility
                        else if (context.Request.Headers.ContainsKey("Authorization"))
                        {
                            var authHeader = context.Request.Headers["Authorization"].ToString();
                            if (authHeader.StartsWith("Bearer "))
                            {
                                context.Token = authHeader.Substring("Bearer ".Length).Trim();
                            }
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                        if (context.Exception.InnerException != null)
                            Console.WriteLine($"Inner exception: {context.Exception.InnerException.Message}");
                        return Task.CompletedTask;
                    }
                };
            });            // Enable PII logging for debugging JWT issues (ONLY in development)
            if (builder.Environment.IsDevelopment())
            {
                Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }            // Use tenant middleware BEFORE CORS
            app.UseMiddleware<TenantMiddleware>();

            // Use CORS
            app.UseCors("AllowTenantApps");

            app.UseHttpsRedirection();
            app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

            // Check if authorization should be disabled for debugging (get value from app config)
            var appDisableAuth = app.Configuration.GetValue<bool>("Debug:DisableAuthorization");

            // Only use authentication/authorization if not disabled for debugging
            if (!appDisableAuth || !app.Environment.IsDevelopment())
            {
                app.UseAuthentication();
                app.UseAuthorization();
            }
            else if (app.Environment.IsDevelopment())
            {
                Console.WriteLine("⚠️  WARNING: Authentication and Authorization middleware DISABLED for debugging!");
            }

            app.MapControllers();

            // Serve Angular index.html for client-side routes
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}
