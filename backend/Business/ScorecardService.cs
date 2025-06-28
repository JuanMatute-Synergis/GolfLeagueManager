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
        private readonly HandicapService _handicapService;

        public ScorecardService(AppDbContext context, MatchupService matchupService, ScoreEntryService scoreEntryService, MatchPlayService matchPlayService, MatchPlayScoringService matchPlayScoringService, AverageScoreService averageScoreService, HandicapService handicapService)
        {
            _context = context;
            _matchupService = matchupService;
            _scoreEntryService = scoreEntryService;
            _matchPlayService = matchPlayService;
            _matchPlayScoringService = matchPlayScoringService;
            _averageScoreService = averageScoreService;
            _handicapService = handicapService;
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
                matchup.PlayerAAbsent = request.PlayerAAbsent;
                matchup.PlayerBAbsent = request.PlayerBAbsent;
                matchup.PlayerAAbsentWithNotice = request.PlayerAAbsentWithNotice;
                matchup.PlayerBAbsentWithNotice = request.PlayerBAbsentWithNotice;

                // If a player is absent, set their score to their average up to this week
                if (matchup.PlayerAAbsent && matchup.Week != null)
                {
                    var avgA = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        matchup.PlayerAId, matchup.Week.SeasonId, matchup.Week.WeekNumber);
                    matchup.PlayerAScore = (int)Math.Round(avgA);
                }
                else
                {
                    matchup.PlayerAScore = request.PlayerATotalScore > 0 ? request.PlayerATotalScore : null;
                }
                if (matchup.PlayerBAbsent && matchup.Week != null)
                {
                    var avgB = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        matchup.PlayerBId, matchup.Week.SeasonId, matchup.Week.WeekNumber);
                    matchup.PlayerBScore = (int)Math.Round(avgB);
                }
                else
                {
                    matchup.PlayerBScore = request.PlayerBTotalScore > 0 ? request.PlayerBTotalScore : null;
                }

                // Special circumstance points logic
                if (matchup.Week != null && matchup.Week.SpecialPointsAwarded.HasValue)
                {
                    int special = matchup.Week.SpecialPointsAwarded.Value;
                    matchup.PlayerAPoints = matchup.PlayerAAbsent ? special / 2 : special;
                    matchup.PlayerBPoints = matchup.PlayerBAbsent ? special / 2 : special;
                }

                // Save changes to get hole scores in database
                await _context.SaveChangesAsync();

                // Calculate match play results using the new scoring system
                await _matchPlayService.CalculateMatchPlayResultsAsync(request.MatchupId);

                // Create or update score entries for both players
                if (matchup.Week != null)
                {
                    // Player A score entry
                    if (matchup.PlayerAScore.HasValue || matchup.PlayerAAbsent)
                    {
                        var scoreEntryA = new ScoreEntry
                        {
                            Id = Guid.NewGuid(),
                            PlayerId = matchup.PlayerAId,
                            WeekId = matchup.WeekId,
                            Score = matchup.PlayerAScore, // Can be null for absent players
                            PointsEarned = matchup.PlayerAPoints ?? 0
                        };
                        await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntryA);
                    }

                    // Player B score entry
                    if (matchup.PlayerBScore.HasValue || matchup.PlayerBAbsent)
                    {
                        var scoreEntryB = new ScoreEntry
                        {
                            Id = Guid.NewGuid(),
                            PlayerId = matchup.PlayerBId,
                            WeekId = matchup.WeekId,
                            Score = matchup.PlayerBScore, // Can be null for absent players
                            PointsEarned = matchup.PlayerBPoints ?? 0
                        };
                        await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntryB);
                    }
                }

                await _context.SaveChangesAsync();
                
                // Update average scores for both players after scores are saved
                // Use WeekNumber + 1 to ensure the current week is included in the average calculation
                if (matchup.Week != null && matchup.Week.CountsForScoring)
                {
                    if (matchup.PlayerAScore.HasValue)
                    {
                        await _averageScoreService.UpdatePlayerAverageScoreAsync(matchup.PlayerAId, matchup.Week.SeasonId, matchup.Week.WeekNumber + 1);
                    }
                    if (matchup.PlayerBScore.HasValue)
                    {
                        await _averageScoreService.UpdatePlayerAverageScoreAsync(matchup.PlayerBId, matchup.Week.SeasonId, matchup.Week.WeekNumber + 1);
                    }
                }
                
                await transaction.CommitAsync();

                // Reload the matchup to get the calculated match play results
                var updatedMatchup = await _context.Matchups.FindAsync(request.MatchupId);

                // Calculate per-week averages for both players (for this week)
                decimal? playerAWeekAverage = null;
                decimal? playerBWeekAverage = null;
                if (matchup.Week != null)
                {
                    playerAWeekAverage = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        matchup.PlayerAId, matchup.Week.SeasonId, matchup.Week.WeekNumber);
                    playerBWeekAverage = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        matchup.PlayerBId, matchup.Week.SeasonId, matchup.Week.WeekNumber);
                }

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
                    PlayerBAbsentWithNotice = updatedMatchup?.PlayerBAbsentWithNotice ?? false,
                    // New: per-week averages
                    PlayerAWeekAverage = playerAWeekAverage,
                    PlayerBWeekAverage = playerBWeekAverage
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
                .Include(m => m.Week)
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

            // Get session handicaps for both players
            decimal playerAHandicap = 0;
            decimal playerBHandicap = 0;
            
            if (matchup.PlayerA != null && matchup.Week != null)
            {
                playerAHandicap = await _handicapService.GetPlayerSessionHandicapAsync(
                    matchup.PlayerA.Id, matchup.Week.SeasonId, matchup.Week.WeekNumber);
            }
            
            if (matchup.PlayerB != null && matchup.Week != null)
            {
                playerBHandicap = await _handicapService.GetPlayerSessionHandicapAsync(
                    matchup.PlayerB.Id, matchup.Week.SeasonId, matchup.Week.WeekNumber);
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
                // Include session handicaps instead of current handicaps
                PlayerAHandicap = playerAHandicap,
                PlayerBHandicap = playerBHandicap,
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
            // Get the matchup and week to determine which 9 holes to initialize
            var matchup = await _context.Matchups
                .Include(m => m.Week)
                .FirstOrDefaultAsync(m => m.Id == matchupId);
                
            if (matchup?.Week == null)
            {
                throw new ArgumentException("Matchup or Week not found");
            }

            // Determine which holes to initialize based on the week's NineHoles setting
            var (startHole, endHole) = matchup.Week.NineHoles == NineHoles.Front ? (1, 9) : (10, 18);

            // Standard pars for all 18 holes
            var defaultPars = new Dictionary<int, int>
            {
                { 1, 4 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 4 },
                { 6, 3 }, { 7, 4 }, { 8, 4 }, { 9, 5 },
                { 10, 4 }, { 11, 3 }, { 12, 4 }, { 13, 5 }, { 14, 4 },
                { 15, 3 }, { 16, 4 }, { 17, 4 }, { 18, 5 }
            };

            var holeScores = new List<HoleScore>();

            for (int holeNumber = startHole; holeNumber <= endHole; holeNumber++)
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

        public async Task<List<Matchup>> GetAllMatchupsForSeasonAsync(Guid seasonId)
        {
            // Join Matchup with Week to filter by SeasonId
            return await _context.Matchups
                .Join(_context.Weeks,
                      m => m.WeekId,
                      w => w.Id,
                      (m, w) => new { Matchup = m, Week = w })
                .Where(x => x.Week.SeasonId == seasonId)
                .Select(x => x.Matchup)
                .ToListAsync();
        }

        public async Task<List<HoleScore>> GetAllHoleScoresForSeasonAsync(Guid seasonId)
        {
            // Get all matchup IDs for the season
            var matchupIds = await _context.Matchups
                .Join(_context.Weeks,
                      m => m.WeekId,
                      w => w.Id,
                      (m, w) => new { Matchup = m, Week = w })
                .Where(x => x.Week.SeasonId == seasonId)
                .Select(x => x.Matchup.Id)
                .ToListAsync();
            return await _context.HoleScores.Where(hs => matchupIds.Contains(hs.MatchupId)).ToListAsync();
        }
    }
}
