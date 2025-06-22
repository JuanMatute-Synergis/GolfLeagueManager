using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GolfLeagueManager.Business;
using GolfLeagueManager;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StandingsController : ControllerBase
    {
        private readonly PlayerFlightAssignmentService _flightAssignmentService;
        private readonly FlightService _flightService;
        private readonly PlayerService _playerService;
        private readonly WeekService _weekService;
        private readonly ScorecardService _scorecardService;
        private readonly MatchPlayService _matchPlayService;
        private readonly AverageScoreService _averageScoreService;

        public StandingsController(
            PlayerFlightAssignmentService flightAssignmentService,
            FlightService flightService,
            PlayerService playerService,
            WeekService weekService,
            ScorecardService scorecardService,
            MatchPlayService matchPlayService,
            AverageScoreService averageScoreService)
        {
            _flightAssignmentService = flightAssignmentService;
            _flightService = flightService;
            _playerService = playerService;
            _weekService = weekService;
            _scorecardService = scorecardService;
            _matchPlayService = matchPlayService;
            _averageScoreService = averageScoreService;
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklyStandings(Guid seasonId, Guid weekId)
        {
            // Get all flights for the season
            var flights = await _flightService.GetFlightsBySeasonIdAsync(seasonId);
            // Get all player assignments for the season
            var assignments = _flightAssignmentService.GetAllAssignments()
                .Where(a => a.Flight != null && a.Flight.SeasonId == seasonId)
                .ToList();
            // Get all players
            var players = await _playerService.GetAllPlayersAsync();
            // Get all weeks up to and including the selected week
            var weeks = await _weekService.GetWeeksBySeasonIdAsync(seasonId);
            var weekOrder = weeks.OrderBy(w => w.WeekNumber).ToList();
            var selectedWeek = weekOrder.FirstOrDefault(w => w.Id == weekId);
            if (selectedWeek == null)
                return BadRequest("Invalid weekId");
            var weekIdx = weekOrder.IndexOf(selectedWeek);
            var weekIdsUpTo = weekOrder.Take(weekIdx + 1).Select(w => w.Id).ToList();

            // Get all matchups for the season and week
            var matchups = await _scorecardService.GetAllMatchupsForSeasonAsync(seasonId);
            var matchupsThisWeek = matchups.Where(m => m.WeekId == weekId).ToList();
            var matchupsUpTo = matchups.Where(m => weekIdsUpTo.Contains(m.WeekId)).ToList();

            // Get all hole scores for the season
            var allHoleScores = await _scorecardService.GetAllHoleScoresForSeasonAsync(seasonId);

            // Build standings per flight
            var result = new List<object>();
            foreach (var flight in flights)
            {
                var flightPlayers = assignments.Where(a => a.FlightId == flight.Id)
                    .Select(a => players.FirstOrDefault(p => p.Id == a.PlayerId))
                    .Where(p => p != null)
                    .ToList();
                var playerStats = new List<object>();
                foreach (var player in flightPlayers)
                {
                    if (player == null) continue;
                    // Find all matchups for this player up to this week
                    var playerMatchupsUpTo = matchupsUpTo.Where(m => m != null && (m.PlayerAId == player.Id || m.PlayerBId == player.Id)).ToList();
                    // Aggregate gross scores up to this week
                    var grossScores = new List<int>();
                    foreach (var matchup in playerMatchupsUpTo) {
                        if (matchup == null) continue;
                        var hs = allHoleScores.Where(h => h.MatchupId == matchup.Id).ToList();
                        if (matchup.PlayerAId == player.Id)
                            grossScores.Add(hs.Sum(h => h.PlayerAScore ?? 0));
                        else if (matchup.PlayerBId == player.Id)
                            grossScores.Add(hs.Sum(h => h.PlayerBScore ?? 0));
                    }
                    decimal? average = grossScores.Any() ? (decimal?)grossScores.Average() : null;

                    // This week's scores
                    var playerMatchupsThisWeek = matchupsThisWeek.Where(m => m != null && (m.PlayerAId == player.Id || m.PlayerBId == player.Id)).ToList();
                    int? gross = null;
                    int? net = null;
                    if (playerMatchupsThisWeek.Count > 0) {
                        int grossSum = 0;
                        foreach (var matchup in playerMatchupsThisWeek) {
                            if (matchup == null) continue;
                            var hs = allHoleScores.Where(h => h.MatchupId == matchup.Id).ToList();
                            if (matchup.PlayerAId == player.Id) {
                                grossSum += hs.Sum(h => h.PlayerAScore ?? 0);
                            } else if (matchup.PlayerBId == player.Id) {
                                grossSum += hs.Sum(h => h.PlayerBScore ?? 0);
                            }
                        }
                        gross = grossSum;
                        net = grossSum; // No net score available, use gross
                        if (playerMatchupsThisWeek.Count == 0) { gross = null; net = null; }
                    }

                    // Match play points for this week and accumulated
                    int weekPoints = matchupsThisWeek.Where(m => m != null && m.PlayerAId == player.Id).Sum(m => m.PlayerAPoints ?? 0)
                        + matchupsThisWeek.Where(m => m != null && m.PlayerBId == player.Id).Sum(m => m.PlayerBPoints ?? 0);
                    int accumPoints = matchupsUpTo.Where(m => m != null && m.PlayerAId == player.Id).Sum(m => m.PlayerAPoints ?? 0)
                        + matchupsUpTo.Where(m => m != null && m.PlayerBId == player.Id).Sum(m => m.PlayerBPoints ?? 0);

                    playerStats.Add(new {
                        id = player.Id,
                        name = player.FirstName + " " + player.LastName,
                        gross = gross,
                        net = net,
                        average = average,
                        weekPoints = weekPoints,
                        accumPoints = accumPoints
                    });
                }
                result.Add(new {
                    id = flight.Id,
                    name = flight.Name,
                    players = playerStats.OrderByDescending(p => ((dynamic)p).accumPoints).ToList()
                });
            }
            return Ok(new { flights = result });
        }
    }
}
