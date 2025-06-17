using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class ScorecardService
    {
        private readonly AppDbContext _context;
        private readonly MatchupService _matchupService;
        private readonly ScoreEntryService _scoreEntryService;

        public ScorecardService(AppDbContext context, MatchupService matchupService, ScoreEntryService scoreEntryService)
        {
            _context = context;
            _matchupService = matchupService;
            _scoreEntryService = scoreEntryService;
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

                // Create new hole scores
                var holeScores = new List<HoleScore>();
                foreach (var holeScoreDto in request.HoleScores)
                {
                    var holeScore = new HoleScore
                    {
                        Id = Guid.NewGuid(),
                        MatchupId = request.MatchupId,
                        HoleNumber = holeScoreDto.HoleNumber,
                        Par = holeScoreDto.Par,
                        PlayerAScore = holeScoreDto.PlayerAScore,
                        PlayerBScore = holeScoreDto.PlayerBScore
                    };
                    
                    holeScores.Add(holeScore);
                }

                await _context.HoleScores.AddRangeAsync(holeScores);

                // Update the matchup with total scores
                matchup.PlayerAScore = request.PlayerATotalScore > 0 ? request.PlayerATotalScore : null;
                matchup.PlayerBScore = request.PlayerBTotalScore > 0 ? request.PlayerBTotalScore : null;

                // Calculate points based on total scores (if both players have scores)
                if (matchup.PlayerAScore.HasValue && matchup.PlayerBScore.HasValue)
                {
                    if (matchup.PlayerAScore < matchup.PlayerBScore)
                    {
                        matchup.PlayerAPoints = 2;
                        matchup.PlayerBPoints = 0;
                    }
                    else if (matchup.PlayerBScore < matchup.PlayerAScore)
                    {
                        matchup.PlayerAPoints = 0;
                        matchup.PlayerBPoints = 2;
                    }
                    else
                    {
                        // Tie
                        matchup.PlayerAPoints = 1;
                        matchup.PlayerBPoints = 1;
                    }
                }

                _context.Matchups.Update(matchup);

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
                await transaction.CommitAsync();

                return new ScorecardResponse
                {
                    MatchupId = request.MatchupId,
                    Success = true,
                    Message = "Scorecard saved successfully",
                    HoleScores = holeScores
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
    }
}
