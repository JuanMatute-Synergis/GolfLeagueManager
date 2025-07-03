using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GolfLeagueManager
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;

        public DataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedPlayersWithHandicapsAsync()
        {
            var handicapDataPath = Path.Combine(Directory.GetCurrentDirectory(), "scripts", "data", "handicap-data.json");
            
            if (!File.Exists(handicapDataPath))
            {
                Console.WriteLine($"Handicap data file not found at: {handicapDataPath}");
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(handicapDataPath);
            var playerHandicapData = JsonSerializer.Deserialize<PlayerHandicapData[]>(jsonContent);

            if (playerHandicapData == null)
            {
                Console.WriteLine("Failed to deserialize handicap data");
                return;
            }

            foreach (var playerData in playerHandicapData)
            {
                // Find existing player by first and last name
                var existingPlayer = await _context.Players
                    .FirstOrDefaultAsync(p => p.FirstName == playerData.FirstName && p.LastName == playerData.LastName);

                if (existingPlayer != null)
                {
                    // Update existing player's handicap
                    existingPlayer.InitialHandicap = playerData.Handicap;
                    Console.WriteLine($"Updated handicap for {playerData.FirstName} {playerData.LastName}: {playerData.Handicap}");
                }
                else
                {
                    // Create new player
                    var newPlayer = new Player
                    {
                        FirstName = playerData.FirstName,
                        LastName = playerData.LastName,
                        InitialHandicap = playerData.Handicap,
                        Email = $"{playerData.FirstName.ToLower()}.{playerData.LastName.ToLower()}@example.com",
                        Phone = ""
                    };

                    _context.Players.Add(newPlayer);
                    Console.WriteLine($"Created new player: {playerData.FirstName} {playerData.LastName} with handicap {playerData.Handicap}");
                }
            }

            await _context.SaveChangesAsync();
            Console.WriteLine($"Processed {playerHandicapData.Length} players with handicap data");
        }
    }

    public class PlayerHandicapData
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = string.Empty;
        
        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = string.Empty;
        
        [JsonPropertyName("handicap")]
        public decimal Handicap { get; set; }
    }
}
