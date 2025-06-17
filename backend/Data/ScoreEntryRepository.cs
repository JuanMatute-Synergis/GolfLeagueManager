using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface IScoreEntryRepository
    {
        Task<IEnumerable<ScoreEntry>> GetAllAsync();
        Task<ScoreEntry?> GetByIdAsync(Guid id);
        Task<IEnumerable<ScoreEntry>> GetScoreEntriesByWeekIdAsync(Guid weekId);
        Task<IEnumerable<ScoreEntry>> GetScoreEntriesByPlayerIdAsync(Guid playerId);
        Task<IEnumerable<ScoreEntry>> GetScoreEntriesBySeasonIdAsync(Guid seasonId);
        Task<ScoreEntry?> GetScoreEntryByPlayerAndWeekAsync(Guid playerId, Guid weekId);
        Task<ScoreEntry> CreateAsync(ScoreEntry scoreEntry);
        Task<ScoreEntry> UpdateAsync(ScoreEntry scoreEntry);
        Task<bool> DeleteAsync(Guid id);
    }

    public class ScoreEntryRepository : IScoreEntryRepository
    {
        private readonly AppDbContext _context;

        public ScoreEntryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ScoreEntry>> GetAllAsync()
        {
            return await _context.ScoreEntries
                .Include(se => se.Player)
                .Include(se => se.Week)
                .ThenInclude(w => w!.Season)
                .OrderBy(se => se.Week!.Season!.Year)
                .ThenBy(se => se.Week!.WeekNumber)
                .ThenBy(se => se.Score)
                .ToListAsync();
        }

        public async Task<ScoreEntry?> GetByIdAsync(Guid id)
        {
            return await _context.ScoreEntries
                .Include(se => se.Player)
                .Include(se => se.Week)
                .ThenInclude(w => w!.Season)
                .FirstOrDefaultAsync(se => se.Id == id);
        }

        public async Task<IEnumerable<ScoreEntry>> GetScoreEntriesByWeekIdAsync(Guid weekId)
        {
            return await _context.ScoreEntries
                .Where(se => se.WeekId == weekId)
                .Include(se => se.Player)
                .Include(se => se.Week)
                .OrderBy(se => se.Score)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScoreEntry>> GetScoreEntriesByPlayerIdAsync(Guid playerId)
        {
            return await _context.ScoreEntries
                .Where(se => se.PlayerId == playerId)
                .Include(se => se.Player)
                .Include(se => se.Week)
                .ThenInclude(w => w!.Season)
                .OrderBy(se => se.Week!.Season!.Year)
                .ThenBy(se => se.Week!.WeekNumber)
                .ToListAsync();
        }

        public async Task<IEnumerable<ScoreEntry>> GetScoreEntriesBySeasonIdAsync(Guid seasonId)
        {
            return await _context.ScoreEntries
                .Where(se => se.Week!.SeasonId == seasonId)
                .Include(se => se.Player)
                .Include(se => se.Week)
                .OrderBy(se => se.Week!.WeekNumber)
                .ThenBy(se => se.Score)
                .ToListAsync();
        }

        public async Task<ScoreEntry?> GetScoreEntryByPlayerAndWeekAsync(Guid playerId, Guid weekId)
        {
            return await _context.ScoreEntries
                .Include(se => se.Player)
                .Include(se => se.Week)
                .FirstOrDefaultAsync(se => se.PlayerId == playerId && se.WeekId == weekId);
        }

        public async Task<ScoreEntry> CreateAsync(ScoreEntry scoreEntry)
        {
            scoreEntry.Id = Guid.NewGuid();
            _context.ScoreEntries.Add(scoreEntry);
            await _context.SaveChangesAsync();
            
            // Return the created score entry with includes
            return await GetByIdAsync(scoreEntry.Id) ?? scoreEntry;
        }

        public async Task<ScoreEntry> UpdateAsync(ScoreEntry scoreEntry)
        {
            _context.ScoreEntries.Update(scoreEntry);
            await _context.SaveChangesAsync();
            
            // Return the updated score entry with includes
            return await GetByIdAsync(scoreEntry.Id) ?? scoreEntry;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var scoreEntry = await _context.ScoreEntries.FindAsync(id);
            if (scoreEntry == null) return false;

            _context.ScoreEntries.Remove(scoreEntry);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
