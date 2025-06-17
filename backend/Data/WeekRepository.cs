using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface IWeekRepository
    {
        Task<IEnumerable<Week>> GetAllAsync();
        Task<Week?> GetByIdAsync(Guid id);
        Task<IEnumerable<Week>> GetWeeksBySeasonIdAsync(Guid seasonId);
        Task<Week> CreateAsync(Week week);
        Task<Week> UpdateAsync(Week week);
        Task<bool> DeleteAsync(Guid id);
    }

    public class WeekRepository : IWeekRepository
    {
        private readonly AppDbContext _context;

        public WeekRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Week>> GetAllAsync()
        {
            return await _context.Weeks
                .Include(w => w.Season)
                .OrderBy(w => w.Season!.Year)
                .ThenBy(w => w.WeekNumber)
                .ToListAsync();
        }

        public async Task<Week?> GetByIdAsync(Guid id)
        {
            return await _context.Weeks
                .Include(w => w.Season)
                .Include(w => w.ScoreEntries)
                .ThenInclude(se => se.Player)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        public async Task<IEnumerable<Week>> GetWeeksBySeasonIdAsync(Guid seasonId)
        {
            return await _context.Weeks
                .Where(w => w.SeasonId == seasonId)
                .Include(w => w.Season)
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();
        }

        public async Task<Week> CreateAsync(Week week)
        {
            week.Id = Guid.NewGuid();
            _context.Weeks.Add(week);
            await _context.SaveChangesAsync();
            
            // Return the created week with includes
            return await GetByIdAsync(week.Id) ?? week;
        }

        public async Task<Week> UpdateAsync(Week week)
        {
            _context.Weeks.Update(week);
            await _context.SaveChangesAsync();
            
            // Return the updated week with includes
            return await GetByIdAsync(week.Id) ?? week;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var week = await _context.Weeks.FindAsync(id);
            if (week == null) return false;

            _context.Weeks.Remove(week);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
