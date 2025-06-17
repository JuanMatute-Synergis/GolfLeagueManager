using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class DatabaseCleanupService
    {
        private readonly AppDbContext _context;

        public DatabaseCleanupService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Delete all hole scores and reset matchup scores
        /// </summary>
        public async Task<CleanupResult> DeleteAllScoresAsync()
        {
            try
            {
                // Get counts before deletion for reporting
                var holeScoreCount = await _context.HoleScores.CountAsync();
                var matchupCount = await _context.Matchups.CountAsync();
                var scoreEntryCount = await _context.ScoreEntries.CountAsync();

                // Delete all hole scores
                var holeScores = await _context.HoleScores.ToListAsync();
                _context.HoleScores.RemoveRange(holeScores);

                // Delete all score entries
                var scoreEntries = await _context.ScoreEntries.ToListAsync();
                _context.ScoreEntries.RemoveRange(scoreEntries);

                // Reset all matchup scores and match play data
                var matchups = await _context.Matchups.ToListAsync();
                foreach (var matchup in matchups)
                {
                    matchup.PlayerAScore = null;
                    matchup.PlayerBScore = null;
                    matchup.PlayerAPoints = null;
                    matchup.PlayerBPoints = null;
                    matchup.PlayerAHolePoints = 0;
                    matchup.PlayerBHolePoints = 0;
                    matchup.PlayerAMatchWin = false;
                    matchup.PlayerBMatchWin = false;
                    matchup.PlayerAAbsent = false;
                    matchup.PlayerBAbsent = false;
                    matchup.PlayerAAbsentWithNotice = false;
                    matchup.PlayerBAbsentWithNotice = false;
                }

                await _context.SaveChangesAsync();

                return new CleanupResult
                {
                    Success = true,
                    Message = $"Successfully deleted {holeScoreCount} hole scores, {scoreEntryCount} score entries, and reset {matchupCount} matchups",
                    HoleScoresDeleted = holeScoreCount,
                    MatchupsReset = matchupCount,
                    ScoreEntriesDeleted = scoreEntryCount
                };
            }
            catch (Exception ex)
            {
                return new CleanupResult
                {
                    Success = false,
                    Message = $"Error during cleanup: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Delete hole scores for a specific week
        /// </summary>
        public async Task<CleanupResult> DeleteScoresForWeekAsync(int weekNumber)
        {
            try
            {
                var week = await _context.Weeks.FirstOrDefaultAsync(w => w.WeekNumber == weekNumber);
                if (week == null)
                {
                    return new CleanupResult
                    {
                        Success = false,
                        Message = $"Week {weekNumber} not found"
                    };
                }

                var matchups = await _context.Matchups
                    .Where(m => m.WeekId == week.Id)
                    .ToListAsync();

                var matchupIds = matchups.Select(m => m.Id).ToList();

                // Delete hole scores for this week
                var holeScores = await _context.HoleScores
                    .Where(hs => matchupIds.Contains(hs.MatchupId))
                    .ToListAsync();

                _context.HoleScores.RemoveRange(holeScores);

                // Reset matchup scores for this week
                foreach (var matchup in matchups)
                {
                    matchup.PlayerAScore = null;
                    matchup.PlayerBScore = null;
                    matchup.PlayerAPoints = null;
                    matchup.PlayerBPoints = null;
                    matchup.PlayerAHolePoints = 0;
                    matchup.PlayerBHolePoints = 0;
                    matchup.PlayerAMatchWin = false;
                    matchup.PlayerBMatchWin = false;
                    matchup.PlayerAAbsent = false;
                    matchup.PlayerBAbsent = false;
                    matchup.PlayerAAbsentWithNotice = false;
                    matchup.PlayerBAbsentWithNotice = false;
                }

                await _context.SaveChangesAsync();

                return new CleanupResult
                {
                    Success = true,
                    Message = $"Successfully deleted {holeScores.Count} hole scores and reset {matchups.Count} matchups for week {weekNumber}",
                    HoleScoresDeleted = holeScores.Count,
                    MatchupsReset = matchups.Count
                };
            }
            catch (Exception ex)
            {
                return new CleanupResult
                {
                    Success = false,
                    Message = $"Error during cleanup for week {weekNumber}: {ex.Message}"
                };
            }
        }
    }

    public class CleanupResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int HoleScoresDeleted { get; set; }
        public int MatchupsReset { get; set; }
        public int ScoreEntriesDeleted { get; set; }
    }
}
