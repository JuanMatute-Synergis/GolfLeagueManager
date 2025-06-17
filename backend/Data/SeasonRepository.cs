using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public interface ISeasonRepository
    {
        void AddSeason(Season season);
        IEnumerable<Season> GetSeasons();
        Season? GetSeasonById(Guid id);
        Task<Season?> GetByIdAsync(Guid id);
        bool DeleteSeason(Guid id);
        bool UpdateSeason(Season season);
        IEnumerable<Season> GetActiveSeasons();
    }

    public class SeasonRepository : ISeasonRepository
    {
        private readonly AppDbContext _context;

        public SeasonRepository(AppDbContext context)
        {
            _context = context;
        }

        public void AddSeason(Season season)
        {
            _context.Seasons.Add(season);
            _context.SaveChanges();
        }

        public IEnumerable<Season> GetSeasons()
        {
            return _context.Seasons.OrderBy(s => s.Year).ThenBy(s => s.SeasonNumber).ToList();
        }

        public Season? GetSeasonById(Guid id)
        {
            return _context.Seasons.Find(id);
        }

        public bool DeleteSeason(Guid id)
        {
            var season = _context.Seasons.Find(id);
            if (season == null)
                return false;
            
            _context.Seasons.Remove(season);
            _context.SaveChanges();
            return true;
        }

        public bool UpdateSeason(Season season)
        {
            var existingSeason = _context.Seasons.Find(season.Id);
            if (existingSeason == null)
                return false;

            existingSeason.Name = season.Name;
            existingSeason.Year = season.Year;
            existingSeason.SeasonNumber = season.SeasonNumber;
            existingSeason.StartDate = season.StartDate;
            existingSeason.EndDate = season.EndDate;

            _context.SaveChanges();
            return true;
        }

        public IEnumerable<Season> GetActiveSeasons()
        {
            var today = DateTime.Today;
            return _context.Seasons
                .Where(s => s.StartDate <= today && s.EndDate >= today)
                .OrderBy(s => s.Year)
                .ThenBy(s => s.SeasonNumber)
                .ToList();
        }

        public async Task<Season?> GetByIdAsync(Guid id)
        {
            return await _context.Seasons.FindAsync(id);
        }
    }
}
