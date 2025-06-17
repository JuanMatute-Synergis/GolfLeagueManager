namespace GolfLeagueManager
{
    public class SeasonService
    {
        private readonly ISeasonRepository _seasonRepository;
        private readonly WeekService _weekService;

        public SeasonService(ISeasonRepository seasonRepository, WeekService weekService)
        {
            _seasonRepository = seasonRepository;
            _weekService = weekService;
        }

        public async Task AddSeasonAsync(Season season)
        {
            // Business logic validation
            ValidateSeason(season);
            _seasonRepository.AddSeason(season);

            // Automatically generate weeks for the season
            await _weekService.GenerateWeeksForSeasonAsync(season.Id);
        }

        public void AddSeason(Season season)
        {
            // Synchronous wrapper for backward compatibility
            AddSeasonAsync(season).GetAwaiter().GetResult();
        }

        public IEnumerable<Season> GetSeasons()
        {
            return _seasonRepository.GetSeasons();
        }

        public Season? GetSeasonById(Guid id)
        {
            return _seasonRepository.GetSeasonById(id);
        }

        public bool DeleteSeason(Guid id)
        {
            return _seasonRepository.DeleteSeason(id);
        }

        public bool UpdateSeason(Season season)
        {
            ValidateSeason(season);
            return _seasonRepository.UpdateSeason(season);
        }

        public IEnumerable<Season> GetActiveSeasons()
        {
            return _seasonRepository.GetActiveSeasons();
        }

        private void ValidateSeason(Season season)
        {
            if (string.IsNullOrWhiteSpace(season.Name))
                throw new ArgumentException("Season name is required.");

            if (season.Year < 2000 || season.Year > 2100)
                throw new ArgumentException("Year must be between 2000 and 2100.");

            if (season.SeasonNumber < 1)
                throw new ArgumentException("Season number must be greater than 0.");

            if (season.StartDate >= season.EndDate)
                throw new ArgumentException("Start date must be before end date.");
        }
    }
}
