using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class LeagueSettingsService
    {
        private readonly AppDbContext _context;

        public LeagueSettingsService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get league settings for a season, creating default settings if none exist
        /// </summary>
        public async Task<LeagueSettings> GetLeagueSettingsAsync(Guid seasonId)
        {
            var settings = await _context.LeagueSettings
                .FirstOrDefaultAsync(ls => ls.SeasonId == seasonId);

            if (settings == null)
            {
                // Create default settings for the season
                settings = CreateDefaultSettings(seasonId);
                _context.LeagueSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        /// <summary>
        /// Update league settings for a season
        /// </summary>
        public async Task<LeagueSettings> UpdateLeagueSettingsAsync(LeagueSettings settings)
        {
            var existingSettings = await _context.LeagueSettings
                .FirstOrDefaultAsync(ls => ls.SeasonId == settings.SeasonId);

            if (existingSettings == null)
            {
                // Create new settings
                settings.Id = Guid.NewGuid();
                settings.CreatedDate = DateTime.UtcNow;
                _context.LeagueSettings.Add(settings);
            }
            else
            {
                // Update existing settings
                existingSettings.HandicapMethod = settings.HandicapMethod;
                existingSettings.CoursePar = settings.CoursePar;
                existingSettings.CourseRating = settings.CourseRating;
                existingSettings.SlopeRating = settings.SlopeRating;
                existingSettings.MaxRoundsForHandicap = settings.MaxRoundsForHandicap;
                existingSettings.ScoringMethod = settings.ScoringMethod;
                existingSettings.PointsSystem = settings.PointsSystem;
                existingSettings.HoleWinPoints = settings.HoleWinPoints;
                existingSettings.HoleHalvePoints = settings.HoleHalvePoints;
                existingSettings.MatchWinBonus = settings.MatchWinBonus;
                existingSettings.MatchTiePoints = settings.MatchTiePoints;
                existingSettings.UseSessionHandicaps = settings.UseSessionHandicaps;
                existingSettings.AllowHandicapUpdates = settings.AllowHandicapUpdates;
                existingSettings.CustomRules = settings.CustomRules;
                existingSettings.ModifiedDate = DateTime.UtcNow;
                
                settings = existingSettings;
            }

            await _context.SaveChangesAsync();
            return settings;
        }

        /// <summary>
        /// Create default league settings
        /// </summary>
        private LeagueSettings CreateDefaultSettings(Guid seasonId)
        {
            return new LeagueSettings
            {
                Id = Guid.NewGuid(),
                SeasonId = seasonId,
                HandicapMethod = HandicapCalculationMethod.WorldHandicapSystem,
                CoursePar = 36,
                CourseRating = 35.0m,
                SlopeRating = 113m,
                MaxRoundsForHandicap = 20,
                ScoringMethod = ScoringMethod.MatchPlay,
                PointsSystem = PointsSystem.HolePointsWithMatchBonus,
                HoleWinPoints = 2,
                HoleHalvePoints = 1,
                MatchWinBonus = 2,
                MatchTiePoints = 1,
                UseSessionHandicaps = true,
                AllowHandicapUpdates = true,
                CreatedDate = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Get all league settings for administrative purposes
        /// </summary>
        public async Task<List<LeagueSettings>> GetAllLeagueSettingsAsync()
        {
            return await _context.LeagueSettings
                .Include(ls => ls.Season)
                .OrderByDescending(ls => ls.CreatedDate)
                .ToListAsync();
        }

        /// <summary>
        /// Reset league settings to defaults for a season
        /// </summary>
        public async Task<LeagueSettings> ResetToDefaultsAsync(Guid seasonId)
        {
            var existingSettings = await _context.LeagueSettings
                .FirstOrDefaultAsync(ls => ls.SeasonId == seasonId);

            if (existingSettings != null)
            {
                _context.LeagueSettings.Remove(existingSettings);
            }

            var defaultSettings = CreateDefaultSettings(seasonId);
            _context.LeagueSettings.Add(defaultSettings);
            await _context.SaveChangesAsync();

            return defaultSettings;
        }

        /// <summary>
        /// Delete league settings for a season
        /// </summary>
        public async Task<bool> DeleteLeagueSettingsAsync(Guid seasonId)
        {
            var settings = await _context.LeagueSettings
                .FirstOrDefaultAsync(ls => ls.SeasonId == seasonId);

            if (settings == null)
                return false;

            _context.LeagueSettings.Remove(settings);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
