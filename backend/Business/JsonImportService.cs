using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Business
{
    public class JsonImportService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IWeekRepository _weekRepository;
        private readonly IScoreEntryRepository _scoreEntryRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly IMatchupRepository _matchupRepository;
        private readonly AppDbContext _context;

        public JsonImportService(
            IPlayerRepository playerRepository,
            IWeekRepository weekRepository,
            IScoreEntryRepository scoreEntryRepository,
            ISeasonRepository seasonRepository,
            IMatchupRepository matchupRepository,
            AppDbContext context)
        {
            _playerRepository = playerRepository;
            _weekRepository = weekRepository;
            _scoreEntryRepository = scoreEntryRepository;
            _seasonRepository = seasonRepository;
            _matchupRepository = matchupRepository;
            _context = context;
        }

        public async Task<ImportResult> ImportScoresFromJsonAsync(string jsonContent)
        {
            var result = new ImportResult();

            try
            {
                // Parse JSON
                var importData = JsonSerializer.Deserialize<JsonImportData>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (importData?.Players == null)
                {
                    result.Errors.Add("Invalid JSON format: 'players' array not found");
                    return result;
                }

                // Get or create season (assuming current season)
                var seasons = _seasonRepository.GetSeasons();
                var season = seasons.FirstOrDefault();
                if (season == null)
                {
                    season = new Season 
                    { 
                        Id = Guid.NewGuid(),
                        Name = "2025 Season", 
                        StartDate = DateTime.Now.Date,
                        EndDate = DateTime.Now.AddMonths(6).Date,
                        Year = 2025,
                        SeasonNumber = 1
                    };
                    _seasonRepository.AddSeason(season);
                }

                foreach (var playerData in importData.Players)
                {
                    try
                    {
                        // Get or create player
                        var existingPlayers = _playerRepository.GetPlayers();
                        var existingPlayer = existingPlayers.FirstOrDefault(p => 
                            p.FirstName.Equals(playerData.FirstName, StringComparison.OrdinalIgnoreCase) &&
                            p.LastName.Equals(playerData.LastName, StringComparison.OrdinalIgnoreCase));
                        
                        Player player;
                        if (existingPlayer == null)
                        {
                            player = new Player
                            {
                                Id = Guid.NewGuid(),
                                FirstName = playerData.FirstName,
                                LastName = playerData.LastName,
                                Email = "",
                                Phone = ""
                            };
                            _playerRepository.AddPlayer(player);
                            result.PlayersCreated++;
                        }
                        else
                        {
                            player = existingPlayer;
                        }

                        // Process each round
                        foreach (var roundData in playerData.Rounds)
                        {
                            try
                            {
                                // Get the week
                                var weeks = await _weekRepository.GetWeeksBySeasonIdAsync(season.Id);
                                var week = weeks.FirstOrDefault(w => w.WeekNumber == roundData.Week);
                                if (week == null)
                                {
                                    week = new Week
                                    {
                                        Id = Guid.NewGuid(),
                                        SeasonId = season.Id,
                                        WeekNumber = roundData.Week,
                                        Date = DateTime.Now.Date.AddDays((roundData.Week - 1) * 7),
                                        Name = $"Week {roundData.Week}",
                                        IsActive = true
                                    };
                                    week = await _weekRepository.CreateAsync(week);
                                }

                                // Find the matchup for this player and week
                                var matchups = await _matchupRepository.GetByWeekIdAsync(week.Id);
                                var matchup = matchups.FirstOrDefault(m => 
                                    m.PlayerAId == player.Id || m.PlayerBId == player.Id);

                                if (matchup == null)
                                {
                                    result.Errors.Add($"No matchup found for {playerData.FirstName} {playerData.LastName} in Week {roundData.Week}");
                                    continue;
                                }

                                // Determine if this player is PlayerA or PlayerB
                                bool isPlayerA = matchup.PlayerAId == player.Id;

                                // Process individual hole scores
                                foreach (var holeScore in roundData.Scores)
                                {
                                    // Extract hole number from the key (e.g., "hole1" -> 1)
                                    if (int.TryParse(holeScore.Key.Replace("hole", ""), out int holeNumber))
                                    {
                                        // Find or create the HoleScore record
                                        var existingHoleScore = await _context.HoleScores
                                            .FirstOrDefaultAsync(hs => hs.MatchupId == matchup.Id && hs.HoleNumber == holeNumber);

                                        if (existingHoleScore == null)
                                        {
                                            // Create new HoleScore record
                                            var newHoleScore = new HoleScore
                                            {
                                                Id = Guid.NewGuid(),
                                                MatchupId = matchup.Id,
                                                HoleNumber = holeNumber,
                                                Par = GetParForHole(holeNumber), // Helper method to get par
                                                HoleHandicap = GetHoleHandicap(holeNumber), // Helper method to get handicap
                                                PlayerAScore = isPlayerA ? holeScore.Value : null,
                                                PlayerBScore = isPlayerA ? null : holeScore.Value,
                                                PlayerAMatchPoints = 0, // Will be calculated later
                                                PlayerBMatchPoints = 0  // Will be calculated later
                                            };
                                            _context.HoleScores.Add(newHoleScore);
                                        }
                                        else
                                        {
                                            // Update existing HoleScore record
                                            if (isPlayerA)
                                                existingHoleScore.PlayerAScore = holeScore.Value;
                                            else
                                                existingHoleScore.PlayerBScore = holeScore.Value;
                                        }
                                    }
                                }

                                await _context.SaveChangesAsync();
                                result.RoundsProcessed++;
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Error processing round for {playerData.FirstName} {playerData.LastName}, Week {roundData.Week}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add($"Error processing player {playerData.FirstName} {playerData.LastName}: {ex.Message}");
                    }
                }

                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"JSON parsing error: {ex.Message}");
                result.Success = false;
            }

            return result;
        }

        private int GetParForHole(int holeNumber)
        {
            // Standard golf course par values - you may need to adjust these based on your specific course
            var parValues = new Dictionary<int, int>
            {
                { 1, 4 }, { 2, 4 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 3 }, { 7, 4 }, { 8, 4 }, { 9, 4 },
                { 10, 4 }, { 11, 3 }, { 12, 5 }, { 13, 4 }, { 14, 4 }, { 15, 3 }, { 16, 4 }, { 17, 4 }, { 18, 4 }
            };
            return parValues.GetValueOrDefault(holeNumber, 4); // Default to par 4 if not found
        }

        private int GetHoleHandicap(int holeNumber)
        {
            // Standard hole handicap/stroke index - you may need to adjust these based on your course
            var handicapValues = new Dictionary<int, int>
            {
                { 1, 1 }, { 2, 5 }, { 3, 7 }, { 4, 3 }, { 5, 9 }, { 6, 8 }, { 7, 2 }, { 8, 4 }, { 9, 6 },
                { 10, 1 }, { 11, 7 }, { 12, 9 }, { 13, 3 }, { 14, 5 }, { 15, 8 }, { 16, 2 }, { 17, 4 }, { 18, 6 }
            };
            return handicapValues.GetValueOrDefault(holeNumber, 1); // Default to handicap 1 if not found
        }
    }

    // Data models for JSON deserialization
    public class JsonImportData
    {
        public List<JsonPlayerData> Players { get; set; } = new();
    }

    public class JsonPlayerData
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<JsonRoundData> Rounds { get; set; } = new();
    }

    public class JsonRoundData
    {
        public int Week { get; set; }
        public string FrontOrBack { get; set; } = string.Empty;
        public Dictionary<string, int> Scores { get; set; } = new();
        public int Total { get; set; }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public int PlayersCreated { get; set; }
        public int RoundsProcessed { get; set; }
        public int ScoresImported { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
