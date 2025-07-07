using GolfLeagueManager.Models;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Business
{
    public class PlayerSeasonStatsService
    {
        private readonly AppDbContext _context;

        public PlayerSeasonStatsService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get or create PlayerSeasonRecord for a player in a specific season
        /// </summary>
        public async Task<PlayerSeasonRecord> GetOrCreatePlayerSeasonStatsAsync(Guid playerId, Guid seasonId)
        {
            var existingStats = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(pss => pss.PlayerId == playerId && pss.SeasonId == seasonId);

            if (existingStats != null)
            {
                return existingStats;
            }

            // Create new stats record
            var newStats = new PlayerSeasonRecord
            {
                PlayerId = playerId,
                SeasonId = seasonId,
                InitialHandicap = 0,
                InitialAverageScore = 0,
                CurrentHandicap = 0,
                CurrentAverageScore = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.PlayerSeasonRecords.Add(newStats);
            await _context.SaveChangesAsync();

            return newStats;
        }

        /// <summary>
        /// Update initial values for a player in a season (used during flight assignment)
        /// </summary>
        public async Task<PlayerSeasonRecord> UpdateInitialValuesAsync(Guid playerId, Guid seasonId, decimal initialHandicap, decimal initialAverageScore)
        {
            var stats = await GetOrCreatePlayerSeasonStatsAsync(playerId, seasonId);

            stats.InitialHandicap = initialHandicap;
            stats.InitialAverageScore = initialAverageScore;
            stats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return stats;
        }

        /// <summary>
        /// Update current calculated values for a player in a season
        /// </summary>
        public async Task<PlayerSeasonRecord> UpdateCurrentValuesAsync(Guid playerId, Guid seasonId, decimal currentHandicap, decimal currentAverageScore)
        {
            var stats = await GetOrCreatePlayerSeasonStatsAsync(playerId, seasonId);

            stats.CurrentHandicap = currentHandicap;
            stats.CurrentAverageScore = currentAverageScore;
            stats.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return stats;
        }

        /// <summary>
        /// Get all players' season stats for a specific season
        /// </summary>
        public async Task<List<PlayerSeasonRecord>> GetSeasonStatsAsync(Guid seasonId)
        {
            return await _context.PlayerSeasonRecords
                .Include(pss => pss.Player)
                .Where(pss => pss.SeasonId == seasonId)
                .ToListAsync();
        }

        /// <summary>
        /// Get specific player's stats for a season
        /// </summary>
        public async Task<PlayerSeasonRecord?> GetPlayerSeasonStatsAsync(Guid playerId, Guid seasonId)
        {
            return await _context.PlayerSeasonRecords
                .Include(pss => pss.Player)
                .Include(pss => pss.Season)
                .FirstOrDefaultAsync(pss => pss.PlayerId == playerId && pss.SeasonId == seasonId);
        }

        /// <summary>
        /// Get initial handicap for a player in a season, with fallback to player's deprecated field
        /// </summary>
        public async Task<decimal> GetInitialHandicapAsync(Guid playerId, Guid seasonId)
        {
            var seasonRecord = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(psr => psr.PlayerId == playerId && psr.SeasonId == seasonId);

            if (seasonRecord != null)
            {
                return seasonRecord.InitialHandicap;
            }

            // Fallback to deprecated player field
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            return player?.InitialHandicap ?? 0;
        }

        /// <summary>
        /// Get initial average score for a player in a season, with fallback to player's deprecated field
        /// </summary>
        public async Task<decimal> GetInitialAverageScoreAsync(Guid playerId, Guid seasonId)
        {
            var seasonRecord = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(psr => psr.PlayerId == playerId && psr.SeasonId == seasonId);

            if (seasonRecord != null)
            {
                return seasonRecord.InitialAverageScore;
            }

            // Fallback to deprecated player field
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            return player?.InitialAverageScore ?? 0;
        }

        /// <summary>
        /// Get current average score for a player in a season, with fallback to player's deprecated field
        /// </summary>
        public async Task<decimal> GetCurrentAverageScoreAsync(Guid playerId, Guid seasonId)
        {
            var seasonRecord = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(psr => psr.PlayerId == playerId && psr.SeasonId == seasonId);

            if (seasonRecord != null)
            {
                return seasonRecord.CurrentAverageScore;
            }

            // Fallback to deprecated player field
            var player = await _context.Players.FirstOrDefaultAsync(p => p.Id == playerId);
            return player?.CurrentAverageScore ?? 0;
        }

        /// <summary>
        /// Get current handicap for a player in a season, with fallback calculation
        /// </summary>
        public async Task<decimal> GetCurrentHandicapAsync(Guid playerId, Guid seasonId)
        {
            var seasonRecord = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(psr => psr.PlayerId == playerId && psr.SeasonId == seasonId);

            if (seasonRecord != null)
            {
                return seasonRecord.CurrentHandicap;
            }

            // Fallback to initial handicap
            return await GetInitialHandicapAsync(playerId, seasonId);
        }

        /// <summary>
        /// Delete season stats for a player (used when removing player from season)
        /// </summary>
        public async Task DeletePlayerSeasonStatsAsync(Guid playerId, Guid seasonId)
        {
            var stats = await _context.PlayerSeasonRecords
                .FirstOrDefaultAsync(pss => pss.PlayerId == playerId && pss.SeasonId == seasonId);

            if (stats != null)
            {
                _context.PlayerSeasonRecords.Remove(stats);
                await _context.SaveChangesAsync();
            }
        }
    }
}
