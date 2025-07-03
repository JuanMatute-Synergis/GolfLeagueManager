using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class HandicapDebugController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HandicapService _handicapService;
        private readonly LeagueSettingsService _leagueSettingsService;

        public HandicapDebugController(AppDbContext context, HandicapService handicapService, LeagueSettingsService leagueSettingsService)
        {
            _context = context;
            _handicapService = handicapService;
            _leagueSettingsService = leagueSettingsService;
        }

        [HttpGet("player/{playerId}/season/{seasonId}")]
        public async Task<ActionResult<object>> DebugPlayerHandicap(Guid playerId, Guid seasonId)
        {
            try
            {
                var player = await _context.Players.FindAsync(playerId);
                if (player == null)
                {
                    return NotFound($"Player with ID {playerId} not found");
                }

                var season = await _context.Seasons.FindAsync(seasonId);
                if (season == null)
                {
                    return NotFound($"Season with ID {seasonId} not found");
                }

                var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

                // Get recent scores for this player
                var recentScores = await GetRecentPlayerScoresAsync(playerId, leagueSettings.MaxRoundsForHandicap);

                // Calculate handicap using current league settings
                decimal calculatedHandicap = 0;
                string calculationMethod = "";
                string calculationDetails = "";

                if (recentScores.Any())
                {
                    if (leagueSettings.HandicapMethod == HandicapCalculationMethod.SimpleAverage)
                    {
                        calculationMethod = "Simple Average";
                        var scores = recentScores.Select(s => s.score).ToArray();
                        var averageScore = (decimal)scores.Average();
                        calculatedHandicap = Math.Round(averageScore - leagueSettings.CoursePar, 1);
                        calculatedHandicap = Math.Max(0, Math.Min(36, calculatedHandicap));
                        
                        calculationDetails = $"Average Score: {averageScore:F2}, Course Par: {leagueSettings.CoursePar}, " +
                                           $"Raw Handicap: {averageScore - leagueSettings.CoursePar:F2}, " +
                                           $"Capped Handicap: {calculatedHandicap:F1}";
                    }
                    else
                    {
                        calculationMethod = "World Handicap System";
                        var adjustedScores = recentScores.Select(s => (s.score, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating)).ToList();
                        calculatedHandicap = CalculateHandicapIndex(adjustedScores);
                        
                        calculationDetails = $"Course Rating: {leagueSettings.CourseRating}, Slope: {leagueSettings.SlopeRating}, " +
                                           $"Using {adjustedScores.Count} rounds";
                    }
                }
                else
                {
                    calculationMethod = "No scores available";
                    calculatedHandicap = player.InitialHandicap;
                    calculationDetails = "Using initial handicap as no scores are available";
                }

                return Ok(new
                {
                    Player = new
                    {
                        Id = player.Id,
                        Name = $"{player.FirstName} {player.LastName}".Trim(),
                        InitialHandicap = player.InitialHandicap
                    },
                    Season = new
                    {
                        Id = season.Id,
                        Name = season.Name,
                        Year = season.Year
                    },
                    LeagueSettings = new
                    {
                        HandicapMethod = leagueSettings.HandicapMethod.ToString(),
                        CoursePar = leagueSettings.CoursePar,
                        CourseRating = leagueSettings.CourseRating,
                        SlopeRating = leagueSettings.SlopeRating,
                        MaxRoundsForHandicap = leagueSettings.MaxRoundsForHandicap
                    },
                    RecentScores = recentScores.Select(s => new
                    {
                        Score = s.score,
                        Date = s.date,
                        Week = s.weekNumber
                    }).ToArray(),
                    Calculation = new
                    {
                        Method = calculationMethod,
                        Details = calculationDetails,
                        CalculatedHandicap = calculatedHandicap,
                        InitialHandicap = player.InitialHandicap,
                        Difference = calculatedHandicap - player.InitialHandicap
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error debugging handicap: {ex.Message}");
            }
        }

        [HttpGet("players/season/{seasonId}")]
        public async Task<ActionResult<object>> GetPlayersForSeason(Guid seasonId)
        {
            try
            {
                var players = await _context.PlayerFlightAssignments
                    .Include(pfa => pfa.Player)
                    .Include(pfa => pfa.Flight)
                    .Where(pfa => pfa.Flight != null && pfa.Flight.SeasonId == seasonId)
                    .Select(pfa => new
                    {
                        Id = pfa.Player!.Id,
                        Name = pfa.Player.FirstName + " " + pfa.Player.LastName,
                        InitialHandicap = pfa.Player.InitialHandicap
                    })
                    .Distinct()
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return Ok(players);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error getting players: {ex.Message}");
            }
        }

        private async Task<List<(int score, DateTime date, int weekNumber)>> GetRecentPlayerScoresAsync(Guid playerId, int maxRounds)
        {
            var matchups = await _context.Matchups
                .Include(m => m.Week)
                .Where(m => (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue)) &&
                           m.Week != null &&
                           m.Week.CountsForScoring &&
                           m.Week.CountsForHandicap &&
                           !m.Week.SpecialPointsAwarded.HasValue) // Exclude weeks with special points
                .OrderByDescending(m => m.Week != null ? m.Week.WeekNumber : 0)
                .Take(maxRounds)
                .ToListAsync();

            var scores = new List<(int score, DateTime date, int weekNumber)>();

            foreach (var matchup in matchups)
            {
                if (matchup.PlayerAId == playerId && matchup.PlayerAScore.HasValue)
                {
                    scores.Add((matchup.PlayerAScore.Value, matchup.Week?.Date ?? DateTime.MinValue, matchup.Week?.WeekNumber ?? 0));
                }
                else if (matchup.PlayerBId == playerId && matchup.PlayerBScore.HasValue)
                {
                    scores.Add((matchup.PlayerBScore.Value, matchup.Week?.Date ?? DateTime.MinValue, matchup.Week?.WeekNumber ?? 0));
                }
            }

            return scores.OrderByDescending(s => s.weekNumber).ToList();
        }

        private decimal CalculateHandicapIndex(List<(int score, int courseRating, decimal slopeRating)> scores)
        {
            if (!scores.Any()) return 0;

            var differentials = scores.Select(s =>
            {
                decimal differential = (s.score - s.courseRating) * 113m / s.slopeRating;
                return Math.Round(differential, 1);
            }).ToList();

            differentials.Sort();

            int numDifferentials = differentials.Count;
            int numToUse = numDifferentials switch
            {
                >= 20 => 8,
                >= 19 => 7,
                >= 16 => 6,
                >= 13 => 5,
                >= 10 => 4,
                >= 7 => 3,
                >= 5 => 2,
                >= 3 => 1,
                _ => 0
            };

            if (numToUse == 0) return 0;

            var bestDifferentials = differentials.Take(numToUse);
            var average = bestDifferentials.Average();
            var handicapIndex = Math.Round(average, 1);

            return Math.Max(0, Math.Min(36, handicapIndex));
        }
    }
}
