using GolfLeagueManager.Models;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Services
{
    public interface ILeagueRulesService
    {
        Task<LeagueRules?> GetRulesBySeasonIdAsync(Guid seasonId);
        Task<LeagueRules> CreateOrUpdateRulesAsync(Guid seasonId, string content, string? updatedBy = null);
        Task DeleteRulesAsync(Guid seasonId);
    }

    public class LeagueRulesService : ILeagueRulesService
    {
        private readonly AppDbContext _context;

        public LeagueRulesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<LeagueRules?> GetRulesBySeasonIdAsync(Guid seasonId)
        {
            return await _context.LeagueRules
                .Include(lr => lr.Season)
                .FirstOrDefaultAsync(lr => lr.SeasonId == seasonId);
        }

        public async Task<LeagueRules> CreateOrUpdateRulesAsync(Guid seasonId, string content, string? updatedBy = null)
        {
            var existingRules = await _context.LeagueRules
                .FirstOrDefaultAsync(lr => lr.SeasonId == seasonId);

            if (existingRules != null)
            {
                // Update existing rules
                existingRules.Content = content;
                existingRules.UpdatedAt = DateTime.UtcNow;
                existingRules.UpdatedBy = updatedBy;
                
                _context.LeagueRules.Update(existingRules);
                await _context.SaveChangesAsync();
                
                return existingRules;
            }
            else
            {
                // Create new rules
                var newRules = new LeagueRules
                {
                    Id = Guid.NewGuid(),
                    SeasonId = seasonId,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    CreatedBy = updatedBy,
                    UpdatedBy = updatedBy
                };

                _context.LeagueRules.Add(newRules);
                await _context.SaveChangesAsync();
                
                return newRules;
            }
        }

        public async Task DeleteRulesAsync(Guid seasonId)
        {
            var rules = await _context.LeagueRules
                .FirstOrDefaultAsync(lr => lr.SeasonId == seasonId);

            if (rules != null)
            {
                _context.LeagueRules.Remove(rules);
                await _context.SaveChangesAsync();
            }
        }
    }
}
