using System.Collections.Generic;

namespace GolfLeagueManager
{
    public class PlayerService
    {
        private readonly IPlayerRepository _playerRepository;

        public PlayerService(IPlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public void AddPlayer(Player player)
        {
            if (player.Id == Guid.Empty)
            {
                player.Id = Guid.NewGuid();
            }
            _playerRepository.AddPlayer(player);
        }

        public IEnumerable<Player> GetPlayers()
        {
            return _playerRepository.GetPlayers();
        }

        public bool DeletePlayer(Guid id)
        {
            return _playerRepository.DeletePlayer(id);
        }

        public bool UpdatePlayer(Player player)
        {
            return _playerRepository.UpdatePlayer(player);
        }

        // Additional business logic methods can be added here
    }

    // IDataAccess and InMemoryDataAccess are now obsolete and can be removed if not needed for testing.
}
