using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class WeekService
    {
        private readonly IWeekRepository _weekRepository;
        private readonly ISeasonRepository _seasonRepository;

        public WeekService(IWeekRepository weekRepository, ISeasonRepository seasonRepository)
        {
            _weekRepository = weekRepository;
            _seasonRepository = seasonRepository;
        }

        public async Task<IEnumerable<Week>> GetAllWeeksAsync()
        {
            return await _weekRepository.GetAllAsync();
        }

        public async Task<Week?> GetWeekByIdAsync(Guid id)
        {
            return await _weekRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Week>> GetWeeksBySeasonIdAsync(Guid seasonId)
        {
            return await _weekRepository.GetWeeksBySeasonIdAsync(seasonId);
        }

        public async Task<Week> CreateWeekAsync(Week week)
        {
            // Validate season exists
            var season = await _seasonRepository.GetByIdAsync(week.SeasonId);
            if (season == null)
            {
                throw new ArgumentException("Season not found");
            }

            // Auto-generate week number if not provided
            if (week.WeekNumber <= 0)
            {
                var existingWeeks = await GetWeeksBySeasonIdAsync(week.SeasonId);
                week.WeekNumber = existingWeeks.Any() ? existingWeeks.Max(w => w.WeekNumber) + 1 : 1;
            }

            // Auto-generate name if not provided
            if (string.IsNullOrEmpty(week.Name))
            {
                week.Name = $"Week {week.WeekNumber}";
            }

            return await _weekRepository.CreateAsync(week);
        }

        public async Task<Week> UpdateWeekAsync(Week week)
        {
            var existingWeek = await _weekRepository.GetByIdAsync(week.Id);
            if (existingWeek == null)
            {
                throw new ArgumentException("Week not found");
            }

            return await _weekRepository.UpdateAsync(week);
        }

        public async Task<bool> DeleteWeekAsync(Guid id)
        {
            return await _weekRepository.DeleteAsync(id);
        }

        public async Task<Week?> GetCurrentWeekAsync(Guid seasonId)
        {
            var weeks = await GetWeeksBySeasonIdAsync(seasonId);
            var today = DateTime.UtcNow.Date;
            
            // Find the week that matches today's date, or the closest past week
            return weeks.Where(w => w.IsActive && w.Date.Date <= today)
                       .OrderByDescending(w => w.Date)
                       .FirstOrDefault();
        }

        public async Task<Week?> GetNextWeekAsync(Guid seasonId)
        {
            var weeks = await GetWeeksBySeasonIdAsync(seasonId);
            var today = DateTime.UtcNow.Date;
            
            return weeks.Where(w => w.Date.Date > today)
                       .OrderBy(w => w.Date)
                       .FirstOrDefault();
        }

        public async Task GenerateWeeksForSeasonAsync(Guid seasonId)
        {
            var season = await _seasonRepository.GetByIdAsync(seasonId);
            if (season == null)
            {
                throw new ArgumentException("Season not found");
            }

            // Check if weeks already exist for this season
            var existingWeeks = await GetWeeksBySeasonIdAsync(seasonId);
            if (existingWeeks.Any())
            {
                return; // Don't generate if weeks already exist
            }

            var startDate = season.StartDate.Date; // Ensure we're working with dates only
            var endDate = season.EndDate.Date;
            var weekNumber = 1;

            // Find the first Wednesday on or after the start date
            var currentDay = startDate;

            // Generate a week for each Wednesday within the season date range
            while (currentDay <= endDate)
            {
                var week = new Week
                {
                    Id = Guid.NewGuid(),
                    SeasonId = seasonId,
                    WeekNumber = weekNumber,
                    Name = $"Week {weekNumber}",
                    Date = DateTime.SpecifyKind(currentDay, DateTimeKind.Utc),
                    IsActive = true
                };

                await _weekRepository.CreateAsync(week);
                
                currentDay = currentDay.AddDays(7); // Move to next Wednesday
                weekNumber++;
            }
        }
    }
}
