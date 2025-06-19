using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/score-calculation")]
    public class ScoreCalculationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MatchPlayScoringService _scoringService;

        public ScoreCalculationController(AppDbContext context, MatchPlayScoringService scoringService)
        {
            _context = context;
            _scoringService = scoringService;
        }

        /// <summary>
        /// Calculate net score for a specific hole
        /// </summary>
        [HttpPost("net-score")]
        public ActionResult<NetScoreResponse> CalculateNetScore([FromBody] NetScoreRequest request)
        {
            try
            {
                if (request.GrossScore <= 0 || request.Handicap < 0 || request.OpponentHandicap < 0)
                {
                    return BadRequest("Invalid score or handicap values");
                }

                var netScore = _scoringService.CalculateNetScore(
                    request.GrossScore, 
                    request.Handicap, 
                    request.OpponentHandicap, 
                    request.HoleHandicap
                );

                return Ok(new NetScoreResponse
                {
                    NetScore = netScore,
                    StrokesReceived = _scoringService.GetStrokesForHole(request.Handicap, request.OpponentHandicap, request.HoleHandicap)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating net score: {ex.Message}");
            }
        }

        /// <summary>
        /// Calculate match play results for hole scores
        /// </summary>
        [HttpPost("match-play")]
        public ActionResult<MatchPlayCalculationResponse> CalculateMatchPlay([FromBody] MatchPlayCalculationRequest request)
        {
            try
            {
                // Convert request to HoleScore objects for calculation
                var holeScores = request.HoleScores.Select(h => new HoleScore
                {
                    HoleNumber = h.HoleNumber,
                    PlayerAScore = h.PlayerAScore,
                    PlayerBScore = h.PlayerBScore
                }).ToList();

                var playerAGrossTotal = holeScores.Where(h => h.PlayerAScore.HasValue).Sum(h => h.PlayerAScore!.Value);
                var playerBGrossTotal = holeScores.Where(h => h.PlayerBScore.HasValue).Sum(h => h.PlayerBScore!.Value);

                var result = _scoringService.CalculateMatchPlayResult(
                    holeScores,
                    request.PlayerAHandicap,
                    request.PlayerBHandicap,
                    playerAGrossTotal,
                    playerBGrossTotal
                );

                return Ok(new MatchPlayCalculationResponse
                {
                    PlayerAHolePoints = result.PlayerAHolePoints,
                    PlayerBHolePoints = result.PlayerBHolePoints,
                    PlayerATotalPoints = result.PlayerATotalPoints,
                    PlayerBTotalPoints = result.PlayerBTotalPoints,
                    PlayerAMatchWin = result.PlayerAMatchWin,
                    PlayerBMatchWin = result.PlayerBMatchWin,
                    HoleResults = result.HoleResults.Select(hr => new HoleResultDto
                    {
                        HoleNumber = hr.HoleNumber,
                        PlayerAPoints = hr.PlayerAPoints,
                        PlayerBPoints = hr.PlayerBPoints,
                        Winner = hr.Winner
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating match play: {ex.Message}");
            }
        }

        /// <summary>
        /// Get calculated scores for a matchup
        /// </summary>
        [HttpGet("matchup/{matchupId}")]
        public async Task<ActionResult<MatchupScoresResponse>> GetMatchupScores(Guid matchupId)
        {
            try
            {
                var matchup = await _context.Matchups
                    .Include(m => m.PlayerA)
                    .Include(m => m.PlayerB)
                    .FirstOrDefaultAsync(m => m.Id == matchupId);

                if (matchup == null)
                {
                    return NotFound("Matchup not found");
                }

                var holeScores = await _context.HoleScores
                    .Where(hs => hs.MatchupId == matchupId)
                    .OrderBy(hs => hs.HoleNumber)
                    .ToListAsync();

                var response = new MatchupScoresResponse
                {
                    MatchupId = matchupId,
                    PlayerAGrossScore = (int?)matchup.PlayerAScore,
                    PlayerBGrossScore = (int?)matchup.PlayerBScore,
                    PlayerANetScore = null,
                    PlayerBNetScore = null,
                    PlayerAHandicap = (int)(matchup.PlayerA?.CurrentHandicap ?? 0),
                    PlayerBHandicap = (int)(matchup.PlayerB?.CurrentHandicap ?? 0),
                    PlayerAPoints = matchup.PlayerAPoints,
                    PlayerBPoints = matchup.PlayerBPoints,
                    HoleScores = new List<HoleScoreDto>()
                };

                // Calculate net scores if we have gross scores and handicaps
                if (matchup.PlayerAScore.HasValue && matchup.PlayerBScore.HasValue)
                {
                    var playerAHandicap = matchup.PlayerA?.CurrentHandicap ?? 0;
                    var playerBHandicap = matchup.PlayerB?.CurrentHandicap ?? 0;
                    
                    // Simple net score calculation for overall scores
                    var handicapDiff = Math.Abs(playerAHandicap - playerBHandicap);
                    
                    if (playerAHandicap > playerBHandicap)
                    {
                        response.PlayerANetScore = matchup.PlayerAScore - (int)handicapDiff;
                        response.PlayerBNetScore = matchup.PlayerBScore;
                    }
                    else if (playerBHandicap > playerAHandicap)
                    {
                        response.PlayerANetScore = matchup.PlayerAScore;
                        response.PlayerBNetScore = matchup.PlayerBScore - (int)handicapDiff;
                    }
                    else
                    {
                        response.PlayerANetScore = matchup.PlayerAScore;
                        response.PlayerBNetScore = matchup.PlayerBScore;
                    }
                }

                // Add hole scores with calculations
                foreach (var holeScore in holeScores)
                {
                    var holeDto = new HoleScoreDto
                    {
                        HoleNumber = holeScore.HoleNumber,
                        PlayerAGrossScore = holeScore.PlayerAScore,
                        PlayerBGrossScore = holeScore.PlayerBScore,
                        PlayerAMatchPoints = holeScore.PlayerAMatchPoints,
                        PlayerBMatchPoints = holeScore.PlayerBMatchPoints
                    };

                    // Calculate net scores for this hole if we have the data
                    if (holeScore.PlayerAScore.HasValue && holeScore.PlayerBScore.HasValue)
                    {
                        var courseHole = await _context.CourseHoles
                            .FirstOrDefaultAsync(ch => ch.HoleNumber == holeScore.HoleNumber);
                        
                        if (courseHole != null)
                        {
                            var playerAHandicap = (int)(matchup.PlayerA?.CurrentHandicap ?? 0);
                            var playerBHandicap = (int)(matchup.PlayerB?.CurrentHandicap ?? 0);

                            holeDto.PlayerANetScore = _scoringService.CalculateNetScore(
                                holeScore.PlayerAScore.Value, 
                                playerAHandicap, 
                                playerBHandicap, 
                                courseHole.HandicapIndex
                            );

                            holeDto.PlayerBNetScore = _scoringService.CalculateNetScore(
                                holeScore.PlayerBScore.Value, 
                                playerBHandicap, 
                                playerAHandicap, 
                                courseHole.HandicapIndex
                            );
                        }
                    }

                    response.HoleScores.Add(holeDto);
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error getting matchup scores: {ex.Message}");
            }
        }
    }

    // Request/Response DTOs
    public class NetScoreRequest
    {
        public int GrossScore { get; set; }
        public int Handicap { get; set; }
        public int OpponentHandicap { get; set; }
        public int HoleHandicap { get; set; }
    }

    public class NetScoreResponse
    {
        public int NetScore { get; set; }
        public int StrokesReceived { get; set; }
    }

    public class MatchPlayCalculationRequest
    {
        public List<HoleScoreInput> HoleScores { get; set; } = new();
        public int PlayerAHandicap { get; set; }
        public int PlayerBHandicap { get; set; }
    }

    public class HoleScoreInput
    {
        public int HoleNumber { get; set; }
        public int? PlayerAScore { get; set; }
        public int? PlayerBScore { get; set; }
    }

    public class MatchPlayCalculationResponse
    {
        public int PlayerAHolePoints { get; set; }
        public int PlayerBHolePoints { get; set; }
        public int PlayerATotalPoints { get; set; }
        public int PlayerBTotalPoints { get; set; }
        public bool PlayerAMatchWin { get; set; }
        public bool PlayerBMatchWin { get; set; }
        public List<HoleResultDto> HoleResults { get; set; } = new();
    }

    public class HoleResultDto
    {
        public int HoleNumber { get; set; }
        public int PlayerAPoints { get; set; }
        public int PlayerBPoints { get; set; }
        public string Winner { get; set; } = "";
    }

    public class MatchupScoresResponse
    {
        public Guid MatchupId { get; set; }
        public int? PlayerAGrossScore { get; set; }
        public int? PlayerBGrossScore { get; set; }
        public int? PlayerANetScore { get; set; }
        public int? PlayerBNetScore { get; set; }
        public int PlayerAHandicap { get; set; }
        public int PlayerBHandicap { get; set; }
        public int? PlayerAPoints { get; set; }
        public int? PlayerBPoints { get; set; }
        public List<HoleScoreDto> HoleScores { get; set; } = new();
    }

    public class HoleScoreDto
    {
        public int HoleNumber { get; set; }
        public int? PlayerAGrossScore { get; set; }
        public int? PlayerBGrossScore { get; set; }
        public int? PlayerANetScore { get; set; }
        public int? PlayerBNetScore { get; set; }
        public int? PlayerAMatchPoints { get; set; }
        public int? PlayerBMatchPoints { get; set; }
    }
}
