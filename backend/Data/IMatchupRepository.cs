namespace GolfLeagueManager
{
    public interface IMatchupRepository
    {
        Task<IEnumerable<Matchup>> GetAllAsync();
        Task<Matchup?> GetByIdAsync(Guid id);
        Task<IEnumerable<Matchup>> GetByWeekIdAsync(Guid weekId);
        Task<IEnumerable<Matchup>> GetBySeasonIdAsync(Guid seasonId);
        Task<IEnumerable<Matchup>> GetByPlayerIdAsync(Guid playerId);
        Task<Matchup> CreateAsync(Matchup matchup);
        Task<Matchup> UpdateAsync(Matchup matchup);
        Task<bool> DeleteAsync(Guid id);
    }
}
