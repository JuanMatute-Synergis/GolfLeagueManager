namespace GolfLeagueManager
{
    public interface IPlayerRepository
    {
        void AddPlayer(GolfLeagueManager.Player player);
        IEnumerable<GolfLeagueManager.Player> GetPlayers();
        Task<Player?> GetByIdAsync(Guid id);
        bool DeletePlayer(Guid id);
        bool UpdatePlayer(GolfLeagueManager.Player player);
        // Additional repository methods can be added here
    }

    public class PlayerRepository : IPlayerRepository
    {
        private readonly GolfLeagueManager.AppDbContext _context;

        public PlayerRepository(GolfLeagueManager.AppDbContext context)
        {
            _context = context;
        }

        public void AddPlayer(GolfLeagueManager.Player player)
        {
            _context.Players.Add(player);
            _context.SaveChanges();
        }

        public IEnumerable<GolfLeagueManager.Player> GetPlayers()
        {
            return _context.Players.ToList();
        }

        public bool DeletePlayer(Guid id)
        {
            var player = _context.Players.Find(id);
            if (player == null)
                return false;
            _context.Players.Remove(player);
            _context.SaveChanges();
            return true;
        }

        public bool UpdatePlayer(GolfLeagueManager.Player player)
        {
            var existingPlayer = _context.Players.Find(player.Id);
            if (existingPlayer == null)
                return false;
            
            existingPlayer.FirstName = player.FirstName;
            existingPlayer.LastName = player.LastName;
            existingPlayer.Email = player.Email;
            existingPlayer.Phone = player.Phone;
            
            _context.SaveChanges();
            return true;
        }

        public async Task<Player?> GetByIdAsync(Guid id)
        {
            return await _context.Players.FindAsync(id);
        }

        // Additional repository methods can be implemented here
    }
}
