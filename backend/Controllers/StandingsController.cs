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
        private readonly HandicapService _handicapService;
        private readonly AppDbContext _context;

        public StandingsController(
            PlayerFlightAssignmentService flightAssignmentService,
            FlightService flightService,
            PlayerService playerService,
            WeekService weekService,
            ScorecardService scorecardService,
            MatchPlayService matchPlayService,
            AverageScoreService averageScoreService,
            HandicapService handicapService,
            AppDbContext context)
        {
            _flightAssignmentService = flightAssignmentService;
            _flightService = flightService;
            _playerService = playerService;
            _weekService = weekService;
            _scorecardService = scorecardService;
            _matchPlayService = matchPlayService;
            _averageScoreService = averageScoreService;
            _handicapService = handicapService;
            _context = context;
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

        [HttpGet("session")]
        public async Task<IActionResult> GetSessionStandings(Guid seasonId, Guid weekId)
        {
            // Get the selected week to determine session
            var allWeeks = await _weekService.GetWeeksBySeasonIdAsync(seasonId);
            var selectedWeek = allWeeks.FirstOrDefault(w => w.Id == weekId);
            if (selectedWeek == null)
                return BadRequest("Invalid weekId");

            // Find the current session start week
            var currentWeekNumber = selectedWeek.WeekNumber;
            var sessionStartWeek = allWeeks
                .Where(w => w.WeekNumber <= currentWeekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefault();
            
            var sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;
            
            // Get all weeks in the current session up to the selected week
            var weekIdsInSession = allWeeks
                .Where(w => w.WeekNumber >= sessionStartWeekNumber && w.WeekNumber <= currentWeekNumber)
                .Select(w => w.Id)
                .ToList();

            // Get all flights for the season
            var flights = await _flightService.GetFlightsBySeasonIdAsync(seasonId);
            var assignments = _flightAssignmentService.GetAllAssignments()
                .Where(a => a.Flight != null && a.Flight.SeasonId == seasonId)
                .ToList();
            var players = await _playerService.GetAllPlayersAsync();

            // Get all matchups for the session
            var allMatchups = await _scorecardService.GetAllMatchupsForSeasonAsync(seasonId);
            var sessionMatchups = allMatchups.Where(m => weekIdsInSession.Contains(m.WeekId)).ToList();
            var thisWeekMatchups = allMatchups.Where(m => m.WeekId == weekId).ToList();

            // Get all hole scores for the session
            var allHoleScores = await _scorecardService.GetAllHoleScoresForSeasonAsync(seasonId);

            // Build session standings per flight
            var result = new List<object>();
            foreach (var flight in flights.OrderBy(f => f.Name))
            {
                var flightPlayers = assignments.Where(a => a.FlightId == flight.Id)
                    .Select(a => players.FirstOrDefault(p => p.Id == a.PlayerId))
                    .Where(p => p != null)
                    .ToList();

                var playerStats = new List<object>();
                foreach (var player in flightPlayers)
                {
                    if (player == null) continue;

                    // Calculate session points - sum of all match play points in the session
                    var sessionTotal = sessionMatchups
                        .Where(m => m.PlayerAId == player.Id || m.PlayerBId == player.Id)
                        .Sum(m => {
                            // Check if this week has special points
                            var matchWeek = allWeeks.FirstOrDefault(w => w.Id == m.WeekId);
                            var playerAbsent = (m.PlayerAId == player.Id) ? m.PlayerAAbsent : m.PlayerBAbsent;
                            
                            // Handle special points weeks
                            if (matchWeek?.SpecialPointsAwarded != null && matchWeek.SpecialPointsAwarded > 0)
                            {
                                return playerAbsent ? (matchWeek.SpecialPointsAwarded / 2) : matchWeek.SpecialPointsAwarded.Value;
                            }
                            
                            // Regular match play points
                            var hasScore = (m.PlayerAId == player.Id && m.PlayerAScore.HasValue) || 
                                          (m.PlayerBId == player.Id && m.PlayerBScore.HasValue);
                            if (!hasScore) return 0;
                            
                            return (m.PlayerAId == player.Id) ? (m.PlayerAPoints ?? 0) : (m.PlayerBPoints ?? 0);
                        });

                    // This week's points
                    var thisWeekPoints = thisWeekMatchups
                        .Where(m => m.PlayerAId == player.Id || m.PlayerBId == player.Id)
                        .Sum(m => {
                            // Handle special points for this week
                            if (selectedWeek.SpecialPointsAwarded != null && selectedWeek.SpecialPointsAwarded > 0)
                            {
                                var playerAbsent = (m.PlayerAId == player.Id) ? m.PlayerAAbsent : m.PlayerBAbsent;
                                return playerAbsent ? (selectedWeek.SpecialPointsAwarded / 2) : selectedWeek.SpecialPointsAwarded.Value;
                            }
                            
                            return (m.PlayerAId == player.Id) ? (m.PlayerAPoints ?? 0) : (m.PlayerBPoints ?? 0);
                        });

                    // This week's gross score
                    var thisWeekMatchup = thisWeekMatchups.FirstOrDefault(m => m.PlayerAId == player.Id || m.PlayerBId == player.Id);
                    int? grossScore = null;
                    bool isAbsent = false;
                    
                    if (thisWeekMatchup != null)
                    {
                        if (thisWeekMatchup.PlayerAId == player.Id)
                        {
                            grossScore = thisWeekMatchup.PlayerAScore;
                            isAbsent = thisWeekMatchup.PlayerAAbsent;
                        }
                        else if (thisWeekMatchup.PlayerBId == player.Id)
                        {
                            grossScore = thisWeekMatchup.PlayerBScore;
                            isAbsent = thisWeekMatchup.PlayerBAbsent;
                        }
                    }

                    // Calculate average score up to and including this week (for display)
                    var averageScore = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        player.Id, seasonId, currentWeekNumber + 1);

                    // Calculate handicap up to and including this week
                    var handicapUpToWeek = await _handicapService.GetPlayerHandicapUpToWeekAsync(
                        player.Id, seasonId, currentWeekNumber);

                    playerStats.Add(new {
                        id = player.Id,
                        name = $"{player.FirstName} {player.LastName.Substring(0, 1)}.",
                        displayName = $"{player.FirstName} {player.LastName.Substring(0, 1)}.",
                        handicap = handicapUpToWeek,
                        averageScore = averageScore,
                        grossScore = grossScore ?? 0,
                        thisWeekPoints = thisWeekPoints,
                        sessionTotal = sessionTotal,
                        isAbsent = isAbsent
                    });
                }

                // Sort by session total (descending), then by last name
                var sortedPlayers = playerStats
                    .OrderByDescending(p => ((dynamic)p).sessionTotal)
                    .ThenBy(p => players.FirstOrDefault(pl => pl.Id == ((dynamic)p).id)?.LastName ?? "")
                    .ToList();

                result.Add(new {
                    id = flight.Id,
                    name = flight.Name,
                    players = sortedPlayers
                });
            }

            // Calculate session number
            var sessionNumber = allWeeks
                .Where(w => w.WeekNumber <= currentWeekNumber && w.SessionStart)
                .Count();
            
            // If no session starts found, it's session 1
            if (sessionNumber == 0) sessionNumber = 1;

            return Ok(new { 
                week = new {
                    id = selectedWeek.Id,
                    name = $"Week {selectedWeek.WeekNumber}",
                    weekNumber = selectedWeek.WeekNumber,
                    date = selectedWeek.Date
                },
                session = new {
                    number = sessionNumber,
                    startWeekNumber = sessionStartWeekNumber,
                    currentWeekNumber = currentWeekNumber
                },
                flights = result 
            });
        }
    }
}
