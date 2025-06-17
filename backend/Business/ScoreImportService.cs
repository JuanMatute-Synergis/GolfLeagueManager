using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace GolfLeagueManager
{
    public class ScoreImportService
    {
        private readonly AppDbContext _context;
        private readonly MatchPlayService _matchPlayService;

        public ScoreImportService(AppDbContext context, MatchPlayService matchPlayService)
        {
            _context = context;
            _matchPlayService = matchPlayService;
        }

        public async Task<ImportResult> ImportScoresFromCsvAsync(string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return new ImportResult { Success = false, Message = "CSV file is empty or invalid" };

            var header = lines[0].Split(',');
            var importedCount = 0;
            var errors = new List<string>();

            // Parse each line of the CSV
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var values = lines[i].Split(',');
                    if (values.Length < header.Length) continue;

                    var playerName = values[0].Trim();
                    var weekNumber = int.Parse(values[11]); // Week_Number column
                    var frontOrBack = values[12].Trim(); // Front_or_Back column

                    if (string.IsNullOrEmpty(playerName)) continue;

                    // Find the player in the database
                    var player = await _context.Players
                        .FirstOrDefaultAsync(p => (p.FirstName + " " + p.LastName).ToLower().Contains(playerName.ToLower()) || 
                                                  playerName.ToLower().Contains((p.FirstName + " " + p.LastName).ToLower()));

                    if (player == null)
                    {
                        errors.Add($"Player not found: {playerName}");
                        continue;
                    }

                    // Find the week
                    var week = await _context.Weeks
                        .FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);

                    if (week == null)
                    {
                        errors.Add($"Week {weekNumber} not found");
                        continue;
                    }

                    // Find matchups for this player and week
                    var matchups = await _context.Matchups
                        .Where(m => m.WeekId == week.Id && 
                                   (m.PlayerAId == player.Id || m.PlayerBId == player.Id))
                        .ToListAsync();

                    foreach (var matchup in matchups)
                    {
                        await ProcessPlayerScores(matchup, player.Id, values, frontOrBack, errors);
                        importedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing line {i + 1}: {ex.Message}");
                }
            }

            return new ImportResult 
            { 
                Success = errors.Count < importedCount,
                Message = $"Imported {importedCount} scorecards. {errors.Count} errors.",
                Errors = errors
            };
        }

        /// <summary>
        /// Import scores from CSV with separate First Name/Last Name columns
        /// </summary>
        public async Task<ImportResult> ImportScoresFromFormattedCsvAsync(string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) return new ImportResult { Success = false, Message = "CSV file is empty or invalid" };

            var importedCount = 0;
            var errors = new List<string>();

            // Skip header line (first line after RemoveEmptyEntries)
            for (int i = 1; i < lines.Length; i++)
            {
                try
                {
                    var values = lines[i].Split(',');
                    if (values.Length < 23) continue; // Need at least 23 columns

                    var weekNumber = int.Parse(values[0]); // Week
                    var frontOrBack = values[1].Trim(); // Front/Back
                    var firstName = values[2].Trim(); // First Name
                    var lastName = values[3].Trim(); // Last Name

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName)) continue;

                    // Find the player in the database
                    var playerName = $"{firstName} {lastName}";
                    var player = await _context.Players
                        .FirstOrDefaultAsync(p => (p.FirstName + " " + p.LastName).ToLower() == playerName.ToLower() ||
                                                  (p.FirstName.ToLower() == firstName.ToLower() && p.LastName.ToLower() == lastName.ToLower()));

                    if (player == null)
                    {
                        errors.Add($"Player not found: {playerName}");
                        continue;
                    }

                    // Find the week
                    var week = await _context.Weeks
                        .FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);

                    if (week == null)
                    {
                        errors.Add($"Week {weekNumber} not found");
                        continue;
                    }

                    // Find matchups for this player and week
                    var matchups = await _context.Matchups
                        .Where(m => m.WeekId == week.Id && 
                                   (m.PlayerAId == player.Id || m.PlayerBId == player.Id))
                        .ToListAsync();

                    foreach (var matchup in matchups)
                    {
                        await ProcessFormattedPlayerScores(matchup, player.Id, values, frontOrBack, errors);
                        importedCount++;
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing line {i + 1}: {ex.Message}");
                }
            }

            return new ImportResult 
            { 
                Success = errors.Count < importedCount,
                Message = $"Imported {importedCount} scorecards. {errors.Count} errors.",
                Errors = errors
            };
        }

        private async Task ProcessPlayerScores(Matchup matchup, Guid playerId, string[] values, string frontOrBack, List<string> errors)
        {
            try
            {
                // Initialize hole scores if they don't exist
                await _matchPlayService.InitializeHoleScoresAsync(matchup.Id);

                var holeScores = await _context.HoleScores
                    .Where(hs => hs.MatchupId == matchup.Id)
                    .OrderBy(hs => hs.HoleNumber)
                    .ToListAsync();

                bool isPlayerA = matchup.PlayerAId == playerId;
                
                // Process hole scores based on front/back
                if (frontOrBack.ToLower() == "front")
                {
                    // Process holes 1-9 (Front nine)
                    for (int hole = 1; hole <= 9; hole++)
                    {
                        var scoreValue = values[hole]; // Hole_1 is at index 1, etc.
                        if (double.TryParse(scoreValue, out double score) && score > 0)
                        {
                            var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                            if (holeScore != null)
                            {
                                if (isPlayerA)
                                    holeScore.PlayerAScore = (int)score;
                                else
                                    holeScore.PlayerBScore = (int)score;
                            }
                        }
                    }
                }
                else if (frontOrBack.ToLower() == "back")
                {
                    // Process holes 10-18 (Back nine) - but store as holes 1-9 in our 9-hole system
                    // We'll need to map this differently since your system uses 9 holes
                    // For now, let's skip back nine or handle it as a separate scorecard
                    return;
                }

                // Update total score for the matchup
                if (isPlayerA)
                {
                    matchup.PlayerAScore = holeScores.Where(hs => hs.PlayerAScore.HasValue)
                                                   .Sum(hs => hs.PlayerAScore!.Value);
                }
                else
                {
                    matchup.PlayerBScore = holeScores.Where(hs => hs.PlayerBScore.HasValue)
                                                   .Sum(hs => hs.PlayerBScore!.Value);
                }

                await _context.SaveChangesAsync();

                // Calculate match play results if both players have scores
                if (matchup.PlayerAScore.HasValue && matchup.PlayerBScore.HasValue)
                {
                    await _matchPlayService.CalculateMatchPlayResultsAsync(matchup.Id);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing scores for matchup {matchup.Id}: {ex.Message}");
            }
        }

        private async Task ProcessFormattedPlayerScores(Matchup matchup, Guid playerId, string[] values, string frontOrBack, List<string> errors)
        {
            try
            {
                // Initialize hole scores if they don't exist
                await _matchPlayService.InitializeHoleScoresAsync(matchup.Id);

                var holeScores = await _context.HoleScores
                    .Where(hs => hs.MatchupId == matchup.Id)
                    .OrderBy(hs => hs.HoleNumber)
                    .ToListAsync();

                bool isPlayerA = matchup.PlayerAId == playerId;
                
                // Process hole scores based on front/back
                if (frontOrBack.ToLower() == "front")
                {
                    // Process holes 1-9 (columns 4-12)
                    for (int hole = 1; hole <= 9; hole++)
                    {
                        var scoreIndex = hole + 3; // Hole 1 is at index 4, etc.
                        var scoreValue = values[scoreIndex].Trim();
                        if (int.TryParse(scoreValue, out int score) && score > 0)
                        {
                            var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                            if (holeScore != null)
                            {
                                if (isPlayerA)
                                    holeScore.PlayerAScore = score;
                                else
                                    holeScore.PlayerBScore = score;
                            }
                        }
                    }
                }
                else if (frontOrBack.ToLower() == "back")
                {
                    // For back 9, we need to decide how to handle this
                    // Since the system is designed for 9-hole matches, we'll skip back 9 for now
                    // or you could create separate matchups for back 9
                    return;
                }

                // Update total score for the matchup
                if (isPlayerA)
                {
                    var totalScore = holeScores.Where(hs => hs.PlayerAScore.HasValue)
                                              .Sum(hs => hs.PlayerAScore!.Value);
                    if (totalScore > 0)
                        matchup.PlayerAScore = totalScore;
                }
                else
                {
                    var totalScore = holeScores.Where(hs => hs.PlayerBScore.HasValue)
                                              .Sum(hs => hs.PlayerBScore!.Value);
                    if (totalScore > 0)
                        matchup.PlayerBScore = totalScore;
                }

                await _context.SaveChangesAsync();

                // Calculate match play results if both players have scores
                if (matchup.PlayerAScore.HasValue && matchup.PlayerBScore.HasValue)
                {
                    await _matchPlayService.CalculateMatchPlayResultsAsync(matchup.Id);
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error processing scores for matchup {matchup.Id}: {ex.Message}");
            }
        }
    }

    public class ImportResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
    }
}
