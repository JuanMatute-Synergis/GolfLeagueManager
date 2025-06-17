using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class ScoreEntryService
    {
        private readonly IScoreEntryRepository _scoreEntryRepository;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWeekRepository _weekRepository;
        private readonly IPlayerFlightAssignmentRepository _playerFlightAssignmentRepository;

        public ScoreEntryService(
            IScoreEntryRepository scoreEntryRepository,
            IPlayerRepository playerRepository,
            IWeekRepository weekRepository,
            IPlayerFlightAssignmentRepository playerFlightAssignmentRepository)
        {
            _scoreEntryRepository = scoreEntryRepository;
            _playerRepository = playerRepository;
            _weekRepository = weekRepository;
            _playerFlightAssignmentRepository = playerFlightAssignmentRepository;
        }

        public async Task<IEnumerable<ScoreEntry>> GetAllScoreEntriesAsync()
        {
            return await _scoreEntryRepository.GetAllAsync();
        }

        public async Task<ScoreEntry?> GetScoreEntryByIdAsync(Guid id)
        {
            return await _scoreEntryRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ScoreEntry>> GetScoreEntriesByWeekIdAsync(Guid weekId)
        {
            return await _scoreEntryRepository.GetScoreEntriesByWeekIdAsync(weekId);
        }

        public async Task<IEnumerable<ScoreEntry>> GetScoreEntriesByPlayerIdAsync(Guid playerId)
        {
            return await _scoreEntryRepository.GetScoreEntriesByPlayerIdAsync(playerId);
        }

        public async Task<ScoreEntry?> GetScoreEntryByPlayerAndWeekAsync(Guid playerId, Guid weekId)
        {
            return await _scoreEntryRepository.GetScoreEntryByPlayerAndWeekAsync(playerId, weekId);
        }

        public async Task<ScoreEntry> CreateOrUpdateScoreEntryAsync(ScoreEntry scoreEntry)
        {
            // Validate player exists
            var player = await _playerRepository.GetByIdAsync(scoreEntry.PlayerId);
            if (player == null)
            {
                throw new ArgumentException("Player not found");
            }

            // Validate week exists
            var week = await _weekRepository.GetByIdAsync(scoreEntry.WeekId);
            if (week == null)
            {
                throw new ArgumentException("Week not found");
            }

            // Check if score entry already exists
            var existingEntry = await GetScoreEntryByPlayerAndWeekAsync(scoreEntry.PlayerId, scoreEntry.WeekId);
            
            if (existingEntry != null)
            {
                // Update existing entry
                existingEntry.Score = scoreEntry.Score;
                existingEntry.PointsEarned = await CalculatePointsAsync(scoreEntry.PlayerId, scoreEntry.WeekId, scoreEntry.Score);
                return await _scoreEntryRepository.UpdateAsync(existingEntry);
            }
            else
            {
                // Create new entry
                scoreEntry.PointsEarned = await CalculatePointsAsync(scoreEntry.PlayerId, scoreEntry.WeekId, scoreEntry.Score);
                return await _scoreEntryRepository.CreateAsync(scoreEntry);
            }
        }

        public async Task<bool> DeleteScoreEntryAsync(Guid id)
        {
            return await _scoreEntryRepository.DeleteAsync(id);
        }

        public async Task<IEnumerable<ScoreEntry>> GetLeaderboardByWeekAsync(Guid weekId)
        {
            var scores = await GetScoreEntriesByWeekIdAsync(weekId);
            return scores.OrderByDescending(s => s.PointsEarned).ThenBy(s => s.Score);
        }

        public async Task<IEnumerable<PlayerSeasonStats>> GetSeasonStandingsAsync(Guid seasonId)
        {
            var scoreEntries = await _scoreEntryRepository.GetScoreEntriesBySeasonIdAsync(seasonId);
            
            var standings = scoreEntries
                .GroupBy(se => se.PlayerId)
                .Select(g => new PlayerSeasonStats
                {
                    PlayerId = g.Key,
                    PlayerName = g.First().Player?.FirstName + " " + g.First().Player?.LastName,
                    TotalPoints = g.Sum(se => se.PointsEarned),
                    AverageScore = g.Average(se => se.Score),
                    RoundsPlayed = g.Count(),
                    BestScore = g.Min(se => se.Score),
                    WorstScore = g.Max(se => se.Score)
                })
                .OrderByDescending(ps => ps.TotalPoints)
                .ThenBy(ps => ps.AverageScore);

            return standings;
        }

        private async Task<int> CalculatePointsAsync(Guid playerId, Guid weekId, int score)
        {
            // Get all scores for this week
            var weekScores = await GetScoreEntriesByWeekIdAsync(weekId);
            var allScores = weekScores.Select(se => se.Score).ToList();
            allScores.Add(score); // Include the new score

            // Sort scores (ascending - lower scores are better in golf)
            allScores.Sort();

            // Calculate points based on position (first place gets most points)
            var position = allScores.IndexOf(score) + 1;
            var totalPlayers = allScores.Count;

            // Points calculation: (Total players - position + 1)
            // First place: totalPlayers points, last place: 1 point
            return Math.Max(1, totalPlayers - position + 1);
        }

        public async Task<IEnumerable<ScoreEntry>> BulkCreateScoreEntriesAsync(IEnumerable<ScoreEntry> scoreEntries)
        {
            var results = new List<ScoreEntry>();

            foreach (var scoreEntry in scoreEntries)
            {
                try
                {
                    var result = await CreateOrUpdateScoreEntryAsync(scoreEntry);
                    results.Add(result);
                }
                catch (Exception)
                {
                    // Log error but continue with other entries
                    continue;
                }
            }

            return results;
        }
    }

    public class PlayerSeasonStats
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public int TotalPoints { get; set; }
        public double AverageScore { get; set; }
        public int RoundsPlayed { get; set; }
        public int BestScore { get; set; }
        public int WorstScore { get; set; }
    }
}
