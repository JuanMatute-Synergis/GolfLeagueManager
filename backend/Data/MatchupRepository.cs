using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class MatchupRepository : IMatchupRepository
    {
        private readonly AppDbContext _context;

        public MatchupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Matchup>> GetAllAsync()
        {
            return await _context.Matchups
                .Include(m => m.Week)
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .OrderBy(m => m.Week!.WeekNumber)
                .ToListAsync();
        }

        public async Task<Matchup?> GetByIdAsync(Guid id)
        {
            return await _context.Matchups
                .Include(m => m.Week)
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<IEnumerable<Matchup>> GetByWeekIdAsync(Guid weekId)
        {
            return await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Where(m => m.WeekId == weekId)
                .OrderBy(m => m.PlayerA!.LastName)
                .ThenBy(m => m.PlayerA!.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Matchup>> GetBySeasonIdAsync(Guid seasonId)
        {
            return await _context.Matchups
                .Include(m => m.Week)
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Where(m => m.Week!.SeasonId == seasonId)
                .OrderBy(m => m.Week!.WeekNumber)
                .ThenBy(m => m.PlayerA!.LastName)
                .ThenBy(m => m.PlayerA!.FirstName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Matchup>> GetByPlayerIdAsync(Guid playerId)
        {
            return await _context.Matchups
                .Include(m => m.Week)
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Where(m => m.PlayerAId == playerId || m.PlayerBId == playerId)
                .OrderBy(m => m.Week!.WeekNumber)
                .ToListAsync();
        }

        public async Task<Matchup> CreateAsync(Matchup matchup)
        {
            matchup.Id = Guid.NewGuid();
            _context.Matchups.Add(matchup);
            await _context.SaveChangesAsync();
            
            // Return the created matchup with includes
            return await GetByIdAsync(matchup.Id) ?? matchup;
        }

        public async Task<Matchup> UpdateAsync(Matchup matchup)
        {
            _context.Matchups.Update(matchup);
            await _context.SaveChangesAsync();
            
            // Return the updated matchup with includes
            return await GetByIdAsync(matchup.Id) ?? matchup;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var matchup = await _context.Matchups.FindAsync(id);
            if (matchup == null) return false;

            _context.Matchups.Remove(matchup);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
