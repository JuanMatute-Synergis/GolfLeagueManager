using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class ScorecardService
    {
        private readonly AppDbContext _context;
        private readonly MatchupService _matchupService;
        private readonly ScoreEntryService _scoreEntryService;
        private readonly MatchPlayService _matchPlayService;
        private readonly MatchPlayScoringService _matchPlayScoringService;
        private readonly AverageScoreService _averageScoreService;

        public ScorecardService(AppDbContext context, MatchupService matchupService, ScoreEntryService scoreEntryService, MatchPlayService matchPlayService, MatchPlayScoringService matchPlayScoringService, AverageScoreService averageScoreService)
        {
            _context = context;
            _matchupService = matchupService;
            _scoreEntryService = scoreEntryService;
            _matchPlayService = matchPlayService;
            _matchPlayScoringService = matchPlayScoringService;
            _averageScoreService = averageScoreService;
        }

        public async Task<ScorecardResponse> SaveScorecardAsync(ScorecardSaveRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Validate the matchup exists
                var matchup = await _context.Matchups
                    .Include(m => m.Week)
                    .FirstOrDefaultAsync(m => m.Id == request.MatchupId);
                
                if (matchup == null)
                {
                    return new ScorecardResponse
                    {
                        MatchupId = request.MatchupId,
                        Success = false,
                        Message = "Matchup not found"
                    };
                }

                // Delete existing hole scores for this matchup
                var existingHoleScores = await _context.HoleScores
                    .Where(hs => hs.MatchupId == request.MatchupId)
                    .ToListAsync();
                
                _context.HoleScores.RemoveRange(existingHoleScores);
                
                // Save the deletion to ensure clean state
                await _context.SaveChangesAsync();

                // Create new hole scores directly from the request
                var holeScores = new List<HoleScore>();
                
                foreach (var holeScoreDto in request.HoleScores)
                {
                    var holeScore = new HoleScore
                    {
                        Id = Guid.NewGuid(),
                        MatchupId = request.MatchupId,
                        HoleNumber = holeScoreDto.HoleNumber,
                        Par = holeScoreDto.Par,
                        HoleHandicap = 1, // Will be set properly below
                        PlayerAScore = holeScoreDto.PlayerAScore,
                        PlayerBScore = holeScoreDto.PlayerBScore,
                        PlayerAMatchPoints = 0, // Will be calculated by match play service
                        PlayerBMatchPoints = 0  // Will be calculated by match play service
                    };
                    
                    holeScores.Add(holeScore);
                }

                // Add all hole scores and set proper hole handicaps
                if (holeScores.Any())
                {
                    // Set the proper hole handicaps from the database
                    foreach (var holeScore in holeScores)
                    {
                        // Get the correct handicap for this hole from the database
                        holeScore.HoleHandicap = await _matchPlayScoringService.GetHoleHandicapAsync(holeScore.HoleNumber);
                    }

                    await _context.HoleScores.AddRangeAsync(holeScores);
                }

                // Update the matchup with total scores and absence information
                matchup.PlayerAScore = request.PlayerATotalScore > 0 ? request.PlayerATotalScore : null;
                matchup.PlayerBScore = request.PlayerBTotalScore > 0 ? request.PlayerBTotalScore : null;
                matchup.PlayerAAbsent = request.PlayerAAbsent;
                matchup.PlayerBAbsent = request.PlayerBAbsent;
                matchup.PlayerAAbsentWithNotice = request.PlayerAAbsentWithNotice;
                matchup.PlayerBAbsentWithNotice = request.PlayerBAbsentWithNotice;

                // Save changes to get hole scores in database
                await _context.SaveChangesAsync();

                // Calculate match play results using the new scoring system
                await _matchPlayService.CalculateMatchPlayResultsAsync(request.MatchupId);

                // Create or update score entries
                if (matchup.Week != null)
                {
                    if (matchup.PlayerAScore.HasValue)
                    {
                        var scoreEntryA = new ScoreEntry
                        {
                            Id = Guid.NewGuid(),
                            PlayerId = matchup.PlayerAId,
                            WeekId = matchup.WeekId,
                            Score = matchup.PlayerAScore.Value,
                            PointsEarned = matchup.PlayerAPoints ?? 0
                        };
                        await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntryA);
                    }

                    if (matchup.PlayerBScore.HasValue)
                    {
                        var scoreEntryB = new ScoreEntry
                        {
                            Id = Guid.NewGuid(),
                            PlayerId = matchup.PlayerBId,
                            WeekId = matchup.WeekId,
                            Score = matchup.PlayerBScore.Value,
                            PointsEarned = matchup.PlayerBPoints ?? 0
                        };
                        await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntryB);
                    }
                }

                await _context.SaveChangesAsync();
                
                // Update average scores for both players after scores are saved
                if (matchup.Week != null && matchup.Week.CountsForScoring)
                {
                    if (matchup.PlayerAScore.HasValue)
                    {
                        await _averageScoreService.UpdatePlayerAverageScoreAsync(matchup.PlayerAId, matchup.Week.SeasonId);
                    }
                    if (matchup.PlayerBScore.HasValue)
                    {
                        await _averageScoreService.UpdatePlayerAverageScoreAsync(matchup.PlayerBId, matchup.Week.SeasonId);
                    }
                }
                
                await transaction.CommitAsync();

                // Reload the matchup to get the calculated match play results
                var updatedMatchup = await _context.Matchups.FindAsync(request.MatchupId);

                return new ScorecardResponse
                {
                    MatchupId = request.MatchupId,
                    Success = true,
                    Message = "Scorecard saved successfully",
                    HoleScores = holeScores,
                    // Include match play results
                    PlayerAMatchPoints = updatedMatchup?.PlayerAPoints,
                    PlayerBMatchPoints = updatedMatchup?.PlayerBPoints,
                    PlayerAHolePoints = updatedMatchup?.PlayerAHolePoints ?? 0,
                    PlayerBHolePoints = updatedMatchup?.PlayerBHolePoints ?? 0,
                    PlayerAMatchWin = updatedMatchup?.PlayerAMatchWin ?? false,
                    PlayerBMatchWin = updatedMatchup?.PlayerBMatchWin ?? false,
                    PlayerAAbsent = updatedMatchup?.PlayerAAbsent ?? false,
                    PlayerBAbsent = updatedMatchup?.PlayerBAbsent ?? false,
                    PlayerAAbsentWithNotice = updatedMatchup?.PlayerAAbsentWithNotice ?? false,
                    PlayerBAbsentWithNotice = updatedMatchup?.PlayerBAbsentWithNotice ?? false
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ScorecardResponse
                {
                    MatchupId = request.MatchupId,
                    Success = false,
                    Message = $"Error saving scorecard: {ex.Message}"
                };
            }
        }

        public async Task<List<HoleScore>> GetScorecardAsync(Guid matchupId)
        {
            return await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();
        }

        public async Task<ScorecardResponse> GetCompleteScorecardAsync(Guid matchupId)
        {
            var matchup = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == matchupId);
                
            if (matchup == null)
            {
                return new ScorecardResponse
                {
                    MatchupId = matchupId,
                    Success = false,
                    Message = "Matchup not found"
                };
            }

            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            // If no hole scores exist, initialize them with default values
            if (!holeScores.Any())
            {
                holeScores = await InitializeDefaultHoleScoresAsync(matchupId);
            }

            return new ScorecardResponse
            {
                MatchupId = matchupId,
                Success = true,
                Message = "Scorecard retrieved successfully",
                HoleScores = holeScores,
                // Include match play results
                PlayerAMatchPoints = matchup.PlayerAPoints,
                PlayerBMatchPoints = matchup.PlayerBPoints,
                PlayerAHolePoints = matchup.PlayerAHolePoints,
                PlayerBHolePoints = matchup.PlayerBHolePoints,
                PlayerAMatchWin = matchup.PlayerAMatchWin,
                PlayerBMatchWin = matchup.PlayerBMatchWin,
                // Include player handicaps
                PlayerAHandicap = matchup.PlayerA?.CurrentHandicap ?? 0,
                PlayerBHandicap = matchup.PlayerB?.CurrentHandicap ?? 0,
                // Include absence status
                PlayerAAbsent = matchup.PlayerAAbsent,
                PlayerBAbsent = matchup.PlayerBAbsent,
                PlayerAAbsentWithNotice = matchup.PlayerAAbsentWithNotice,
                PlayerBAbsentWithNotice = matchup.PlayerBAbsentWithNotice
            };
        }

        public async Task<bool> DeleteScorecardAsync(Guid matchupId)
        {
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .ToListAsync();

            if (!holeScores.Any())
                return false;

            _context.HoleScores.RemoveRange(holeScores);
            
            // Reset matchup scores
            var matchup = await _context.Matchups.FindAsync(matchupId);
            if (matchup != null)
            {
                matchup.PlayerAScore = null;
                matchup.PlayerBScore = null;
                matchup.PlayerAPoints = null;
                matchup.PlayerBPoints = null;
                _context.Matchups.Update(matchup);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Initialize hole scores with default hole handicaps and pars for a matchup
        /// </summary>
        private async Task<List<HoleScore>> InitializeDefaultHoleScoresAsync(Guid matchupId)
        {
            // Standard 9-hole pars
            var defaultPars = new Dictionary<int, int>
            {
                { 1, 4 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 4 },
                { 6, 3 }, { 7, 4 }, { 8, 4 }, { 9, 5 }
            };

            var holeScores = new List<HoleScore>();

            for (int holeNumber = 1; holeNumber <= 9; holeNumber++)
            {
                // Get the correct handicap for this hole from the database
                var holeHandicap = await _matchPlayScoringService.GetHoleHandicapAsync(holeNumber);
                
                var holeScore = new HoleScore
                {
                    Id = Guid.NewGuid(),
                    MatchupId = matchupId,
                    HoleNumber = holeNumber,
                    Par = defaultPars[holeNumber],
                    HoleHandicap = holeHandicap,
                    PlayerAMatchPoints = 0,
                    PlayerBMatchPoints = 0
                };

                holeScores.Add(holeScore);
                _context.HoleScores.Add(holeScore);
            }

            await _context.SaveChangesAsync();
            return holeScores;
        }
    }
}
