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

        private async Task<Table> CreateCompactScorecardTableAsync(Matchup matchup, List<CourseHole> holes, PdfFont font, PdfFont boldFont)
        {
            // Get player info
            var playerA = matchup.PlayerA;
            var playerB = matchup.PlayerB;
            string playerAName = playerA != null ? $"{playerA.FirstName} {playerA.LastName}" : "Player A";
            string playerBName = playerB != null ? $"{playerB.FirstName} {playerB.LastName}" : "Player B";
            var playerAHandicap = playerA?.CurrentHandicap ?? 0;
            var playerBHandicap = playerB?.CurrentHandicap ?? 0;

            // Get hole scores
            var holeScores = await _context.HoleScores.Where(hs => hs.MatchupId == matchup.Id).OrderBy(hs => hs.HoleNumber).ToListAsync();
            await _matchPlayService.CalculateMatchPlayResultsAsync(matchup.Id);
            var updatedMatchup = await _context.Matchups.FirstOrDefaultAsync(m => m.Id == matchup.Id);
            if (updatedMatchup != null) matchup = updatedMatchup;

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
            int playerAHandicapInt = (int)Math.Round((double)playerAHandicap);
            int playerBHandicapInt = (int)Math.Round((double)playerBHandicap);
            bool playerAReceivesStrokes = playerAHandicapInt > playerBHandicapInt;
            bool playerBReceivesStrokes = playerBHandicapInt > playerAHandicapInt;
            int handicapDiff = Math.Abs(playerAHandicapInt - playerBHandicapInt);
            var sortedHoles = holes.OrderBy(h => h.HandicapIndex).ToList();
            var strokeHoles = sortedHoles.Take(handicapDiff).Select(h => h.HoleNumber).ToHashSet();
            foreach (var hole in holes)
            {
                bool highlight = false;
                if (playerAReceivesStrokes && strokeHoles.Contains(hole.HoleNumber)) highlight = true;
                if (playerBReceivesStrokes && strokeHoles.Contains(hole.HoleNumber)) highlight = true;
                var cell = new Cell().Add(new Paragraph(hole.HandicapIndex.ToString()).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBackgroundColor(highlight ? new DeviceRgb(255, 255, 180) : new DeviceRgb(245, 245, 245));
                table.AddCell(cell);
            }
            table.AddCell(new Cell().SetBackgroundColor(new DeviceRgb(245, 245, 245)));

            // Row: Player A scores
            table.AddCell(new Cell().Add(new Paragraph(playerAName.Split(' ')[0]).SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(235, 245, 255)));
            int playerATotal = 0;
            foreach (var hole in holes)
            {
                var hs = holeScores.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                playerATotal += hs?.PlayerAScore ?? 0;
                table.AddCell(new Cell().Add(new Paragraph(hs?.PlayerAScore?.ToString() ?? "")
                    .SetFont(boldFont)
                    .SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            table.AddCell(new Cell().Add(new Paragraph(matchup.PlayerAScore?.ToString() ?? playerATotal.ToString())
                .SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(220, 255, 220)));

            // Row: Player B scores
            table.AddCell(new Cell().Add(new Paragraph(playerBName.Split(' ')[0]).SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(255, 245, 235)));
            int playerBTotal = 0;
            foreach (var hole in holes)
            {
                var hs = holeScores.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                playerBTotal += hs?.PlayerBScore ?? 0;
                table.AddCell(new Cell().Add(new Paragraph(hs?.PlayerBScore?.ToString() ?? "")
                    .SetFont(boldFont)
                    .SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER));
            }
            table.AddCell(new Cell().Add(new Paragraph(matchup.PlayerBScore?.ToString() ?? playerBTotal.ToString())
                .SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBackgroundColor(new DeviceRgb(255, 220, 220)));

            // Consolidated Match Play Points Row (per hole + total)
            table.AddCell(new Cell().Add(new Paragraph("MP/Hole").SetFont(boldFont).SetFontSize(9f))
                .SetBackgroundColor(new DeviceRgb(240, 240, 255)));
            int playerAMatchTotal = matchup.PlayerAPoints ?? 0;
            int playerBMatchTotal = matchup.PlayerBPoints ?? 0;
            foreach (var hole in holes)
            {
                var hs = holeScores.FirstOrDefault(x => x.HoleNumber == hole.HoleNumber);
                string mp = hs != null ? $"{hs.PlayerAMatchPoints}-{hs.PlayerBMatchPoints}" : "-";
                table.AddCell(new Cell().Add(new Paragraph(mp).SetFont(boldFont).SetFontSize(9f))
                    .SetTextAlignment(TextAlignment.CENTER).SetFontColor(new DeviceRgb(0, 0, 255))  // Dark blue for match points
                    .SetBackgroundColor(new DeviceRgb(240, 240, 255)));
            }
            // Show total as e.g. "12-8" in the last cell
            table.AddCell(new Cell().Add(new Paragraph($"{playerAMatchTotal}-{playerBMatchTotal}").SetFont(boldFont).SetFontSize(9f))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(new DeviceRgb(0, 0, 255)) // Dark blue for totals
                .SetBackgroundColor(new DeviceRgb(240, 240, 255)));

            // Absence status
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                string absenceExplanation = "";
                if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
                {
                    if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                        absenceExplanation = "Both absent with notice: 10 points each";
                    else if (matchup.PlayerAAbsentWithNotice)
                        absenceExplanation = "Player A absent with notice (4), Player B absent without notice (0)";
                    else if (matchup.PlayerBAbsentWithNotice)
                        absenceExplanation = "Player B absent with notice (4), Player A absent without notice (0)";
                    else
                        absenceExplanation = "Both absent without notice: 0 points each";
                }
                else if (matchup.PlayerAAbsent)
                {
                    if (matchup.PlayerAAbsentWithNotice)
                        absenceExplanation = "Player A absent with notice (4 points)";
                    else
                        absenceExplanation = "Player A absent without notice (0 points)";
                    absenceExplanation += ". Player B: " + (matchup.PlayerBPoints == 16 ? "Beat avg by whole number (16 points)" : "Did not beat avg (8 points)");
                }
                else if (matchup.PlayerBAbsent)
                {
                    if (matchup.PlayerBAbsentWithNotice)
                        absenceExplanation = "Player B absent with notice (4 points)";
                    else
                        absenceExplanation = "Player B absent without notice (0 points)";
                    absenceExplanation += ". Player A: " + (matchup.PlayerAPoints == 16 ? "Beat avg by whole number (16 points)" : "Did not beat avg (8 points)");
                }
                table.AddCell(new Cell(1, holeCount + 2)
                    .Add(new Paragraph(absenceExplanation))
                    .SetFontColor(ColorConstants.RED)
                    .SetFont(boldFont));
            }

            return table;
        }
    }
}
