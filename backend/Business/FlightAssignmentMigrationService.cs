using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Business
{
    public class FlightAssignmentMigrationService
    {
        private readonly AppDbContext _context;

        public FlightAssignmentMigrationService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Migrates existing flight assignments to be session-based.
        /// This should be run once after the database migration to populate the new fields.
        /// </summary>
        public async Task<int> MigrateExistingFlightAssignmentsAsync()
        {
            var migratedCount = 0;

            // Get all existing flight assignments that don't have session information
            var assignmentsToMigrate = await _context.PlayerFlightAssignments
                .Include(a => a.Flight)
                .Where(a => a.SeasonId == Guid.Empty || a.SessionStartWeekNumber == 0)
                .ToListAsync();

            foreach (var assignment in assignmentsToMigrate)
            {
                if (assignment.Flight?.SeasonId == null)
                    continue;

                // Set the season ID from the flight
                assignment.SeasonId = assignment.Flight.SeasonId.Value;

                // For existing assignments, assume they belong to the first session (week 1)
                assignment.SessionStartWeekNumber = 1;

                // Set assignment date to now if not set
                if (assignment.AssignmentDate == default)
                {
                    assignment.AssignmentDate = DateTime.UtcNow;
                }

                migratedCount++;
            }

            await _context.SaveChangesAsync();
            return migratedCount;
        }

        /// <summary>
        /// Creates flight assignments for all players for a new session.
        /// This copies their current flight assignments to the new session.
        /// </summary>
        public async Task<int> CopyFlightAssignmentsToNewSessionAsync(Guid seasonId, int newSessionStartWeekNumber)
        {
            var copiedCount = 0;

            // Find the previous session start week
            var previousSessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.SessionStart &&
                           w.WeekNumber < newSessionStartWeekNumber)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();

            var previousSessionStartWeekNumber = previousSessionStartWeek?.WeekNumber ?? 1;

            // Get all assignments from the previous session
            var previousAssignments = await _context.PlayerFlightAssignments
                .Where(a => a.SeasonId == seasonId &&
                           a.SessionStartWeekNumber == previousSessionStartWeekNumber)
                .ToListAsync();

            // Copy each assignment to the new session
            foreach (var prevAssignment in previousAssignments)
            {
                // Check if assignment already exists for this session
                var existingAssignment = await _context.PlayerFlightAssignments
                    .FirstOrDefaultAsync(a => a.PlayerId == prevAssignment.PlayerId &&
                                            a.SeasonId == seasonId &&
                                            a.SessionStartWeekNumber == newSessionStartWeekNumber);

                if (existingAssignment == null)
                {
                    var newAssignment = new PlayerFlightAssignment
                    {
                        Id = Guid.NewGuid(),
                        PlayerId = prevAssignment.PlayerId,
                        FlightId = prevAssignment.FlightId,
                        SeasonId = seasonId,
                        SessionStartWeekNumber = newSessionStartWeekNumber,
                        IsFlightLeader = prevAssignment.IsFlightLeader,
                        HandicapAtAssignment = prevAssignment.HandicapAtAssignment,
                        AssignmentDate = DateTime.UtcNow
                    };

                    _context.PlayerFlightAssignments.Add(newAssignment);
                    copiedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return copiedCount;
        }

        /// <summary>
        /// Gets the current session start week number for a given week in a season.
        /// </summary>
        public async Task<int> GetSessionStartWeekNumberForWeekAsync(Guid seasonId, int weekNumber)
        {
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.WeekNumber <= weekNumber &&
                           w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();

            return sessionStartWeek?.WeekNumber ?? 1;
        }
    }
}
