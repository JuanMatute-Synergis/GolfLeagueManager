using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class PdfScorecardService
    {
        private readonly AppDbContext _context;
        private readonly MatchupService _matchupService;
        private readonly PlayerFlightAssignmentService _flightAssignmentService;
        private readonly MatchPlayService _matchPlayService;

        public PdfScorecardService(AppDbContext context, MatchupService matchupService, PlayerFlightAssignmentService flightAssignmentService, MatchPlayService matchPlayService)
        {
            _context = context;
            _matchupService = matchupService;
            _flightAssignmentService = flightAssignmentService;
            _matchPlayService = matchPlayService;
        }

        /// <summary>
        /// Generates a compact PDF of all scorecards for a specific week, grouped by flight, 4 per page, 1 flight per page.
        /// </summary>
        public async Task<byte[]> GenerateWeekScorecardPdfAsync(Guid weekId)
        {
            var matchups = (await _matchupService.GetMatchupsByWeekIdAsync(weekId)).ToList();
            if (!matchups.Any()) throw new ArgumentException("No matchups found for this week.");

            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == weekId);
            if (week == null) throw new ArgumentException("Week not found.");
            var seasonId = week.SeasonId;
            var assignments = _flightAssignmentService.GetAllAssignments().ToList();
            var flights = await _context.Flights.ToListAsync();
            var course = await _context.Courses.Include(c => c.CourseHoles).FirstOrDefaultAsync();
            if (course == null) throw new ArgumentException("No course found.");
            var holes = course.CourseHoles.OrderBy(h => h.HoleNumber).ToList();

            // Sort flights by name (assuming names are numbers or can be sorted as 1-4)
            var orderedFlights = flights.OrderBy(f => f.Name).ToList();
            // Group matchups by flight (using PlayerA's assignment for the season)
            var matchupsByFlight = orderedFlights
                .Select(flight => new {
                    Flight = flight,
                    Matchups = matchups.Where(m => {
                        var assignment = assignments.FirstOrDefault(a => a.PlayerId == m.PlayerAId && a.Flight != null && a.Flight.SeasonId == seasonId);
                        return assignment?.Flight?.Id == flight.Id;
                    }).ToList()
                })
                .Where(g => g.Matchups.Any())
                .ToList();

            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            document.SetMargins(10, 10, 10, 10);
            // Use Open Sans fonts from Fonts directory if available, else fallback to DejaVu Sans system fonts
            string fontsDir = Path.Combine(AppContext.BaseDirectory, "Fonts");
            string openSansFontPath = Path.Combine(fontsDir, "OpenSans-Regular.ttf");
            string openSansBoldFontPath = Path.Combine(fontsDir, "OpenSans-Bold.ttf");
            string fontPath = openSansFontPath;
            string boldFontPath = openSansBoldFontPath;

            var font = PdfFontFactory.CreateFont(fontPath, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);
            var boldFont = PdfFontFactory.CreateFont(boldFontPath, PdfFontFactory.EmbeddingStrategy.PREFER_EMBEDDED);

            foreach (var flightGroup in matchupsByFlight)
            {
                var flight = flightGroup.Flight;
                var flightName = $"Flight {flight.Name}";
                var matchupsInFlight = flightGroup.Matchups;
                if (!matchupsInFlight.Any()) continue;
                if (pdf.GetNumberOfPages() > 0) document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));

                // --- Add summary table as the first page for this flight ---
                var summaryTable = await CreateFlightSummaryTableAsync(flight, matchupsInFlight, week, seasonId, font, boldFont);
                document.Add(new Paragraph($"{flightName} - Weekly Summary").SetFont(boldFont).SetFontSize(11).SetTextAlignment(TextAlignment.CENTER).SetMarginBottom(5));
                document.Add(summaryTable);
                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                // --- End summary page ---

                document.Add(new Paragraph($"{flightName} - H.T. Lyons Golf League")
                    .SetFont(boldFont).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER));

                // 4 cards per page, each on its own row
                for (int i = 0; i < matchupsInFlight.Count; i++)
                {
                    var cardsTable = new Table(UnitValue.CreatePercentArray(new float[] { 1f })).UseAllAvailableWidth();
                    cardsTable.SetMarginBottom(3);
                    var matchup = matchupsInFlight[i];
                    var card = await CreateCompactScorecardTableAsync(matchup, holes, font, boldFont);
                    cardsTable.AddCell(new Cell().Add(card).SetBorder(Border.NO_BORDER).SetPadding(2));
                    document.Add(cardsTable);
                    // Add page break after every 4 cards, but only if more cards remain
                    if ((i + 1) % 4 == 0 && (i + 1) < matchupsInFlight.Count)
                    {
                        document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        document.Add(new Paragraph($"{flightName} - H.T. Lyons Golf League")
                            .SetFont(boldFont).SetFontSize(7).SetTextAlignment(TextAlignment.CENTER));
                    }
                }
            }
            document.Close();
            return ms.ToArray();
        }

        // --- New: Create summary table for a flight ---
        private async Task<Table> CreateFlightSummaryTableAsync(Flight flight, List<Matchup> matchupsInFlight, Week currentWeek, Guid seasonId, PdfFont font, PdfFont boldFont)
        {
            // Get all players in this flight for the season
            var playerIds = matchupsInFlight
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToList();
            var players = await _context.Players.Where(p => playerIds.Contains(p.Id)).ToListAsync();

            // Get all matchups for these players in the season up to and including the current week
            var allMatchups = await _context.Matchups
                .Where(m => (playerIds.Contains(m.PlayerAId) || playerIds.Contains(m.PlayerBId)))
                .ToListAsync();
            var weekNumber = currentWeek.WeekNumber;
            var weekIdsUpToCurrent = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= weekNumber)
                .Select(w => w.Id)
                .ToListAsync();
            var matchupsUpToCurrent = allMatchups.Where(m => weekIdsUpToCurrent.Contains(m.WeekId)).ToList();

            // Table columns: Player, Handicap, Average, Gross, Net, Match Play Points, Accumulated Score
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 2f, 1f, 1f, 1f, 1f, 1f, 1.5f })).UseAllAvailableWidth();
            table.SetFont(font).SetFontSize(9f);
            table.AddHeaderCell(new Cell().Add(new Paragraph("Player").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("HCP").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Avg").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Gross").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Net").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("MP Pts").SetFont(boldFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Accum. Score").SetFont(boldFont)));

            foreach (var player in players.OrderBy(p => p.LastName).ThenBy(p => p.FirstName))
            {
                // Handicap and average from player
                var hcp = player.CurrentHandicap;
                var avg = player.CurrentAverageScore;

                // Find this week's matchup for this player
                var matchup = matchupsInFlight.FirstOrDefault(m => m.PlayerAId == player.Id || m.PlayerBId == player.Id);
                int gross = 0, net = 0, mpPoints = 0;
                if (matchup != null)
                {
                    if (matchup.PlayerAId == player.Id)
                    {
                        gross = matchup.PlayerAScore ?? 0;
                        mpPoints = matchup.PlayerAPoints ?? 0;
                        net = gross; // For more accurate net, sum net per hole if needed
                    }
                    else if (matchup.PlayerBId == player.Id)
                    {
                        gross = matchup.PlayerBScore ?? 0;
                        mpPoints = matchup.PlayerBPoints ?? 0;
                        net = gross;
                    }
                }
                // Accumulated score: sum all gross scores for this player up to and including this week
                var grossScores = matchupsUpToCurrent
                    .Where(m => (m.PlayerAId == player.Id && m.PlayerAScore.HasValue) || (m.PlayerBId == player.Id && m.PlayerBScore.HasValue))
                    .Select(m => m.PlayerAId == player.Id ? (m.PlayerAScore ?? 0) : (m.PlayerBScore ?? 0))
                    .ToList();
                var accumScore = grossScores.Sum();

                table.AddCell(new Cell().Add(new Paragraph($"{player.FirstName} {player.LastName}")));
                table.AddCell(new Cell().Add(new Paragraph(hcp.ToString("0.##"))));
                table.AddCell(new Cell().Add(new Paragraph(avg.ToString("0.##"))));
                table.AddCell(new Cell().Add(new Paragraph(gross > 0 ? gross.ToString() : "-")));
                table.AddCell(new Cell().Add(new Paragraph(net > 0 ? net.ToString() : "-")));
                table.AddCell(new Cell().Add(new Paragraph(mpPoints > 0 ? mpPoints.ToString() : "-")));
                table.AddCell(new Cell().Add(new Paragraph(accumScore > 0 ? accumScore.ToString() : "-")));
            }
            return table;
        }

        private async Task<Table> CreateCompactScorecardTableAsync(Matchup matchup, List<CourseHole> holes, PdfFont font, PdfFont boldFont)
        {
            // Get player info
            var playerA = matchup.PlayerA;
            var playerB = matchup.PlayerB;
            string playerAName = playerA != null ? $"{playerA.FirstName} {playerA.LastName}" : "Player A";
            string playerBName = playerB != null ? $"{playerB.FirstName} {playerB.LastName}" : "Player B";
            var playerAHandicap = playerA?.CurrentHandicap ?? 0;
            var playerBHandicap = playerB?.CurrentHandicap ?? 0;

            // Get hole scores - use the data that's already calculated and stored
            var holeScores = await _context.HoleScores.Where(hs => hs.MatchupId == matchup.Id).OrderBy(hs => hs.HoleNumber).ToListAsync();
            
            // Get the updated matchup data with all calculated scores
            var updatedMatchup = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == matchup.Id);
            if (updatedMatchup != null) matchup = updatedMatchup;

            // --- Use proper 9-hole stroke allocation matching backend logic ---
            var holeScoresOrdered = holeScores.OrderBy(hs => hs.HoleNumber).ToList();
            var holesInPlay = holeScoresOrdered.Select(hs => new { hs.HoleNumber, hs.HoleHandicap }).ToList();
            int playerAHandicapInt = (int)Math.Round((double)playerAHandicap);
            int playerBHandicapInt = (int)Math.Round((double)playerBHandicap);
            bool playerAReceivesStrokes = playerAHandicapInt > playerBHandicapInt;
            bool playerBReceivesStrokes = playerBHandicapInt > playerAHandicapInt;
            int handicapDiff = Math.Abs(playerAHandicapInt - playerBHandicapInt);
            // Find the hardest holes among the 9 in play (lowest HoleHandicap values) - matches backend logic
            var hardestHoles = holesInPlay.OrderBy(h => h.HoleHandicap).Take(handicapDiff).Select(h => h.HoleNumber).ToHashSet();

            // Compact horizontal layout: holes as columns + total
            int holeCount = holes.Count;
            var columnWidths = new float[holeCount + 2];
            columnWidths[0] = 1.2f;
            for (int i = 1; i <= holeCount; i++) columnWidths[i] = 1f;
            columnWidths[holeCount + 1] = 1.2f;
            var table = new Table(UnitValue.CreatePercentArray(columnWidths)).SetWidth(UnitValue.CreatePercentValue(100));
            table.SetFont(boldFont).SetFontSize(9f);

            // Header: Player names and handicaps, with winner (if any) in red at the top center
            string winner = "Tie";
            if (matchup.PlayerAMatchWin) winner = playerAName;
            else if (matchup.PlayerBMatchWin) winner = playerBName;
            // Special case: both absent, but points are not equal
            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent && (matchup.PlayerAPoints != matchup.PlayerBPoints))
            {
                if ((matchup.PlayerAPoints ?? 0) > (matchup.PlayerBPoints ?? 0))
                    winner = playerAName;
                else if ((matchup.PlayerBPoints ?? 0) > (matchup.PlayerAPoints ?? 0))
                    winner = playerBName;
            }
            string winnerText = winner == "Tie"
                ? "Tie\n"
                : $"Winner: {winner}\n";

            var headerParagraph = new Paragraph()
                .Add(new Text(winnerText).SetFont(boldFont).SetFontSize(11f).SetFontColor(ColorConstants.RED))
                .Add(new Text($"{playerAName} (HCP: {playerAHandicap}, Avg: {playerA?.CurrentAverageScore:F1}) vs {playerBName} (HCP: {playerBHandicap}, Avg: {playerB?.CurrentAverageScore:F1})")
                    .SetFont(boldFont).SetFontSize(9.5f))
                .SetMarginTop(0).SetMarginBottom(0);
            table.AddHeaderCell(new Cell(1, holeCount + 2)
                .Add(headerParagraph)
                .SetBackgroundColor(new DeviceRgb(220, 230, 250))
                .SetTextAlignment(TextAlignment.CENTER));

            // Row: Hole numbers
            table.AddCell(new Cell().Add(new Paragraph("Hole").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(220, 230, 250)));
            foreach (var hole in holes)
                table.AddCell(new Cell().Add(new Paragraph(hole.HoleNumber.ToString()).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(new DeviceRgb(220, 230, 250)));
            table.AddCell(new Cell().Add(new Paragraph("Total").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(200, 220, 255)));

            // Row: Par
            table.AddCell(new Cell().Add(new Paragraph("Par").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(ColorConstants.WHITE));
            int parTotal = 0;
            foreach (var hole in holes)
            {
                parTotal += hole.Par;
                table.AddCell(new Cell().Add(new Paragraph(hole.Par.ToString()).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            table.AddCell(new Cell().Add(new Paragraph(parTotal.ToString()).SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(240, 250, 220)));

            // Row: Handicap
            table.AddCell(new Cell().Add(new Paragraph("HCP").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(245, 245, 245)));
            foreach (var hole in holes)
            {
                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                var cell = new Cell().Add(new Paragraph((hs?.HoleHandicap ?? hole.HandicapIndex).ToString()).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(new DeviceRgb(245, 245, 245));
                table.AddCell(cell);
            }
            table.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(245, 245, 245)));

            // Row: Player A scores
            table.AddCell(new Cell().Add(new Paragraph(playerAName.Split(' ')[0]).SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(235, 245, 255)));
            int playerATotal = 0;
            int playerANetTotal = 0;
            foreach (var hole in holes)
            {
                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                int gross = hs?.PlayerAScore ?? 0;
                if (gross > 0) playerATotal += gross;
                
                // Use exact backend logic for stroke calculation
                int strokes = (playerAReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber)) ? 1 : 0;
                int net = gross > 0 ? gross - strokes : 0;
                if (gross > 0) playerANetTotal += net;
                
                string displayText = "";
                if (matchup.PlayerAAbsent)
                {
                    displayText = "ABS";
                }
                else if (hs?.PlayerAScore != null)
                {
                    displayText = $"{gross}/{net}";
                }
                
                // Highlight score cell if player A receives strokes on this hole
                bool highlightPlayerA = playerAReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber);
                var backgroundColor = highlightPlayerA ? new DeviceRgb(255, 255, 180) : ColorConstants.WHITE;
                
                table.AddCell(new Cell().Add(new Paragraph(displayText)
                    .SetFont(boldFont)
                    .SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(backgroundColor));
            }
            
            // Use the stored totals from the matchup, but show calculated gross/net for display
            var storedPlayerAScore = matchup.PlayerAScore ?? playerATotal;
            string playerATotalText = matchup.PlayerAAbsent ? "ABSENT" : $"{storedPlayerAScore}/{playerANetTotal}";
            table.AddCell(new Cell().Add(new Paragraph(playerATotalText)
                .SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(220, 255, 220)));

            // Row: Player B scores
            table.AddCell(new Cell().Add(new Paragraph(playerBName.Split(' ')[0]).SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(255, 245, 235)));
            int playerBTotal = 0;
            int playerBNetTotal = 0;
            foreach (var hole in holes)
            {
                var hs = holeScoresOrdered.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                int gross = hs?.PlayerBScore ?? 0;
                if (gross > 0) playerBTotal += gross;
                
                // Use exact backend logic for stroke calculation
                int strokes = (playerBReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber)) ? 1 : 0;
                int net = gross > 0 ? gross - strokes : 0;
                if (gross > 0) playerBNetTotal += net;
                
                string displayText = "";
                if (matchup.PlayerBAbsent)
                {
                    displayText = "ABS";
                }
                else if (hs?.PlayerBScore != null)
                {
                    displayText = $"{gross}/{net}";
                }
                
                // Highlight score cell if player B receives strokes on this hole
                bool highlightPlayerB = playerBReceivesStrokes && hs != null && hardestHoles.Contains(hole.HoleNumber);
                var backgroundColor = highlightPlayerB ? new DeviceRgb(255, 255, 180) : ColorConstants.WHITE;
                
                table.AddCell(new Cell().Add(new Paragraph(displayText)
                    .SetFont(boldFont)
                    .SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(backgroundColor));
            }
            
            // Use the stored totals from the matchup, but show calculated gross/net for display
            var storedPlayerBScore = matchup.PlayerBScore ?? playerBTotal;
            string playerBTotalText = matchup.PlayerBAbsent ? "ABSENT" : $"{storedPlayerBScore}/{playerBNetTotal}";
            table.AddCell(new Cell().Add(new Paragraph(playerBTotalText)
                .SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(255, 220, 220)));

            // Consolidated Match Play Points Row (per hole + total)
            table.AddCell(new Cell().Add(new Paragraph("MP/Hole").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(240, 240, 255)));
            
            // Use the backend-calculated match play points that are already stored
            int playerAMatchTotal = matchup.PlayerAPoints ?? 0;
            int playerBMatchTotal = matchup.PlayerBPoints ?? 0;
            
            foreach (var hole in holes)
            {
                var hs = holeScores.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                string mp;
                
                if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
                {
                    // For absent players, show 0-0 for each hole
                    mp = "0-0";
                }
                else if (hs != null)
                {
                    // Use the backend-calculated hole match points
                    mp = $"{hs.PlayerAMatchPoints}-{hs.PlayerBMatchPoints}";
                }
                else
                {
                    mp = "-";
                }
                
                table.AddCell(new Cell().Add(new Paragraph(mp).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(0, 0, 255))  // Dark blue for match points
                    .SetBackgroundColor(new DeviceRgb(240, 240, 255)));
            }
            
            // Show total match points as calculated by backend
            table.AddCell(new Cell().Add(new Paragraph($"{playerAMatchTotal}-{playerBMatchTotal}").SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(0, 0, 255)) // Dark blue for totals
                .SetBackgroundColor(new DeviceRgb(240, 240, 255)));

            // Absence status explanation
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                string absenceExplanation = "";
                if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
                {
                    if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                        absenceExplanation = "Both absent with notice: 10-10 points";
                    else if (matchup.PlayerAAbsentWithNotice)
                        absenceExplanation = $"A absent with notice (4 pts), B absent no notice (0 pts)";
                    else if (matchup.PlayerBAbsentWithNotice)
                        absenceExplanation = $"A absent no notice (0 pts), B absent with notice (4 pts)";
                    else
                        absenceExplanation = "Both absent without notice: 0-0 points";
                }
                else if (matchup.PlayerAAbsent)
                {
                    var noticeText = matchup.PlayerAAbsentWithNotice ? "with notice" : "no notice";
                    var playerAPoints = matchup.PlayerAAbsentWithNotice ? 4 : 0;
                    var playerBPoints = matchup.PlayerBPoints ?? 8;
                    var playerBExplanation = playerBPoints == 16 ? "beat average by whole stroke" : "did not beat average by whole stroke";
                    absenceExplanation = $"A absent {noticeText} ({playerAPoints} pts). B played and {playerBExplanation} ({playerBPoints} pts)";
                }
                else if (matchup.PlayerBAbsent)
                {
                    var noticeText = matchup.PlayerBAbsentWithNotice ? "with notice" : "no notice";
                    var playerBPoints = matchup.PlayerBAbsentWithNotice ? 4 : 0;
                    var playerAPoints = matchup.PlayerAPoints ?? 8;
                    var playerAExplanation = playerAPoints == 16 ? "beat average by whole stroke" : "did not beat average by whole stroke";
                    absenceExplanation = $"B absent {noticeText} ({playerBPoints} pts). A played and {playerAExplanation} ({playerAPoints} pts)";
                }
                
                // Add explanation row spanning all columns
                table.AddCell(new Cell(1, holeCount + 2)
                    .Add(new Paragraph(absenceExplanation)
                        .SetFont(boldFont)
                        .SetFontSize(8f))
                    .SetFontColor(ColorConstants.RED)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(new DeviceRgb(255, 240, 240)));
            }

            return table;
        }
    }
}
