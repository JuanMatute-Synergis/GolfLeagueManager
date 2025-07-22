using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GolfLeagueManager.Business
{
    public class PdfScorecardService
    {
        private readonly AppDbContext _context;
        private readonly MatchupService _matchupService;
        private readonly PlayerFlightAssignmentService _flightAssignmentService;
        private readonly MatchPlayService _matchPlayService;
        private readonly AverageScoreService _averageScoreService;
        private readonly HandicapService _handicapService;
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;
        private readonly LeagueSettingsService _leagueSettingsService;

        public PdfScorecardService(AppDbContext context, MatchupService matchupService, PlayerFlightAssignmentService flightAssignmentService, MatchPlayService matchPlayService, AverageScoreService averageScoreService, HandicapService handicapService, PlayerSeasonStatsService playerSeasonStatsService, LeagueSettingsService leagueSettingsService)
        {
            _context = context;
            _matchupService = matchupService;
            _flightAssignmentService = flightAssignmentService;
            _matchPlayService = matchPlayService;
            _averageScoreService = averageScoreService;
            _handicapService = handicapService;
            _playerSeasonStatsService = playerSeasonStatsService;
            _leagueSettingsService = leagueSettingsService;
        }

        /// <summary>
        /// Gets the league name from settings, with fallback to default
        /// </summary>
        private async Task<string> GetLeagueNameAsync(Guid seasonId)
        {
            try
            {
                var settings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);
                return !string.IsNullOrWhiteSpace(settings.LeagueName) ? settings.LeagueName : "Golf League";
            }
            catch
            {
                return "Golf League"; // Fallback if settings not found
            }
        }

        /// <summary>
        /// Generates a compact PDF of all scorecards for a specific week, grouped by flight, 4 per page, 1 flight per page.
        /// </summary>
        public async Task<byte[]> GenerateWeekScorecardPdfAsync(Guid weekId)
        {
            // Configure QuestPDF license (free for commercial use)
            QuestPDF.Settings.License = LicenseType.Community;

            var matchups = (await _matchupService.GetMatchupsByWeekIdAsync(weekId)).ToList();
            if (!matchups.Any()) throw new ArgumentException("No matchups found for this week.");

            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == weekId);
            if (week == null) throw new ArgumentException("Week not found.");
            var seasonId = week.SeasonId;

            // Get league name from settings
            var leagueName = await GetLeagueNameAsync(seasonId);

            // Calculate session number
            var sessionNumber = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= week.WeekNumber && w.SessionStart)
                .Count();
            if (sessionNumber == 0) sessionNumber = 1;

            var assignments = _flightAssignmentService.GetAllAssignments().ToList();
            var flights = await _context.Flights.ToListAsync();
            var course = await _context.Courses.Include(c => c.CourseHoles).FirstOrDefaultAsync();
            if (course == null) throw new ArgumentException("No course found.");

            // Pre-fetch session handicaps for all players in the matchups
            var allPlayerIds = matchups.SelectMany(m => new[] { m.PlayerAId, m.PlayerBId }).Distinct().ToList();
            var handicapData = new Dictionary<Guid, decimal>();

            // Get league settings for this season to use proper handicap calculation
            var leagueSettings = await _context.LeagueSettings
                .FirstOrDefaultAsync(ls => ls.SeasonId == seasonId);

            foreach (var playerId in allPlayerIds)
            {
                try
                {
                    decimal handicap;
                    if (leagueSettings != null)
                    {
                        // Use the proper method that respects league settings and week number
                        handicap = await _handicapService.GetPlayerHandicapUpToWeekAsync(playerId, seasonId, week.WeekNumber);
                    }
                    else
                    {
                        // Fallback to hardcoded WHS values if no league settings found
                        handicap = await _handicapService.CalculatePlayerHandicapAsync(playerId, 35, 113, 20);
                    }
                    handicapData[playerId] = handicap;
                }
                catch
                {
                    // Fallback to 0 if handicap calculation fails
                    handicapData[playerId] = 0;
                }
            }

            // Filter holes based on the week's NineHoles setting
            var allHoles = course.CourseHoles.OrderBy(h => h.HoleNumber).ToList();
            var holes = week.NineHoles == NineHoles.Front
                ? allHoles.Where(h => h.HoleNumber >= 1 && h.HoleNumber <= 9).ToList()
                : allHoles.Where(h => h.HoleNumber >= 10 && h.HoleNumber <= 18).ToList();

            // Sort flights by name (assuming names are numbers or can be sorted as 1-4)
            var orderedFlights = flights.OrderBy(f => f.Name).ToList();
            // Group matchups by flight (using PlayerA's assignment for the season)
            var matchupsByFlight = orderedFlights
                .Select(flight => new
                {
                    Flight = flight,
                    Matchups = matchups.Where(m =>
                    {
                        var assignment = assignments.FirstOrDefault(a => a.PlayerId == m.PlayerAId && a.Flight != null && a.Flight.SeasonId == seasonId);
                        return assignment?.Flight?.Id == flight.Id;
                    }).ToList()
                })
                .Where(g => g.Matchups.Any())
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(10);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontColor(Colors.Black));
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Column(content =>
                    {
                        bool isFirstPage = true;

                        foreach (var flightGroup in matchupsByFlight)
                        {
                            var flight = flightGroup.Flight;
                            var flightName = $"Flight {flight.Name}";
                            var matchupsInFlight = flightGroup.Matchups;
                            if (!matchupsInFlight.Any()) continue;

                            if (!isFirstPage)
                            {
                                content.Item().PageBreak();
                            }
                            isFirstPage = false;

                            var weekTitle = $"Week {week.WeekNumber} (" + week.Date.ToString("dddd MMMM d yyyy") + ")";

                            // Header
                            content.Item().Text($"{weekTitle} - {flightName} - {leagueName} - Session {sessionNumber}")
                                .FontSize(10).FontColor(Colors.Black).Bold().AlignCenter();

                            // 4 cards per page, each on its own row
                            for (int i = 0; i < matchupsInFlight.Count; i++)
                            {
                                var matchup = matchupsInFlight[i];
                                content.Item().PaddingVertical(5).Element(container =>
                                {
                                    CreateCompactScorecardTable(container, matchup, holes, handicapData);
                                });

                                // Add page break after every 4 cards, but only if more cards remain
                                if ((i + 1) % 4 == 0 && (i + 1) < matchupsInFlight.Count)
                                {
                                    content.Item().PageBreak();
                                    content.Item().Text($"{weekTitle} - {flightName} - {leagueName} - Session {sessionNumber}")
                                        .FontSize(10).Bold().AlignCenter();
                                }
                            }
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        /// <summary>
        /// Generates a summary PDF report for a specific week, showing only the summary tables for all flights.
        /// </summary>
        public async Task<byte[]> GenerateWeekSummaryPdfAsync(Guid weekId)
        {
            // Configure QuestPDF license (free for commercial use)
            QuestPDF.Settings.License = LicenseType.Community;

            var matchups = (await _matchupService.GetMatchupsByWeekIdAsync(weekId)).ToList();
            if (!matchups.Any()) throw new ArgumentException("No matchups found for this week.");

            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == weekId);
            if (week == null) throw new ArgumentException("Week not found.");
            var seasonId = week.SeasonId;

            // Get league name from settings
            var leagueName = await GetLeagueNameAsync(seasonId);

            var assignments = _flightAssignmentService.GetAllAssignments().ToList();
            var flights = await _context.Flights.ToListAsync();

            // Sort flights by name (assuming names are numbers or can be sorted as 1-4)
            var orderedFlights = flights.OrderBy(f => f.Name).ToList();
            // Group matchups by flight (using PlayerA's assignment for the season)
            var matchupsByFlight = orderedFlights
                .Select(flight => new
                {
                    Flight = flight,
                    Matchups = matchups.Where(m =>
                    {
                        var assignment = assignments.FirstOrDefault(a => a.PlayerId == m.PlayerAId && a.Flight != null && a.Flight.SeasonId == seasonId);
                        return assignment?.Flight?.Id == flight.Id;
                    }).ToList()
                })
                .Where(g => g.Matchups.Any())
                .ToList();

            var weekTitle = $"Week {week.WeekNumber} (" + week.Date.ToString("dddd MMMM d yyyy") + ")";

            // Pre-calculate handicaps for all players in all matchups (same method as UI)
            var allPlayerIds = matchups.SelectMany(m => new[] { m.PlayerAId, m.PlayerBId }).Distinct().ToList();
            var handicapData = new Dictionary<Guid, decimal>();
            foreach (var playerId in allPlayerIds)
            {
                try
                {
                    var handicap = await _handicapService.GetPlayerHandicapUpToWeekAsync(playerId, seasonId, week.WeekNumber);
                    handicapData[playerId] = handicap;
                }
                catch
                {
                    handicapData[playerId] = 0; // Fallback on error
                }
            }

            // Calculate session number
            var sessionNumber = _context.Weeks
                .Where(w => w.SeasonId == week.SeasonId && w.WeekNumber <= week.WeekNumber && w.SessionStart)
                .Count();
            if (sessionNumber == 0) sessionNumber = 1;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(15);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontColor(Colors.Black));
                    page.DefaultTextStyle(x => x.FontSize(8));

                    page.Content().Column(content =>
                    {
                        // Modern styled header
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => columns.RelativeColumn());
                            table.Cell().Padding(15).Background(Colors.White)
                                .Text($"{weekTitle}\n{leagueName} - Session {sessionNumber} Weekly Summary")
                                .FontSize(16).Bold().FontColor(Colors.Black).AlignCenter();
                        });

                        content.Item().PaddingTop(10);

                        // Create single page layout with all flights
                        if (matchupsByFlight.Count <= 4)
                        {
                            // First row: Flights 1 & 2
                            var firstRowFlights = matchupsByFlight.Take(2).ToList();
                            if (firstRowFlights.Any())
                            {
                                content.Item().Row(row =>
                                {
                                    row.RelativeItem().Padding(3).Element(container =>
                                    {
                                        var firstFlight = firstRowFlights.ElementAtOrDefault(0);
                                        if (firstFlight != null)
                                        {
                                            CreateCompactFlightSummaryTable(container, firstFlight, week, seasonId, handicapData);
                                        }
                                    });

                                    if (firstRowFlights.Count >= 2)
                                    {
                                        row.RelativeItem().Padding(3).Element(container =>
                                        {
                                            CreateCompactFlightSummaryTable(container, firstRowFlights[1], week, seasonId, handicapData);
                                        });
                                    }
                                });
                            }

                            // Second row: Flights 3 & 4 (if they exist)
                            var secondRowFlights = matchupsByFlight.Skip(2).Take(2).ToList();
                            if (secondRowFlights.Any())
                            {
                                content.Item().PaddingTop(5).Row(row =>
                                {
                                    row.RelativeItem().Padding(3).Element(container =>
                                    {
                                        var firstFlight = secondRowFlights.ElementAtOrDefault(0);
                                        if (firstFlight != null)
                                        {
                                            CreateCompactFlightSummaryTable(container, firstFlight, week, seasonId, handicapData);
                                        }
                                    });

                                    if (secondRowFlights.Count >= 2)
                                    {
                                        row.RelativeItem().Padding(3).Element(container =>
                                        {
                                            CreateCompactFlightSummaryTable(container, secondRowFlights[1], week, seasonId, handicapData);
                                        });
                                    }
                                });
                            }
                        }
                        else
                        {
                            // Fallback to original layout if more than 4 flights
                            bool isFirstFlight = true;
                            foreach (var flightGroup in matchupsByFlight)
                            {
                                if (!isFirstFlight)
                                {
                                    content.Item().PageBreak();
                                }
                                isFirstFlight = false;

                                content.Item().Element(container =>
                                {
                                    CreateCompactFlightSummaryTable(container, flightGroup, week, seasonId, handicapData);
                                });
                            }
                        }

                        // Page 2: Detailed Match Play Points Breakdown by Week
                        content.Item().PageBreak();

                        // Header for page 2
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => columns.RelativeColumn());
                            table.Cell().Padding(15).Background(Colors.White)
                                .Text($"{weekTitle}\nMatch Play Points by Week - Session {sessionNumber} Breakdown")
                                .FontSize(16).Bold().FontColor(Colors.Black).AlignCenter();
                        });

                        content.Item().PaddingTop(3);

                        // Create detailed weekly breakdown for each flight
                        foreach (var flightGroup in matchupsByFlight)
                        {
                            content.Item().Element(container =>
                            {
                                CreateWeeklyPointsBreakdownTable(container, flightGroup, week, seasonId);
                            });
                            content.Item().PaddingTop(5);
                        }

                        // Page 3: Remaining Schedule
                        content.Item().PageBreak();

                        // Header for page 3
                        content.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns => columns.RelativeColumn());
                            table.Cell().Padding(15).Background(Colors.White)
                                .Text($"{weekTitle}\nSession {sessionNumber} - Remaining Schedule")
                                .FontSize(16).Bold().FontColor(Colors.Black).AlignCenter();
                        });

                        content.Item().PaddingTop(10);

                        // Create remaining schedule table
                        content.Item().Element(container =>
                        {
                            CreateRemainingScheduleTable(container, week, seasonId, orderedFlights, assignments);
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void CreateCompactScorecardTable(IContainer container, Matchup matchup, List<CourseHole> holes, Dictionary<Guid, decimal> handicapData)
        {
            // Get player info
            var playerA = matchup.PlayerA;
            var playerB = matchup.PlayerB;
            string playerAName = playerA != null ? $"{playerA.FirstName} {playerA.LastName}" : "Player A";
            string playerBName = playerB != null ? $"{playerB.FirstName} {playerB.LastName}" : "Player B";

            // Get session-specific handicaps from pre-fetched data
            var playerAHandicap = playerA != null && handicapData.ContainsKey(playerA.Id)
                ? handicapData[playerA.Id]
                : 0;
            var playerBHandicap = playerB != null && handicapData.ContainsKey(playerB.Id)
                ? handicapData[playerB.Id]
                : 0;

            // Get hole scores - use the data that's already calculated and stored
            var holeScores = _context.HoleScores.Where(hs => hs.MatchupId == matchup.Id).OrderBy(hs => hs.HoleNumber).ToList();
            var holeScoresOrdered = holeScores.OrderBy(hs => hs.HoleNumber).ToList();

            // Calculate stroke allocation for visual highlighting only
            var holesInPlay = holeScoresOrdered.Select(hs => new { hs.HoleNumber, hs.HoleHandicap }).ToList();
            int playerAHandicapInt = (int)Math.Round((double)playerAHandicap);
            int playerBHandicapInt = (int)Math.Round((double)playerBHandicap);
            bool playerAReceivesStrokes = playerAHandicapInt > playerBHandicapInt;
            bool playerBReceivesStrokes = playerBHandicapInt > playerAHandicapInt;
            int handicapDiff = Math.Abs(playerAHandicapInt - playerBHandicapInt);
            var hardestHoles = holesInPlay.OrderBy(h => h.HoleHandicap).Take(handicapDiff).Select(h => h.HoleNumber).ToHashSet();

            // Determine winner
            string winner = "Tie";
            if (matchup.PlayerAMatchWin) winner = playerAName;
            else if (matchup.PlayerBMatchWin) winner = playerBName;

            string winnerText = winner == "Tie" ? "Tie" : $"Winner: {winner}";

            // Modern card-like container with border, background - Dark mode
            container.Padding(4)
                .Background(Colors.White)
                .Border(1).BorderColor(Colors.Grey.Lighten1)
                .Column(cardCol =>
                {
                    // Card Title/Header
                    cardCol.Item().PaddingBottom(6).Text(winnerText)
                        .FontSize(12).Bold().FontColor(Colors.Blue.Darken1).AlignCenter();
                    cardCol.Item().Text($"{playerAName} vs {playerBName}")
                        .FontSize(9).Bold().FontColor(Colors.Grey.Darken1).AlignCenter();
                    cardCol.Item().Text($"Handicaps: {playerAHandicap:0.#} - {playerBHandicap:0.#}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1).AlignCenter();
                    cardCol.Item().PaddingTop(6).Element(cardTable =>
                    {
                        cardTable.Table(table =>
                        {
                            // Define columns: Label + holes + total
                            var columnWidths = new float[holes.Count + 2];
                            columnWidths[0] = 1.2f; // Label column
                            for (int i = 1; i <= holes.Count; i++) columnWidths[i] = 1f; // Hole columns
                            columnWidths[holes.Count + 1] = 1.2f; // Total column

                            table.ColumnsDefinition(columns =>
                            {
                                for (int i = 0; i < columnWidths.Length; i++)
                                {
                                    columns.RelativeColumn(columnWidths[i]);
                                }
                            });

                            // Hole numbers row
                            // Center vertically and horizontally
                            // Label cell
                            table.Cell().Background(Colors.White).Padding(4).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text("HOLE").FontSize(10).Bold().FontColor(Colors.Black));
                            foreach (var hole in holes)
                                table.Cell().Background(Colors.White).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                    .Element(cell => cell.AlignCenter().AlignMiddle().Text(hole.HoleNumber.ToString()).FontSize(10).Bold().FontColor(Colors.Black));
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text("TOTAL").FontSize(8).Bold().FontColor(Colors.Black));

                            // Par row
                            table.Cell().Background(Colors.White).Padding(4).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text("PAR").FontSize(10).Bold().FontColor(Colors.Black));
                            int parTotal = 0;
                            foreach (var hole in holes)
                            {
                                parTotal += hole.Par;
                                table.Cell().Background(Colors.White).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                    .Element(cell => cell.AlignCenter().AlignMiddle().Text(hole.Par.ToString()).FontSize(10).Bold().FontColor(Colors.Black));
                            }
                            table.Cell().Background(Colors.White).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text(parTotal.ToString()).FontSize(11).Bold().FontColor(Colors.Black));

                            // Player A row
                            table.Cell().Background(Colors.Blue.Lighten3).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1).Element(cell => cell.AlignCenter().AlignMiddle().Text(playerAName.Split(' ')[0]).FontSize(12).Bold().FontColor(Colors.Black));
                            int playerATotal = 0;
                            foreach (var hole in holes)
                            {
                                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                                int playerAGross = hs?.PlayerAScore ?? 0;
                                if (playerAGross > 0) playerATotal += playerAGross;

                                // Determine if player A gets a stroke on this hole (for yellow background)
                                bool playerAReceivesStrokeOnHole = playerAReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber);
                                var backgroundColor = playerAReceivesStrokeOnHole ? Colors.Yellow.Lighten3 : Colors.White;

                                // Use stored match play points to determine hole winner (no recalculation needed)
                                int playerAHolePoints = hs?.PlayerAMatchPoints ?? 0;
                                int playerBHolePoints = hs?.PlayerBMatchPoints ?? 0;
                                bool aWins = playerAHolePoints > playerBHolePoints;
                                bool isTie = playerAHolePoints == playerBHolePoints && playerAHolePoints > 0;

                                table.Cell().Background(backgroundColor).Padding(0).Height(28).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                    .Layers(layers =>
                                    {
                                        layers.PrimaryLayer().AlignCenter().AlignMiddle()
                                            .Text(playerAGross > 0 ? playerAGross.ToString() : "").FontSize(13).Bold().FontColor(Colors.Black);
                                        if (aWins || isTie)
                                        {
                                            string color = aWins ? "#22c55e" : "#eab308"; // Use hex colors for SVG
                                            layers.Layer().AlignRight().AlignTop().Padding(1).Svg($"<svg width='20' height='20'><circle cx='6' cy='6' r='4' fill='{color}' /></svg>");
                                        }
                                        // Add net score at bottom if player receives stroke on this hole
                                        if (playerAReceivesStrokeOnHole && playerAGross > 0)
                                        {
                                            int netScore = playerAGross - 1;
                                            layers.Layer().AlignCenter().AlignBottom().Padding(0)
                                                .Text(netScore.ToString()).FontSize(8).Bold().FontColor(Colors.Red.Darken2);
                                        }
                                    });
                            }
                            var storedPlayerAScore = matchup.PlayerAScore ?? playerATotal;
                            table.Cell().Background(Colors.Blue.Lighten2).Padding(0).Height(28).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle()
                                    .Text(matchup.PlayerAAbsent ? "ABS" : storedPlayerAScore.ToString())
                                    .FontSize(12).Bold().FontColor(Colors.Black));

                            // Player B row
                            table.Cell().Background(Colors.Orange.Lighten2).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text(playerBName.Split(' ')[0]).FontSize(12).Bold().FontColor(Colors.Black));
                            int playerBTotal = 0;
                            foreach (var hole in holes)
                            {
                                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                                int playerBGross = hs?.PlayerBScore ?? 0;
                                if (playerBGross > 0) playerBTotal += playerBGross;

                                // Determine if player B gets a stroke on this hole (for yellow background)
                                bool playerBReceivesStrokeOnHole = playerBReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber);
                                var backgroundColor = playerBReceivesStrokeOnHole ? Colors.Yellow.Lighten3 : Colors.White;

                                // Use stored match play points to determine hole winner (no recalculation needed)
                                int playerAHolePoints = hs?.PlayerAMatchPoints ?? 0;
                                int playerBHolePoints = hs?.PlayerBMatchPoints ?? 0;
                                bool bWins = playerBHolePoints > playerAHolePoints;
                                bool isTie = playerAHolePoints == playerBHolePoints && playerBHolePoints > 0;

                                table.Cell().Background(backgroundColor).Padding(0).Height(28).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                    .Layers(layers =>
                                    {
                                        layers.PrimaryLayer().AlignCenter().AlignMiddle()
                                            .Text(playerBGross > 0 ? playerBGross.ToString() : "").FontSize(13).Bold().FontColor(Colors.Black);
                                        if (bWins || isTie)
                                        {
                                            string color = bWins ? "#22c55e" : "#eab308"; // Use hex colors for SVG
                                            layers.Layer().AlignRight().AlignTop().Padding(1).Svg($"<svg width='12' height='12'><circle cx='6' cy='6' r='4' fill='{color}' /></svg>");
                                        }
                                        // Add net score at bottom if player receives stroke on this hole
                                        if (playerBReceivesStrokeOnHole && playerBGross > 0)
                                        {
                                            int netScore = playerBGross - 1;
                                            layers.Layer().AlignCenter().AlignBottom().Padding(0)
                                                .Text(netScore.ToString()).FontSize(8).Bold().FontColor(Colors.Red.Darken2);
                                        }
                                    });
                            }
                            var storedPlayerBScore = matchup.PlayerBScore ?? playerBTotal;
                            table.Cell().Background(Colors.Orange.Lighten3).Padding(0).Height(28).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle()
                                    .Text(matchup.PlayerBAbsent ? "ABS" : storedPlayerBScore.ToString())
                                    .FontSize(12).Bold().FontColor(Colors.Black));

                            // Matchplay Points row - Use stored calculated values instead of recalculating
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(6)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text("POINTS").FontSize(6).Bold().FontColor(Colors.Black));

                            // Display individual hole match play points using stored values
                            foreach (var hole in holes)
                            {
                                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                                string holeResult = "";
                                if (hs != null)
                                {
                                    int playerAPoints = hs.PlayerAMatchPoints;
                                    int playerBPoints = hs.PlayerBMatchPoints;
                                    if (playerAPoints > 0 || playerBPoints > 0)
                                    {
                                        holeResult = $"{playerAPoints}-{playerBPoints}";
                                    }
                                }
                                table.Cell().Background(Colors.White).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                    .Element(cell => cell.AlignCenter().AlignMiddle().Text(holeResult).FontSize(8).Bold().FontColor(Colors.Black));
                            }

                            // Use stored total matchplay points from the matchup (already calculated by MatchPlayService)
                            int playerATotalPoints = matchup.PlayerAPoints ?? 0;
                            int playerBTotalPoints = matchup.PlayerBPoints ?? 0;

                            // Total points cell (showing final match play points out of 20)
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(0).Border(0.7f).BorderColor(Colors.Grey.Darken1)
                                .Element(cell => cell.AlignCenter().AlignMiddle().Text($"{playerATotalPoints}-{playerBTotalPoints}").FontSize(10).Bold().FontColor(Colors.Black));
                        });
                    });

                    // Match result explanation at the bottom
                    cardCol.Item().PaddingTop(0).Element(explanationContainer =>
                    {
                        string explanationText = GetMatchResultExplanation(matchup, playerAName, playerBName);
                        explanationContainer.Text(explanationText)
                            .FontSize(8).FontColor(Colors.Grey.Darken1).AlignCenter().Italic();
                    });
                });
        }

        private void CreateCompactFlightSummaryTable(IContainer container, dynamic flightGroup, Week currentWeek, Guid seasonId, Dictionary<Guid, decimal> handicapData)
        {
            if (flightGroup == null) return;

            var flight = flightGroup.Flight;
            var matchupsInFlight = (List<Matchup>)flightGroup.Matchups;

            // Get all player IDs in this flight to fetch session handicaps
            var playerIds = matchupsInFlight
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToList();

            container.Column(column =>
            {
                // Flight header
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => columns.RelativeColumn());
                    table.Cell().Padding(6).Background(Colors.White)
                        .Text($"Flight {flight.Name}")
                        .FontSize(9).Bold().FontColor(Colors.Black).AlignCenter();
                });

                // Summary table
                column.Item().Element(container =>
                {
                    CreateFlightSummaryTable(container, flight, matchupsInFlight, currentWeek, seasonId, handicapData);
                });

                // Next week matchups
                column.Item().PaddingTop(4).Element(container =>
                {
                    CreateNextWeekMatchupsTable(container, flight, currentWeek, seasonId, handicapData);
                });
            });
        }

        private void CreateFlightSummaryTable(IContainer container, Flight flight, List<Matchup> matchupsInFlight, Week currentWeek, Guid seasonId, Dictionary<Guid, decimal> handicapData)
        {
            // Get all players in this flight for the season
            var playerIds = matchupsInFlight
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToList();
            var players = _context.Players.Where(p => playerIds.Contains(p.Id)).ToList();

            // Get session data
            var allMatchups = _context.Matchups
                .Where(m => (playerIds.Contains(m.PlayerAId) || playerIds.Contains(m.PlayerBId)))
                .ToList();
            var weekNumber = currentWeek.WeekNumber;

            var sessionStartWeek = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= weekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefault();
            int sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;

            var weekIdsInSession = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber >= sessionStartWeekNumber && w.WeekNumber <= weekNumber)
                .OrderBy(w => w.WeekNumber)
                .Select(w => w.Id)
                .ToList();

            // Calculate scores for sorting
            var playersWithScores = new List<(Player player, int accumScore)>();

            foreach (var player in players)
            {
                var matchPoints = allMatchups
                    .Where(m => weekIdsInSession.Contains(m.WeekId) && (m.PlayerAId == player.Id || m.PlayerBId == player.Id))
                    .Select(m =>
                    {
                        var week = _context.Weeks.FirstOrDefault(x => x.Id == m.WeekId);
                        bool absent = (m.PlayerAId == player.Id) ? m.PlayerAAbsent : m.PlayerBAbsent;
                        bool absentWithNotice = (m.PlayerAId == player.Id) ? m.PlayerAAbsentWithNotice : m.PlayerBAbsentWithNotice;
                        
                        if (week != null && week.SpecialPointsAwarded.HasValue)
                        {
                            int special = week.SpecialPointsAwarded.Value;
                            return absent ? (special / 2) : special;
                        }
                        
                        // Handle absent players with/without notice
                        if (absent)
                        {
                            return absentWithNotice ? 4 : 0;
                        }
                        
                        // Regular match points for players who played
                        return m.PlayerAId == player.Id ? (m.PlayerAPoints ?? 0) : (m.PlayerBPoints ?? 0);
                    })
                    .Sum();
                playersWithScores.Add((player, matchPoints));
            }

            var sortedPlayers = playersWithScores
                .OrderByDescending(p => p.accumScore)
                .ThenBy(p => p.player.LastName)
                .ToList();

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2.0f); // Player (reduced slightly)
                    columns.RelativeColumn(1.6f); // Phone (increased)
                    columns.RelativeColumn(0.8f); // HCP
                    columns.RelativeColumn(0.8f); // Avg
                    columns.RelativeColumn(0.8f); // Gross
                    columns.RelativeColumn(1f);   // This Week
                    columns.RelativeColumn(1.2f); // Session Total
                });

                // Header
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Player").FontSize(6).Bold().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Phone").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("HCP").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Avg").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Gross").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("This Week").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                table.Cell().Background(Colors.Grey.Lighten2).Padding(3).Text("Session Total").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);

                // Player rows
                for (int i = 0; i < sortedPlayers.Count; i++)
                {
                    var (player, accumScore) = sortedPlayers[i];
                    var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten2;

                    // Get handicap from calculated data or fallback to season record
                    var hcp = handicapData.ContainsKey(player.Id) ? handicapData[player.Id] :
                        _playerSeasonStatsService.GetInitialHandicapAsync(player.Id, seasonId).Result;
                    var avg = _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                        player.Id, seasonId, currentWeek.WeekNumber + 1).Result;

                    var matchup = matchupsInFlight.FirstOrDefault(m => m.PlayerAId == player.Id || m.PlayerBId == player.Id);
                    int gross = 0, thisWeekMpPoints = 0;
                    bool isAbsent = false;
                    bool isAbsentWithNotice = false;
                    if (matchup != null)
                    {
                        if (matchup.PlayerAId == player.Id)
                        {
                            gross = matchup.PlayerAScore ?? 0;
                            isAbsent = matchup.PlayerAAbsent;
                            isAbsentWithNotice = matchup.PlayerAAbsentWithNotice;
                            thisWeekMpPoints = matchup.PlayerAPoints ?? 0;
                        }
                        else if (matchup.PlayerBId == player.Id)
                        {
                            gross = matchup.PlayerBScore ?? 0;
                            isAbsent = matchup.PlayerBAbsent;
                            isAbsentWithNotice = matchup.PlayerBAbsentWithNotice;
                            thisWeekMpPoints = matchup.PlayerBPoints ?? 0;
                        }
                    }

                    if (currentWeek.SpecialPointsAwarded.HasValue)
                    {
                        int special = currentWeek.SpecialPointsAwarded.Value;
                        thisWeekMpPoints = isAbsent ? (special / 2) : special;
                    }
                    else if (isAbsent)
                    {
                        // Handle absent players with/without notice for regular weeks
                        thisWeekMpPoints = isAbsentWithNotice ? 4 : 0;
                    }

                    var displayName = $"{player.FirstName} {player.LastName.Substring(0, 1)}.";
                    var phoneDisplay = FormatPhoneNumber(player.Phone);

                    table.Cell().Background(rowColor).Padding(3).Text(displayName).FontSize(6).FontColor(Colors.Black);
                    table.Cell().Background(rowColor).Padding(2).Text(phoneDisplay).FontSize(6).AlignCenter().FontColor(Colors.Black);
                    table.Cell().Background(rowColor).Padding(3).Text(hcp.ToString("0.#")).FontSize(6).AlignCenter().FontColor(Colors.Black);
                    table.Cell().Background(rowColor).Padding(3).Text(avg.ToString("0.00")).FontSize(6).AlignCenter().FontColor(Colors.Black);
                    table.Cell().Background(rowColor).Padding(3).Text(gross > 0 ? gross.ToString() : (isAbsent ? "ABS" : "-")).FontSize(6).AlignCenter().FontColor(Colors.Black);
                    table.Cell().Background(rowColor).Padding(3).Text(thisWeekMpPoints > 0 ? thisWeekMpPoints.ToString() : "-").FontSize(6).AlignCenter().FontColor(Colors.Black);

                    var sessionTotalColor = accumScore > 0 ? Colors.Grey.Lighten2 : rowColor;
                    table.Cell().Background(sessionTotalColor).Padding(3).Text(accumScore > 0 ? accumScore.ToString() : "-").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                }
            });
        }

        private void CreateNextWeekMatchupsTable(IContainer container, Flight flight, Week currentWeek, Guid seasonId, Dictionary<Guid, decimal> handicapData)
        {
            var nextWeek = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber == currentWeek.WeekNumber + 1)
                .FirstOrDefault();

            if (nextWeek == null) return;

            // Get ALL matchups for next week, then filter by flight
            var allNextWeekMatchups = _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Where(m => m.WeekId == nextWeek.Id)
                .ToList();

            // Filter matchups to only include those for this flight
            var assignments = _flightAssignmentService.GetAllAssignments().ToList();
            var nextWeekMatchupsForFlight = allNextWeekMatchups.Where(m =>
            {
                var playerAAssignment = assignments.FirstOrDefault(a => a.PlayerId == m.PlayerAId && a.Flight != null && a.Flight.SeasonId == seasonId);
                return playerAAssignment?.Flight?.Id == flight.Id;
            }).ToList();

            if (!nextWeekMatchupsForFlight.Any()) return;

            container.Table(table =>
            {
                table.ColumnsDefinition(columns => columns.RelativeColumn());

                // Header
                table.Cell().Padding(3).Background(Colors.White)
                    .Text($"Next Matchups - {nextWeek.Date:dddd, MMMM d}")
                    .FontSize(8).Bold().FontColor(Colors.Black).AlignCenter();

                // Matchups content - using Column instead of single text for better line handling
                table.Cell().Padding(3).Background(Colors.White)
                    .Column(column =>
                    {
                        foreach (var matchup in nextWeekMatchupsForFlight)
                        {
                            var playerA = matchup.PlayerA;
                            var playerB = matchup.PlayerB;

                            if (playerA == null || playerB == null) continue;

                            var playerAHandicap = playerA != null && handicapData.ContainsKey(playerA.Id)
                                ? handicapData[playerA.Id]
                                : (playerA != null ? _playerSeasonStatsService.GetInitialHandicapAsync(playerA.Id, seasonId).Result : 0m);
                            var playerBHandicap = playerB != null && handicapData.ContainsKey(playerB.Id)
                                ? handicapData[playerB.Id]
                                : (playerB != null ? _playerSeasonStatsService.GetInitialHandicapAsync(playerB.Id, seasonId).Result : 0m);

                            int playerAHandicapInt = (int)Math.Round((double)playerAHandicap);
                            int playerBHandicapInt = (int)Math.Round((double)playerBHandicap);
                            int handicapDiff = Math.Abs(playerAHandicapInt - playerBHandicapInt);

                            string strokeInfo = "";
                            if (handicapDiff > 0)
                            {
                                if (playerAHandicapInt > playerBHandicapInt)
                                    strokeInfo = $" ({playerA!.FirstName.Substring(0, 1)}. gets {handicapDiff})";
                                else
                                    strokeInfo = $" ({playerB!.FirstName.Substring(0, 1)}. gets {handicapDiff})";
                            }

                            var line = $"{playerA!.FirstName} {playerA.LastName.Substring(0, 1)}. ({playerAHandicap:0.#}) vs " +
                                      $"{playerB!.FirstName} {playerB.LastName.Substring(0, 1)}. ({playerBHandicap:0.#}){strokeInfo}";

                            column.Item().Text(line).FontSize(8).LineHeight(1.0f).FontColor(Colors.Black);
                        }
                    });
            });
        }

        /// <summary>
        /// Generate an explanation of how the match was won, including absentee scenarios
        /// </summary>
        private string GetMatchResultExplanation(Matchup matchup, string playerAName, string playerBName)
        {
            string playerAFirstName = playerAName.Split(' ')[0];
            string playerBFirstName = playerBName.Split(' ')[0];

            // Handle absence scenarios first
            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
            {
                if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                {
                    return "Both players absent with notice - 4 points each";
                }
                else if (matchup.PlayerAAbsentWithNotice)
                {
                    return $"{playerAFirstName} absent with notice (4 pts), {playerBFirstName} absent without notice (0 pts)";
                }
                else if (matchup.PlayerBAbsentWithNotice)
                {
                    return $"{playerBFirstName} absent with notice (4 pts), {playerAFirstName} absent without notice (0 pts)";
                }
                else
                {
                    return "Both players absent without notice - 0 points each";
                }
            }
            else if (matchup.PlayerAAbsent)
            {
                int playerBPoints = matchup.PlayerBPoints ?? 0;
                string noticeText = matchup.PlayerAAbsentWithNotice ? "with notice (4 pts)" : "without notice (0 pts)";
                string playerBPerformance = playerBPoints == 16 ? "beat average (16 pts)" : playerBPoints == 8 ? "didn't beat average (8 pts)" : $"earned {playerBPoints} pts";
                return $"{playerAFirstName} absent {noticeText} - {playerBFirstName} played and {playerBPerformance}";
            }
            else if (matchup.PlayerBAbsent)
            {
                int playerAPoints = matchup.PlayerAPoints ?? 0;
                string noticeText = matchup.PlayerBAbsentWithNotice ? "with notice (4 pts)" : "without notice (0 pts)";
                string playerAPerformance = playerAPoints == 16 ? "beat average (16 pts)" : playerAPoints == 8 ? "didn't beat average (8 pts)" : $"earned {playerAPoints} pts";
                return $"{playerBFirstName} absent {noticeText} - {playerAFirstName} played and {playerAPerformance}";
            }

            // Normal match play scenarios
            int playerATotalPoints = matchup.PlayerAPoints ?? 0;
            int playerBTotalPoints = matchup.PlayerBPoints ?? 0;
            int playerAHolePoints = matchup.PlayerAHolePoints;
            int playerBHolePoints = matchup.PlayerBHolePoints;

            if (playerATotalPoints > playerBTotalPoints)
            {
                // Player A wins
                if (matchup.PlayerAMatchWin)
                {
                    // Player A won both match play holes AND net score
                    return $"{playerAFirstName} wins match play ({playerAHolePoints}-{playerBHolePoints} holes) + net score bonus = {playerATotalPoints} pts";
                }
                else
                {
                    // Player A won match play holes but net scores were tied
                    return $"{playerAFirstName} wins match play ({playerAHolePoints}-{playerBHolePoints} holes), tied net scores = {playerATotalPoints} pts";
                }
            }
            else if (playerBTotalPoints > playerATotalPoints)
            {
                // Player B wins
                if (matchup.PlayerBMatchWin)
                {
                    // Player B won both match play holes AND net score
                    return $"{playerBFirstName} wins match play ({playerBHolePoints}-{playerAHolePoints} holes) + net score bonus = {playerBTotalPoints} pts";
                }
                else
                {
                    // Player B won match play holes but net scores were tied
                    return $"{playerBFirstName} wins match play ({playerBHolePoints}-{playerAHolePoints} holes), tied net scores = {playerBTotalPoints} pts";
                }
            }
            else
            {
                // Complete tie in total points
                if (playerAHolePoints == playerBHolePoints)
                {
                    return $"Complete tie - equal hole wins ({playerAHolePoints}-{playerBHolePoints}) and net scores = {playerATotalPoints} pts each";
                }
                else
                {
                    // This shouldn't happen in normal match play, but handle edge case
                    return $"Tied total points despite different hole wins ({playerAHolePoints}-{playerBHolePoints}) = {playerATotalPoints} pts each";
                }
            }
        }

        private void CreateWeeklyPointsBreakdownTable(IContainer container, dynamic flightGroup, Week currentWeek, Guid seasonId)
        {
            if (flightGroup == null) return;

            var flight = flightGroup.Flight;
            var matchupsInFlight = (List<Matchup>)flightGroup.Matchups;

            // Get all player IDs in this flight
            var playerIds = matchupsInFlight
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToList();
            var players = _context.Players.Where(p => playerIds.Contains(p.Id)).ToList();

            // Get session data
            var sessionStartWeek = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= currentWeek.WeekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefault();
            int sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;

            var weeksInSession = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber >= sessionStartWeekNumber && w.WeekNumber <= currentWeek.WeekNumber)
                .OrderBy(w => w.WeekNumber)
                .ToList();

            // Get all matchups for these players in the session
            var allMatchups = _context.Matchups
                .Include(m => m.Week)
                .Where(m => (playerIds.Contains(m.PlayerAId) || playerIds.Contains(m.PlayerBId))
                           && weeksInSession.Select(w => w.Id).Contains(m.WeekId))
                .ToList();

            container.Column(column =>
            {
                // Flight header
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns => columns.RelativeColumn());
                    table.Cell().Padding(2).Background(Colors.White)
                        .Text($"Flight {flight.Name}")
                        .FontSize(8).Bold().FontColor(Colors.Black).AlignCenter();
                });

                // Create table with players as rows and weeks as columns
                column.Item().Element(container =>
                {
                    container.Table(table =>
                    {
                        // Dynamic column definition: Player name + one column per week + total
                        var columnCount = weeksInSession.Count + 2;
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2f); // Player name
                            foreach (var week in weeksInSession)
                            {
                                columns.RelativeColumn(1f); // Week column
                            }
                            columns.RelativeColumn(1.2f); // Total column
                        });

                        // Header row 1 - Week numbers
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(2)
                            .Text("Player").FontSize(7).Bold().FontColor(Colors.Black);
                        foreach (var week in weeksInSession)
                        {
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(2)
                                .Text($"W{week.WeekNumber}").FontSize(7).Bold().AlignCenter().FontColor(Colors.Black);
                        }
                        table.Cell().Background(Colors.Grey.Lighten2).Padding(2)
                            .Text("Total").FontSize(7).Bold().AlignCenter().FontColor(Colors.Black);

                        // Header row 2 - Data labels
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .Text("Name").FontSize(6).FontColor(Colors.Black);
                        foreach (var week in weeksInSession)
                        {
                            table.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                                .Text("Avg  |  Score  |  HC  |  Pts").FontSize(6).AlignCenter().FontColor(Colors.Black);
                        }
                        table.Cell().Background(Colors.Grey.Lighten3).Padding(2)
                            .Text("Points").FontSize(6).AlignCenter().FontColor(Colors.Black);

                        // Player rows
                        var playersWithTotals = new List<(Player player, int totalPoints)>();

                        // Calculate total points for each player
                        foreach (var player in players)
                        {
                            int totalPoints = 0;
                            foreach (var week in weeksInSession)
                            {
                                var matchup = allMatchups.FirstOrDefault(m => m.WeekId == week.Id &&
                                    (m.PlayerAId == player.Id || m.PlayerBId == player.Id));

                                int weekPoints = 0;
                                bool isAbsent = false;
                                bool isAbsentWithNotice = false;
                                if (matchup != null)
                                {
                                    if (matchup.PlayerAId == player.Id)
                                    {
                                        weekPoints = matchup.PlayerAPoints ?? 0;
                                        isAbsent = matchup.PlayerAAbsent;
                                        isAbsentWithNotice = matchup.PlayerAAbsentWithNotice;
                                    }
                                    else if (matchup.PlayerBId == player.Id)
                                    {
                                        weekPoints = matchup.PlayerBPoints ?? 0;
                                        isAbsent = matchup.PlayerBAbsent;
                                        isAbsentWithNotice = matchup.PlayerBAbsentWithNotice;
                                    }

                                    // Handle special points weeks
                                    if (week.SpecialPointsAwarded.HasValue)
                                    {
                                        int special = week.SpecialPointsAwarded.Value;
                                        weekPoints = isAbsent ? (special / 2) : special;
                                    }
                                    else if (isAbsent)
                                    {
                                        // Handle absent players with/without notice for regular weeks
                                        weekPoints = isAbsentWithNotice ? 4 : 0;
                                    }
                                }
                                totalPoints += weekPoints;
                            }
                            playersWithTotals.Add((player, totalPoints));
                        }

                        // Sort by total points (descending), then by last name
                        var sortedPlayers = playersWithTotals
                            .OrderByDescending(p => p.totalPoints)
                            .ThenBy(p => p.player.LastName)
                            .ToList();

                        for (int i = 0; i < sortedPlayers.Count; i++)
                        {
                            var (player, playerTotalPoints) = sortedPlayers[i];
                            var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten2;
                            var displayName = $"{player.FirstName} {player.LastName.Substring(0, 1)}.";

                            table.Cell().Background(rowColor).Padding(2)
                                .Text(displayName).FontSize(7).FontColor(Colors.Black);

                            foreach (var week in weeksInSession)
                            {
                                var matchup = allMatchups.FirstOrDefault(m => m.WeekId == week.Id &&
                                    (m.PlayerAId == player.Id || m.PlayerBId == player.Id));

                                int weekPoints = 0;
                                int? grossScore = null;
                                bool isAbsent = false;
                                bool isAbsentWithNotice = false;

                                if (matchup != null)
                                {
                                    if (matchup.PlayerAId == player.Id)
                                    {
                                        weekPoints = matchup.PlayerAPoints ?? 0;
                                        grossScore = matchup.PlayerAScore;
                                        isAbsent = matchup.PlayerAAbsent;
                                        isAbsentWithNotice = matchup.PlayerAAbsentWithNotice;
                                    }
                                    else if (matchup.PlayerBId == player.Id)
                                    {
                                        weekPoints = matchup.PlayerBPoints ?? 0;
                                        grossScore = matchup.PlayerBScore;
                                        isAbsent = matchup.PlayerBAbsent;
                                        isAbsentWithNotice = matchup.PlayerBAbsentWithNotice;
                                    }

                                    // Handle special points weeks
                                    if (week.SpecialPointsAwarded.HasValue)
                                    {
                                        int special = week.SpecialPointsAwarded.Value;
                                        weekPoints = isAbsent ? (special / 2) : special;
                                    }
                                    else if (isAbsent)
                                    {
                                        // Handle absent players with/without notice for regular weeks
                                        weekPoints = isAbsentWithNotice ? 4 : 0;
                                    }
                                }

                                if (isAbsent)
                                {
                                    var cellText = isAbsentWithNotice ? "-  |  -  |  -  |  4" : "-  |  -  |  -  |  0";
                                    table.Cell().Background(rowColor).Padding(2)
                                        .Text(cellText).FontSize(7).AlignCenter().FontColor(Colors.Black);
                                }
                                else
                                {
                                    // Calculate average score up to this week (including this week)
                                    var avg = _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(
                                        player.Id, seasonId, week.WeekNumber + 1).Result;

                                    // Get player's handicap up to this week (same method as league summary)
                                    var handicap = _handicapService.GetPlayerHandicapUpToWeekAsync(
                                        player.Id, seasonId, week.WeekNumber).Result;

                                    var avgText = avg > 0 ? avg.ToString("F2") : "-";
                                    var grossText = grossScore?.ToString() ?? "-";
                                    var handicapText = handicap > 0 ? Math.Round(handicap).ToString() : "-";
                                    var pointsText = weekPoints > 0 ? weekPoints.ToString() : "-";

                                    // Format horizontally with better separation for readability
                                    var cellText = $"{avgText}  |  {grossText}  |  {handicapText}  |  {pointsText}";

                                    table.Cell().Background(rowColor).Padding(2)
                                        .Text(cellText).FontSize(7).AlignCenter().FontColor(Colors.Black);
                                }
                            }

                            // Total column
                            var totalColor = playerTotalPoints > 0 ? Colors.Grey.Lighten3 : rowColor;
                            table.Cell().Background(totalColor).Padding(2)
                                .Text(playerTotalPoints > 0 ? playerTotalPoints.ToString() : "-")
                                .FontSize(7).Bold().AlignCenter().FontColor(Colors.Black);
                        }
                    });
                });
            });
        }

        private void CreateRemainingScheduleTable(IContainer container, Week currentWeek, Guid seasonId, List<Flight> orderedFlights, List<PlayerFlightAssignment> assignments)
        {
            // Find the current session end (next session start or end of season)
            var nextSessionStartWeek = _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber > currentWeek.WeekNumber && w.SessionStart)
                .OrderBy(w => w.WeekNumber)
                .FirstOrDefault();

            int sessionEndWeekNumber = nextSessionStartWeek?.WeekNumber - 1 ?? int.MaxValue;

            // Get remaining weeks in the current session only
            var remainingWeeks = _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.WeekNumber > currentWeek.WeekNumber &&
                           w.WeekNumber <= sessionEndWeekNumber)
                .OrderBy(w => w.WeekNumber)
                .ToList();

            if (!remainingWeeks.Any())
            {
                container.Text("No remaining weeks in the current session.")
                    .FontSize(12).FontColor(Colors.Black).AlignCenter();
                return;
            }

            // Get all remaining matchups
            var remainingMatchups = _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Include(m => m.Week)
                .Where(m => remainingWeeks.Select(w => w.Id).Contains(m.WeekId))
                .OrderBy(m => m.Week!.WeekNumber)
                .ToList();

            container.Column(column =>
            {
                // Create a schedule matrix for each flight
                foreach (var flight in orderedFlights)
                {
                    // Get players in this flight
                    var flightPlayerIds = assignments
                        .Where(a => a.Flight != null && a.Flight.Id == flight.Id && a.Flight.SeasonId == seasonId)
                        .Select(a => a.PlayerId)
                        .ToList();

                    // Get players who have remaining matchups in this flight
                    var flightPlayers = _context.Players
                        .Where(p => flightPlayerIds.Contains(p.Id))
                        .OrderBy(p => p.LastName)
                        .ThenBy(p => p.FirstName)
                        .ToList();

                    if (!flightPlayers.Any()) continue;

                    // Get matchups for this flight
                    var flightMatchups = remainingMatchups
                        .Where(m => flightPlayerIds.Contains(m.PlayerAId) && flightPlayerIds.Contains(m.PlayerBId))
                        .ToList();

                    if (!flightMatchups.Any()) continue;

                    // Create matrix data structure for this flight: player -> week -> opponent
                    var scheduleMatrix = new Dictionary<Guid, Dictionary<int, string>>();
                    foreach (var player in flightPlayers)
                    {
                        scheduleMatrix[player.Id] = new Dictionary<int, string>();
                    }

                    // Populate the matrix for this flight
                    foreach (var matchup in flightMatchups)
                    {
                        if (matchup.Week == null || matchup.PlayerA == null || matchup.PlayerB == null) continue;

                        var weekNum = matchup.Week.WeekNumber;
                        var playerAName = $"{matchup.PlayerA.FirstName} {matchup.PlayerA.LastName.Substring(0, 1)}.";
                        var playerBName = $"{matchup.PlayerB.FirstName} {matchup.PlayerB.LastName.Substring(0, 1)}.";

                        scheduleMatrix[matchup.PlayerAId][weekNum] = playerBName;
                        scheduleMatrix[matchup.PlayerBId][weekNum] = playerAName;
                    }

                    // Flight header
                    column.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns => columns.RelativeColumn());
                        table.Cell().Padding(6).Background(Colors.White)
                            .Text($"Flight {flight.Name} - Remaining Schedule")
                            .FontSize(10).Bold().FontColor(Colors.Black).AlignCenter();
                    });

                    // Flight schedule table
                    column.Item().PaddingTop(5).Element(flightContainer =>
                    {
                        flightContainer.Table(table =>
                        {
                            // Dynamic column definition: Player name + one column per remaining week
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2f); // Player name column
                                foreach (var week in remainingWeeks)
                                {
                                    columns.RelativeColumn(1.2f); // Week columns
                                }
                            });

                            // Header row
                            table.Cell().Background(Colors.Grey.Lighten2).Padding(3)
                                .Text("Player").FontSize(7).Bold().FontColor(Colors.Black);
                            foreach (var week in remainingWeeks)
                            {
                                table.Cell().Background(Colors.Grey.Lighten2).Padding(3)
                                    .Column(col =>
                                    {
                                        col.Item().Text($"Week {week.WeekNumber}").FontSize(6).Bold().AlignCenter().FontColor(Colors.Black);
                                        col.Item().Text(week.Date.ToString("MM/dd")).FontSize(6).AlignCenter().FontColor(Colors.Black);
                                    });
                            }

                            // Player rows for this flight
                            for (int i = 0; i < flightPlayers.Count; i++)
                            {
                                var player = flightPlayers[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten2;
                                var playerDisplayName = $"{player.FirstName} {player.LastName}";

                                // Player name cell
                                table.Cell().Background(rowColor).Padding(3)
                                    .Text(playerDisplayName).FontSize(7).FontColor(Colors.Black);

                                // Opponent cells for each week
                                foreach (var week in remainingWeeks)
                                {
                                    var opponent = scheduleMatrix[player.Id].ContainsKey(week.WeekNumber)
                                        ? scheduleMatrix[player.Id][week.WeekNumber]
                                        : "-";

                                    table.Cell().Background(rowColor).Padding(3)
                                        .Text(opponent).FontSize(6).AlignCenter().FontColor(Colors.Black);
                                }
                            }
                        });
                    });
                }
            });
        }

        /// <summary>
        /// Format phone number for display in PDF, ensuring it stays on one line
        /// </summary>
        private string FormatPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "-";

            // Remove all non-numeric characters
            var digits = new string(phone.Where(char.IsDigit).ToArray());

            // Format as (XXX) XXX-XXXX for 10-digit numbers
            if (digits.Length == 10)
            {
                return $"({digits.Substring(0, 3)}) {digits.Substring(3, 3)}-{digits.Substring(6, 4)}";
            }

            // For other lengths, just return the original phone number without extra spaces
            return phone.Replace(" ", " ").Trim(); // Replace multiple spaces with single space
        }
    }
}
