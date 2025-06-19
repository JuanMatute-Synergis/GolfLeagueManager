using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Business
{
    public class PdfScorecardService
    {
        private readonly AppDbContext _context;

        public PdfScorecardService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generate PDF scorecard for a specific week
        /// </summary>
        public async Task<byte[]> GenerateWeekScorecardPdfAsync(Guid weekId)
        {
            var week = await _context.Weeks
                .Include(w => w.Season)
                .FirstOrDefaultAsync(w => w.Id == weekId);

            if (week == null) 
                throw new ArgumentException("Week not found");

            var matchups = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Include(m => m.Week)
                .Where(m => m.WeekId == weekId)
                .ToListAsync();

            // Group matchups by flight
            var flightGroups = new List<(string FlightName, List<Matchup> Matchups)>();
            
            foreach (var matchup in matchups)
            {
                // Get player A's flight assignment
                var playerAFlight = await _context.PlayerFlightAssignments
                    .Include(pfa => pfa.Flight)
                    .Where(pfa => pfa.PlayerId == matchup.PlayerAId)
                    .FirstOrDefaultAsync();
                
                var flightName = playerAFlight?.Flight?.Name ?? "Unassigned";
                
                var existingGroup = flightGroups.FirstOrDefault(fg => fg.FlightName == flightName);
                if (existingGroup.FlightName != null)
                {
                    existingGroup.Matchups.Add(matchup);
                }
                else
                {
                    flightGroups.Add((flightName, new List<Matchup> { matchup }));
                }
            }
            
            // Sort flights by flight number (numerically) and matchups by player name within each flight
            flightGroups = flightGroups
                .OrderBy(fg => int.TryParse(fg.FlightName, out int flightNum) ? flightNum : int.MaxValue)
                .ThenBy(fg => fg.FlightName) // Fallback for non-numeric flight names
                .Select(fg => (fg.FlightName, fg.Matchups.OrderBy(m => m.PlayerA!.LastName).ToList()))
                .ToList();

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Set smaller margins for more content per page
            document.SetMargins(20, 20, 20, 20);

            try
            {
                // Create fonts
                var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var smallFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Title section with background color - compact version
                var titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell()
                    .Add(new Paragraph("HT LYONS GOLF LEAGUE")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(14)
                        .SetFont(titleFont)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(new DeviceRgb(41, 128, 185)) // Blue background
                    .SetPadding(6)
                    .SetBorder(Border.NO_BORDER));

                titleTable.AddCell(new Cell()
                    .Add(new Paragraph($"{week.Season?.Name} - Week {week.WeekNumber} - {week.Date:MMM dd, yyyy}")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(10)
                        .SetFont(normalFont)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(new DeviceRgb(52, 152, 219)) // Lighter blue
                    .SetPadding(4)
                    .SetBorder(Border.NO_BORDER));

                document.Add(titleTable);
                document.Add(new Paragraph("\n").SetFontSize(6));

                // Create compact matchup cards grouped by flight
                if (flightGroups.Any())
                {
                    bool isFirstFlight = true;
                    
                    foreach (var (flightName, flightMatchups) in flightGroups)
                    {
                        // Add page break before new flight (except the first one)
                        if (!isFirstFlight)
                        {
                            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        }
                        isFirstFlight = false;
                        
                        // Add flight header - compact version
                        var flightHeader = new Table(1).UseAllAvailableWidth();
                        flightHeader.AddCell(new Cell()
                            .Add(new Paragraph($"FLIGHT: {flightName}")
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(12)
                                .SetFont(boldFont)
                                .SetFontColor(ColorConstants.WHITE))
                            .SetBackgroundColor(new DeviceRgb(231, 76, 60)) // Red background
                            .SetPadding(4)
                            .SetBorder(Border.NO_BORDER));
                        
                        document.Add(flightHeader);
                        document.Add(new Paragraph("\n").SetFontSize(4));
                        
                        // Add matchups for this flight - 1 per row, centered
                        for (int i = 0; i < flightMatchups.Count; i++)
                        {
                            // Create a single-column table to center the card
                            var rowTable = new Table(1);
                            rowTable.SetWidth(UnitValue.CreatePercentValue(60)); // Make narrower to center
                            rowTable.SetHorizontalAlignment(HorizontalAlignment.CENTER);
                            rowTable.SetMarginBottom(8); // Reduced margin between cards
                            rowTable.SetKeepTogether(true); // Prevent splitting across pages
                            
                            // Set fixed height for consistent card sizing
                            var fixedHeight = 180f; // Fixed height in points
                            
                            // Add the card
                            var card = await CreateCompactMatchupCardAsync(flightMatchups[i], boldFont, normalFont, smallFont);
                            card.SetKeepTogether(true); // Prevent card from splitting
                            var cell = new Cell()
                                .Add(card)
                                .SetPadding(4) // Reduced padding around card
                                .SetBorder(Border.NO_BORDER)
                                .SetHeight(fixedHeight)
                                .SetVerticalAlignment(VerticalAlignment.TOP)
                                .SetHorizontalAlignment(HorizontalAlignment.CENTER);
                            rowTable.AddCell(cell);
                            
                            // Check if adding this card would exceed page capacity
                            // Estimate: compact title (~40pt) + compact flight header (~25pt) + current cards + new card (~190pt)
                            var estimatedHeight = 65 + (i + 1) * 190;
                            var pageHeight = 792; // Standard letter size height in points
                            var availableHeight = pageHeight - 60; // Accounting for smaller margins
                            
                            if (i > 0 && estimatedHeight > availableHeight)
                            {
                                // Add page break before this card to prevent splitting
                                document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                                
                                // Re-add flight header on new page - compact version
                                var newPageFlightHeader = new Table(1).UseAllAvailableWidth();
                                newPageFlightHeader.AddCell(new Cell()
                                    .Add(new Paragraph($"FLIGHT: {flightName} (continued)")
                                        .SetTextAlignment(TextAlignment.CENTER)
                                        .SetFontSize(12)
                                        .SetFont(boldFont)
                                        .SetFontColor(ColorConstants.WHITE))
                                    .SetBackgroundColor(new DeviceRgb(231, 76, 60)) // Red background
                                    .SetPadding(4)
                                    .SetBorder(Border.NO_BORDER));
                                
                                document.Add(newPageFlightHeader);
                                document.Add(new Paragraph("\n").SetFontSize(4));
                            }
                            
                            document.Add(rowTable);
                        }
                        
                        // Add minimal spacing after flight
                        document.Add(new Paragraph("\n").SetFontSize(4));
                    }
                }
                else
                {
                    document.Add(new Paragraph("No matchups found for this week.")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(14)
                        .SetFont(normalFont)
                        .SetFontColor(new DeviceRgb(231, 76, 60))); // Red color
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"PDF generation failed: {ex.Message}", ex);
            }
        }

        private async Task<Table> CreateCompactMatchupCardAsync(Matchup matchup, PdfFont boldFont, PdfFont normalFont, PdfFont smallFont)
        {
            // Get hole scores for display purposes only (not for calculation)
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {holeScores.Count} hole scores for matchup {matchup.Id}");
            foreach (var hs in holeScores)
            {
                Console.WriteLine($"[DEBUG] Hole {hs.HoleNumber}: PlayerA={hs.PlayerAScore}, PlayerB={hs.PlayerBScore}");
            }

            var courseHoles = await _context.CourseHoles
                .OrderBy(ch => ch.HoleNumber)
                .Take(9) // 9-hole course
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {courseHoles.Count} course holes");

            // Use existing backend calculated results - NO recalculation needed!
            var playerATotalPoints = matchup.PlayerAPoints ?? 0;  // Total points including match bonus
            var playerBTotalPoints = matchup.PlayerBPoints ?? 0;  // Total points including match bonus
            var playerAHolePoints = matchup.PlayerAHolePoints;    // Hole-by-hole points only
            var playerBHolePoints = matchup.PlayerBHolePoints;    // Hole-by-hole points only
            var playerAWins = playerATotalPoints > playerBTotalPoints;
            var playerBWins = playerBTotalPoints > playerATotalPoints;

            // Get handicap information
            var playerAHandicap = (int)(matchup.PlayerA?.CurrentHandicap ?? 0);
            var playerBHandicap = (int)(matchup.PlayerB?.CurrentHandicap ?? 0);
            var handicapDifference = Math.Abs(playerAHandicap - playerBHandicap);
            var strokeRecipient = playerAHandicap > playerBHandicap ? "A" : (playerBHandicap > playerAHandicap ? "B" : null);

            // Create main card container with fixed dimensions
            var cardTable = new Table(1).UseAllAvailableWidth();
            cardTable.SetBorder(new SolidBorder(new DeviceRgb(189, 195, 199), 2));
            cardTable.SetKeepTogether(true); // Ensure entire card stays together
            
            // Remove fixed height to allow content to determine card size
            // cardTable.SetHeight(170f); // Fixed height to ensure all cards are exactly the same size
            // cardTable.SetMaxHeight(170f); // Ensure no overflow

            // Header with player names and match info (matching frontend header)
            var headerTable = new Table(new float[] { 3, 2, 3 }).UseAllAvailableWidth();
            
            // Player A info (left-aligned like frontend)
            var playerAColor = playerAWins ? new DeviceRgb(46, 204, 113) : new DeviceRgb(59, 130, 246); // Green if wins, blue otherwise
            var playerACell = new Cell()
                .Add(new Paragraph($"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}")
                    .SetFont(boldFont)
                    .SetFontSize(8) // Reduced from 9
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"HC: {playerAHandicap}")
                    .SetFont(smallFont)
                    .SetFontSize(6) // Reduced from 7
                    .SetFontColor(ColorConstants.WHITE));

            // Add absence info or stroke info for Player A
            if (matchup.PlayerAAbsent)
            {
                var absenceText = matchup.PlayerAAbsentWithNotice ? "ABSENT (Notice - 4pts)" : "ABSENT (No Notice - 0pts)";
                playerACell.Add(new Paragraph(absenceText)
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(new DeviceRgb(255, 193, 7))); // Yellow for absence
            }
            else if (strokeRecipient == "A" && handicapDifference > 0)
            {
                playerACell.Add(new Paragraph($"+{handicapDifference} strokes")
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(new DeviceRgb(34, 197, 94))); // Green stroke indicator
            }
            else
            {
                playerACell.Add(new Paragraph(""));
            }

            playerACell
                .Add(new Paragraph($"{playerATotalPoints}/20")
                    .SetFont(boldFont)
                    .SetFontSize(9) // Reduced from 10
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"({playerAHolePoints}{(matchup.PlayerAMatchWin ? "+2" : "")})")
                    .SetFont(smallFont)
                    .SetFontSize(7) // Reduced from 8
                    .SetFontColor(new DeviceRgb(200, 200, 200)))
                .SetBackgroundColor(playerAColor)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetPadding(3) // Reduced from 4
                .SetBorder(Border.NO_BORDER);

            // VS section (center)
            var vsCell = new Cell()
                .Add(new Paragraph("VS")
                    .SetFont(boldFont)
                    .SetFontSize(9) // Reduced from 10
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"Week {matchup.Week?.WeekNumber ?? 0}")
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"{playerATotalPoints} - {playerBTotalPoints}")
                    .SetFont(boldFont)
                    .SetFontSize(7) // Reduced from 8
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(155, 89, 182))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(3) // Reduced from 4
                .SetBorder(Border.NO_BORDER);

            // Player B info (right-aligned like frontend)
            var playerBColor = playerBWins ? new DeviceRgb(46, 204, 113) : new DeviceRgb(239, 68, 68); // Green if wins, red otherwise
            var playerBCell = new Cell()
                .Add(new Paragraph($"{playerBTotalPoints}/20")
                    .SetFont(boldFont)
                    .SetFontSize(7) // Reduced from 8
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"({playerBHolePoints}{(matchup.PlayerBMatchWin ? "+2" : "")})")
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(new DeviceRgb(200, 200, 200)));

            // Add absence info or stroke info for Player B
            if (matchup.PlayerBAbsent)
            {
                var absenceText = matchup.PlayerBAbsentWithNotice ? "ABSENT (Notice - 4pts)" : "ABSENT (No Notice - 0pts)";
                playerBCell.Add(new Paragraph(absenceText)
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(new DeviceRgb(255, 193, 7))); // Yellow for absence
            }
            else if (strokeRecipient == "B" && handicapDifference > 0)
            {
                playerBCell.Add(new Paragraph($"+{handicapDifference} strokes")
                    .SetFont(smallFont)
                    .SetFontSize(5) // Reduced from 6
                    .SetFontColor(new DeviceRgb(34, 197, 94))); // Green stroke indicator
            }
            else
            {
                playerBCell.Add(new Paragraph(""));
            }

            playerBCell
                .Add(new Paragraph($"HC: {playerBHandicap}")
                    .SetFont(smallFont)
                    .SetFontSize(6) // Reduced from 7
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}")
                    .SetFont(boldFont)
                    .SetFontSize(8) // Reduced from 9
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(playerBColor)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPadding(3) // Reduced from 4
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(playerACell);
            headerTable.AddCell(vsCell);
            headerTable.AddCell(playerBCell);

            // Scorecard table (matching frontend layout exactly)
            var scoreTable = new Table(new float[] { 1.2f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 0.8f, 1.2f }).UseAllAvailableWidth();
            scoreTable.SetMarginTop(2); // Increased from 1 for better spacing

            // Headers row
            scoreTable.AddHeaderCell(CreateHeaderCell("Player", boldFont));
            for (int hole = 1; hole <= 9; hole++)
            {
                var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                var par = courseHole?.Par ?? 4;
                var handicap = courseHole?.HandicapIndex ?? hole;
                
                scoreTable.AddHeaderCell(CreateHeaderCell($"H{hole}\nPar {par}", smallFont));
            }
            scoreTable.AddHeaderCell(CreateHeaderCell("Total", boldFont));

            // Player A row
            var playerARow = new List<Cell>();
            playerARow.Add(CreatePlayerNameCell($"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}", boldFont, playerAColor));
            
            if (matchup.PlayerAAbsent)
            {
                // Show "ABSENT" across all holes for absent player
                for (int hole = 1; hole <= 9; hole++)
                {
                    playerARow.Add(CreateScoreCell("ABSENT", smallFont, new DeviceRgb(255, 235, 205))); // Light orange background
                }
                playerARow.Add(CreateTotalCell("ABSENT", boldFont));
            }
            else
            {
                // Normal scoring display
                for (int hole = 1; hole <= 9; hole++)
                {
                    var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                    var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                    var holeHandicap = courseHole?.HandicapIndex ?? hole;
                    
                    var grossScore = holeScore?.PlayerAScore;
                    var strokesReceived = CalculateStrokesForHole(holeHandicap, playerAHandicap, playerBHandicap);
                    var netScore = grossScore.HasValue ? grossScore.Value - strokesReceived : (int?)null;
                    
                    var isStrokeHole = strokeRecipient == "A" && strokesReceived > 0;
                    var cellColor = isStrokeHole ? new DeviceRgb(220, 252, 231) : null; // Light green for stroke holes
                    
                    var scoreText = grossScore?.ToString() ?? "-";
                    var netText = netScore?.ToString() ?? "-";
                    var cellText = $"{scoreText}\n{netText}";
                    
                    Console.WriteLine($"[DEBUG] Hole {hole} PlayerA: gross={grossScore}, net={netScore}, strokes={strokesReceived}, cellText='{cellText}'");
                    
                    playerARow.Add(CreateScoreCell(cellText, normalFont, cellColor));
                }
                
                // Calculate gross totals from actual hole scores for display
                var playerAGrossTotal = holeScores.Where(hs => hs.PlayerAScore.HasValue).Sum(hs => hs.PlayerAScore!.Value);
                
                // Calculate net totals 
                var playerANetTotal = playerAGrossTotal - CalculateTotalStrokes(playerAHandicap, playerBHandicap, courseHoles, true);
                
                playerARow.Add(CreateTotalCell($"G: {playerAGrossTotal}\nN: {playerANetTotal}", boldFont));
            }
            
            foreach (var cell in playerARow)
            {
                scoreTable.AddCell(cell);
            }

            // Player B row  
            var playerBRow = new List<Cell>();
            playerBRow.Add(CreatePlayerNameCell($"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}", boldFont, playerBColor));
            
            if (matchup.PlayerBAbsent)
            {
                // Show "ABSENT" across all holes for absent player
                for (int hole = 1; hole <= 9; hole++)
                {
                    playerBRow.Add(CreateScoreCell("ABSENT", smallFont, new DeviceRgb(255, 235, 205))); // Light orange background
                }
                playerBRow.Add(CreateTotalCell("ABSENT", boldFont));
            }
            else
            {
                // Normal scoring display
                for (int hole = 1; hole <= 9; hole++)
                {
                    var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                    var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                    var holeHandicap = courseHole?.HandicapIndex ?? hole;
                    
                    var grossScore = holeScore?.PlayerBScore;
                    var strokesReceived = CalculateStrokesForHole(holeHandicap, playerBHandicap, playerAHandicap);
                    var netScore = grossScore.HasValue ? grossScore.Value - strokesReceived : (int?)null;
                    
                    var isStrokeHole = strokeRecipient == "B" && strokesReceived > 0;
                    var cellColor = isStrokeHole ? new DeviceRgb(220, 252, 231) : null; // Light green for stroke holes
                    
                    var scoreText = grossScore?.ToString() ?? "-";
                    var netText = netScore?.ToString() ?? "-";
                    var cellText = $"{scoreText}\n{netText}";
                    
                    Console.WriteLine($"[DEBUG] Hole {hole} PlayerB: gross={grossScore}, net={netScore}, strokes={strokesReceived}, cellText='{cellText}'");
                    
                    playerBRow.Add(CreateScoreCell(cellText, normalFont, cellColor));
                }
                
                // Calculate gross and net totals for Player B
                var playerBGrossTotal = holeScores.Where(hs => hs.PlayerBScore.HasValue).Sum(hs => hs.PlayerBScore!.Value);
                var playerBNetTotal = playerBGrossTotal - CalculateTotalStrokes(playerBHandicap, playerAHandicap, courseHoles, false);
                
                playerBRow.Add(CreateTotalCell($"G: {playerBGrossTotal}\nN: {playerBNetTotal}", boldFont));
            }
            
            foreach (var cell in playerBRow)
            {
                scoreTable.AddCell(cell);
            }

            // Match Points row (like frontend)
            var matchPointsRow = new List<Cell>();
            matchPointsRow.Add(CreateHeaderCell("Match Pts", boldFont));
            
            // Calculate total hole points from individual holes
            var calculatedPlayerAHolePoints = 0;
            var calculatedPlayerBHolePoints = 0;
            
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                // For absent players, show absence points across all holes
                for (int hole = 1; hole <= 9; hole++)
                {
                    matchPointsRow.Add(CreateScoreCell("0\n0", smallFont, new DeviceRgb(255, 235, 205))); // Light orange for absence
                }
                // Use stored absence points from database
                calculatedPlayerAHolePoints = playerAHolePoints;
                calculatedPlayerBHolePoints = playerBHolePoints;
                var totalPointsText = $"{calculatedPlayerAHolePoints}\n{calculatedPlayerBHolePoints}";
                matchPointsRow.Add(CreateTotalCell(totalPointsText, boldFont));
            }
            else
            {
                // Normal hole-by-hole points display
                for (int hole = 1; hole <= 9; hole++)
                {
                    var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                    var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                    
                    if (holeScore?.PlayerAScore.HasValue == true && holeScore?.PlayerBScore.HasValue == true && courseHole != null)
                    {
                        var holeWinner = CalculateHoleWinner(holeScore, courseHole, playerAHandicap, playerBHandicap);
                        var playerAPoints = holeWinner.PlayerAPoints;
                        var playerBPoints = holeWinner.PlayerBPoints;
                        
                        // Add to totals
                        calculatedPlayerAHolePoints += playerAPoints;
                        calculatedPlayerBHolePoints += playerBPoints;
                        
                        var pointsText = $"{playerAPoints}\n{playerBPoints}";
                        var pointsColor = playerAPoints > playerBPoints ? new DeviceRgb(46, 204, 113) : 
                                        (playerBPoints > playerAPoints ? new DeviceRgb(239, 68, 68) : 
                                         new DeviceRgb(241, 196, 15)); // Green, Red, or Yellow
                        
                        matchPointsRow.Add(CreateScoreCell(pointsText, smallFont, pointsColor));
                    }
                    else
                    {
                        matchPointsRow.Add(CreateScoreCell("0\n0", smallFont));
                    }
                }
                
                var totalPointsText = $"{calculatedPlayerAHolePoints}\n{calculatedPlayerBHolePoints}";
                matchPointsRow.Add(CreateTotalCell(totalPointsText, boldFont));
            }
            
            foreach (var cell in matchPointsRow)
            {
                scoreTable.AddCell(cell);
            }
            
            // Add match bonus summary row showing total points (hole points + 2-point bonus)
            var bonusRow = new List<Cell>();
            bonusRow.Add(CreateHeaderCell("Match Bonus", boldFont));
            
            // Show the 2-point bonus or 1-point tie bonus for each hole (empty cells)
            for (int hole = 1; hole <= 9; hole++)
            {
                bonusRow.Add(CreateScoreCell("", smallFont)); // Empty cells for individual holes
            }
            
            // Calculate and show the match bonus
            var playerABonus = playerATotalPoints - playerAHolePoints;  // Should be 2, 1, or 0
            var playerBBonus = playerBTotalPoints - playerBHolePoints;  // Should be 2, 1, or 0
            var bonusText = $"{playerABonus}\n{playerBBonus}";
            bonusRow.Add(CreateTotalCell(bonusText, boldFont));
            
            foreach (var cell in bonusRow)
            {
                scoreTable.AddCell(cell);
            }
            
            // Add total match points row (hole points + bonus)
            var totalRow = new List<Cell>();
            totalRow.Add(CreateHeaderCell("Total Points", boldFont));
            
            // Show empty cells for individual holes
            for (int hole = 1; hole <= 9; hole++)
            {
                totalRow.Add(CreateScoreCell("", smallFont)); // Empty cells for individual holes
            }
            
            // Show total match points
            var finalTotalText = $"{playerATotalPoints}\n{playerBTotalPoints}";
            var totalColor = playerAWins ? new DeviceRgb(46, 204, 113) : 
                           playerBWins ? new DeviceRgb(239, 68, 68) : 
                           new DeviceRgb(241, 196, 15); // Green for A wins, Red for B wins, Yellow for tie
            totalRow.Add(CreateTotalCellWithColor(finalTotalText, boldFont, totalColor));
            
            foreach (var cell in totalRow)
            {
                scoreTable.AddCell(cell);
            }

            // Match result summary (like frontend footer)
            var summaryTable = new Table(1).UseAllAvailableWidth();
            
            string winnerText;
            string detailText;
            
            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
            {
                winnerText = "Both Players Absent";
                if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                    detailText = "Both gave notice - Points split 10-10";
                else if (matchup.PlayerAAbsentWithNotice)
                    detailText = $"{matchup.PlayerA?.FirstName} gave notice - 4 points";
                else if (matchup.PlayerBAbsentWithNotice)
                    detailText = $"{matchup.PlayerB?.FirstName} gave notice - 4 points";
                else
                    detailText = "No notice given - 0 points each";
            }
            else if (matchup.PlayerAAbsent)
            {
                winnerText = $"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName} Wins";
                var noticeText = matchup.PlayerAAbsentWithNotice ? "with notice" : "no notice";
                detailText = $"{matchup.PlayerA?.FirstName} absent ({noticeText}) - Final: {playerATotalPoints}-{playerBTotalPoints}";
            }
            else if (matchup.PlayerBAbsent)
            {
                winnerText = $"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName} Wins";
                var noticeText = matchup.PlayerBAbsentWithNotice ? "with notice" : "no notice";
                detailText = $"{matchup.PlayerB?.FirstName} absent ({noticeText}) - Final: {playerATotalPoints}-{playerBTotalPoints}";
            }
            else
            {
                // Normal match
                winnerText = playerAWins ? $"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName} Wins" :
                           playerBWins ? $"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName} Wins" :
                           "Match Tied";
                detailText = $"Final Score: {playerATotalPoints} - {playerBTotalPoints}";
            }
            
            var winnerColor = (matchup.PlayerAAbsent || matchup.PlayerBAbsent) ? new DeviceRgb(255, 193, 7) : // Yellow for absence
                            playerAWins ? new DeviceRgb(46, 204, 113) :
                            playerBWins ? new DeviceRgb(239, 68, 68) :
                            new DeviceRgb(241, 196, 15);
            
            summaryTable.AddCell(new Cell()
                .Add(new Paragraph(winnerText)
                    .SetFont(boldFont)
                    .SetFontSize(10) // Reduced from 12
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph(detailText)
                    .SetFont(normalFont)
                    .SetFontSize(8) // Reduced from 10
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(winnerColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(4) // Reduced from 6
                .SetBorder(Border.NO_BORDER));

            // Add all components to card
            cardTable.AddCell(new Cell().Add(headerTable).SetBorder(Border.NO_BORDER).SetPadding(0));
            cardTable.AddCell(new Cell().Add(scoreTable).SetBorder(Border.NO_BORDER).SetPadding(3)); // Increased from 1 for better visibility
            cardTable.AddCell(new Cell().Add(summaryTable).SetBorder(Border.NO_BORDER).SetPadding(0));

            return cardTable;
        }

        // New helper methods matching frontend logic
        private int CalculateStrokesForHole(int holeHandicap, int playerHandicap, int opponentHandicap)
        {
            // Calculate absolute handicap difference (matching frontend logic)
            var handicapDifference = Math.Abs(playerHandicap - opponentHandicap);
            
            if (handicapDifference == 0)
                return 0; // No strokes when handicaps are equal

            // Only the higher handicap player gets strokes
            var playerReceivesStrokes = playerHandicap > opponentHandicap;
            if (!playerReceivesStrokes)
                return 0;

            // Standard golf handicap allocation (match frontend logic):
            // Holes are ranked 1-9 by difficulty (handicap index)
            // Player receives strokes on holes based on their handicap difference
            var totalStrokes = (int)Math.Round((double)handicapDifference);
            
            // First round: give strokes to holes 1-9
            if (holeHandicap <= Math.Min(totalStrokes, 9))
                return 1;
            
            // Second round: give additional strokes to holes 1-9 if handicap difference > 9
            if (totalStrokes > 9 && holeHandicap <= (totalStrokes - 9))
                return 2;
            
            return 0;
        }

        private (int PlayerAPoints, int PlayerBPoints) CalculateHoleWinner(HoleScore holeScore, CourseHole courseHole, int playerAHandicap, int playerBHandicap)
        {
            var playerAGross = holeScore.PlayerAScore ?? 0;
            var playerBGross = holeScore.PlayerBScore ?? 0;
            var holeHandicap = courseHole?.HandicapIndex ?? 1;

            // Calculate net scores using exact frontend logic
            var playerAStrokes = CalculateStrokesForHole(holeHandicap, playerAHandicap, playerBHandicap);
            var playerBStrokes = CalculateStrokesForHole(holeHandicap, playerBHandicap, playerAHandicap);

            var playerANet = playerAGross - playerAStrokes;
            var playerBNet = playerBGross - playerBStrokes;

            // Award points like frontend (2 for win, 1 for tie, 0 for loss)
            if (playerANet < playerBNet)
                return (2, 0);
            else if (playerBNet < playerANet)
                return (0, 2);
            else
                return (1, 1);
        }

        private async Task<MatchPlayResult> CalculateMatchPlayScores(List<HoleScore> holeScores, List<CourseHole> courseHoles, Matchup matchup)
        {
            var result = new MatchPlayResult();
            
            var playerAHandicap = (int)(matchup.PlayerA?.CurrentHandicap ?? 0);
            var playerBHandicap = (int)(matchup.PlayerB?.CurrentHandicap ?? 0);

            // Handle absence scenarios first (matching frontend and backend logic)
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                return await CalculateAbsenceScenarioAsync(matchup);
            }

            // Normal scenario - calculate hole by hole (like frontend)
            int playerAHolePoints = 0;
            int playerBHolePoints = 0;
            int playerAGrossTotal = 0;
            int playerBGrossTotal = 0;
            int playerANetTotal = 0;
            int playerBNetTotal = 0;

            // Calculate hole by hole (like frontend)
            for (int hole = 1; hole <= 9; hole++)
            {
                var holeScore = holeScores.FirstOrDefault(hs => hs.HoleNumber == hole);
                var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                var holeHandicap = courseHole?.HandicapIndex ?? hole;

                var playerAGross = holeScore?.PlayerAScore ?? 0;
                var playerBGross = holeScore?.PlayerBScore ?? 0;

                if (playerAGross > 0) playerAGrossTotal += playerAGross;
                if (playerBGross > 0) playerBGrossTotal += playerBGross;

                if (playerAGross > 0 && playerBGross > 0)
                {
                    // Calculate net scores using frontend logic
                    var playerAStrokes = CalculateStrokesForHole(holeHandicap, playerAHandicap, playerBHandicap);
                    var playerBStrokes = CalculateStrokesForHole(holeHandicap, playerBHandicap, playerAHandicap);

                    var playerANet = playerAGross - playerAStrokes;
                    var playerBNet = playerBGross - playerBStrokes;

                    playerANetTotal += playerANet;
                    playerBNetTotal += playerBNet;

                    // Award points like frontend (2 for win, 1 for tie, 0 for loss)
                    if (playerANet < playerBNet)
                    {
                        playerAHolePoints += 2;
                    }
                    else if (playerBNet < playerANet)
                    {
                        playerBHolePoints += 2;
                    }
                    else
                    {
                        playerAHolePoints += 1;
                        playerBHolePoints += 1;
                    }
                }
            }

            // Calculate total match points (like frontend calculateMatchPlayPoints)
            int playerATotalPoints = playerAHolePoints;
            int playerBTotalPoints = playerBHolePoints;

            // Award 2-point bonus for lower net total (match winner)
            if (playerANetTotal > 0 && playerBNetTotal > 0)
            {
                if (playerANetTotal < playerBNetTotal)
                {
                    playerATotalPoints += 2;
                    result.PlayerAWins = true;
                }
                else if (playerBNetTotal < playerANetTotal)
                {
                    playerBTotalPoints += 2;
                    result.PlayerBWins = true;
                }
                else
                {
                    // Tie in net scores - each gets 1 point instead of 2
                    playerATotalPoints += 1;
                    playerBTotalPoints += 1;
                }
            }

            result.PlayerAGrossTotal = playerAGrossTotal;
            result.PlayerBGrossTotal = playerBGrossTotal;
            result.PlayerANetTotal = playerANetTotal;
            result.PlayerBNetTotal = playerBNetTotal;
            result.PlayerAHolePoints = playerAHolePoints;
            result.PlayerBHolePoints = playerBHolePoints;
            result.PlayerATotalPoints = playerATotalPoints;
            result.PlayerBTotalPoints = playerBTotalPoints;

            return result;
        }

        private async Task<MatchPlayResult> CalculateAbsenceScenarioAsync(Matchup matchup)
        {
            var result = new MatchPlayResult();
            
            // Use the actual stored values from the matchup (already calculated by backend)
            result.PlayerATotalPoints = matchup.PlayerAPoints ?? 0;
            result.PlayerBTotalPoints = matchup.PlayerBPoints ?? 0;
            result.PlayerAHolePoints = matchup.PlayerAHolePoints;
            result.PlayerBHolePoints = matchup.PlayerBHolePoints;
            result.PlayerAWins = matchup.PlayerAMatchWin;
            result.PlayerBWins = matchup.PlayerBMatchWin;
            
            // Get hole scores to calculate actual totals for players who played
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();
            
            // Calculate totals from hole scores for players who played
            if (matchup.PlayerAAbsent)
            {
                result.PlayerAGrossTotal = 0;
                result.PlayerANetTotal = 0;
            }
            else
            {
                // Calculate from hole scores
                result.PlayerAGrossTotal = holeScores.Where(hs => hs.PlayerAScore.HasValue && hs.PlayerAScore > 0)
                    .Sum(hs => hs.PlayerAScore ?? 0);
                // For net, we'll need handicap calculation - for now use gross
                result.PlayerANetTotal = result.PlayerAGrossTotal;
            }
            
            if (matchup.PlayerBAbsent)
            {
                result.PlayerBGrossTotal = 0;
                result.PlayerBNetTotal = 0;
            }
            else
            {
                // Calculate from hole scores
                result.PlayerBGrossTotal = holeScores.Where(hs => hs.PlayerBScore.HasValue && hs.PlayerBScore > 0)
                    .Sum(hs => hs.PlayerBScore ?? 0);
                // For net, we'll need handicap calculation - for now use gross
                result.PlayerBNetTotal = result.PlayerBGrossTotal;
            }

            return result;
        }

        // Keep the existing helper methods and add new ones
        private Cell CreateHeaderCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(7)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(44, 62, 80))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(2)
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 1));
        }

        private Cell CreatePlayerNameCell(string name, PdfFont font, Color color)
        {
            return new Cell()
                .Add(new Paragraph(name)
                    .SetFont(font)
                    .SetFontSize(7)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(color)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(2)
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 1));
        }

        private Cell CreateScoreCell(string text, PdfFont font, Color? backgroundColor = null)
        {
            var cell = new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(10) // Increased from 8 to make scores even more visible
                    .SetFontColor(backgroundColor != null ? ColorConstants.BLACK : ColorConstants.BLACK))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(3) // Increased padding for better visibility
                .SetBorder(new SolidBorder(new DeviceRgb(189, 195, 199), 0.5f));

            if (backgroundColor != null)
                cell.SetBackgroundColor(backgroundColor);

            return cell;
        }

        private Cell CreateDataCell(string text, PdfFont font, Color? textColor = null)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(7)
                    .SetFontColor(textColor ?? ColorConstants.BLACK))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(2)
                .SetBorder(new SolidBorder(new DeviceRgb(189, 195, 199), 0.5f));
        }

        private Cell CreateTotalCell(string text, PdfFont font)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(7)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(44, 62, 80))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(2)
                .SetBorder(new SolidBorder(ColorConstants.WHITE, 1));
        }

        private Cell CreateTotalCellWithColor(string text, PdfFont font, DeviceRgb backgroundColor)
        {
            return new Cell()
                .Add(new Paragraph(text)
                    .SetFont(font)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(backgroundColor)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(4)
                .SetBorder(new SolidBorder(new DeviceRgb(189, 195, 199), 1));
        }

        private class MatchPlayResult
        {
            public int PlayerAGrossTotal { get; set; }
            public int PlayerBGrossTotal { get; set; }
            public int PlayerANetTotal { get; set; }
            public int PlayerBNetTotal { get; set; }
            public int PlayerAHolePoints { get; set; }
            public int PlayerBHolePoints { get; set; }
            public int PlayerATotalPoints { get; set; }
            public int PlayerBTotalPoints { get; set; }
            public bool PlayerAWins { get; set; }
            public bool PlayerBWins { get; set; }
        }

        /// <summary>
        /// Generate blank PDF scorecard for a specific week
        /// </summary>
        public async Task<byte[]> GenerateBlankWeekScorecardPdfAsync(Guid weekId)
        {
            var week = await _context.Weeks
                .Include(w => w.Season)
                .FirstOrDefaultAsync(w => w.Id == weekId);

            if (week == null) 
                throw new ArgumentException("Week not found");

            var matchups = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .Where(m => m.WeekId == weekId)
                .OrderBy(m => m.PlayerA!.LastName)
                .ToListAsync();

            using var memoryStream = new MemoryStream();
            using var writer = new PdfWriter(memoryStream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // Set smaller margins for more content per page
            document.SetMargins(20, 20, 20, 20);

            try
            {
                // Create fonts
                var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                var normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var smallFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Title section
                var titleTable = new Table(1).UseAllAvailableWidth();
                titleTable.AddCell(new Cell()
                    .Add(new Paragraph("GOLF LEAGUE SCORECARD (BLANK)")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(18)
                        .SetFont(titleFont)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(new DeviceRgb(41, 128, 185))
                    .SetPadding(10)
                    .SetBorder(Border.NO_BORDER));

                titleTable.AddCell(new Cell()
                    .Add(new Paragraph($"{week.Season?.Name} - Week {week.WeekNumber} - {week.Date:MMM dd, yyyy}")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(12)
                        .SetFont(normalFont)
                        .SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(new DeviceRgb(52, 152, 219))
                    .SetPadding(8)
                    .SetBorder(Border.NO_BORDER));

                document.Add(titleTable);
                document.Add(new Paragraph("\n").SetFontSize(10));

                // Create blank matchup cards
                if (matchups.Any())
                {
                    for (int i = 0; i < matchups.Count; i++)
                    {
                        AddBlankMatchupCard(document, matchups[i], boldFont, normalFont, smallFont);
                        
                        document.Add(new Paragraph("\n").SetFontSize(8));
                        
                        if ((i + 1) % 2 == 0 && i < matchups.Count - 1)
                        {
                            document.Add(new AreaBreak(AreaBreakType.NEXT_PAGE));
                        }
                    }
                }

                document.Close();
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"PDF generation failed: {ex.Message}", ex);
            }
        }

        private void AddBlankMatchupCard(Document document, Matchup matchup, PdfFont boldFont, PdfFont normalFont, PdfFont smallFont)
        {
            // Create main card container
            var cardTable = new Table(1).UseAllAvailableWidth();
            cardTable.SetBorder(new SolidBorder(new DeviceRgb(189, 195, 199), 2));

            // Header with player names
            var headerTable = new Table(new float[] { 1, 1, 1 }).UseAllAvailableWidth();
            
            // Player A info
            var playerACell = new Cell()
                .Add(new Paragraph($"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"Handicap: {matchup.PlayerA?.CurrentHandicap ?? 0}")
                    .SetFont(smallFont)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"Avg: {matchup.PlayerA?.CurrentAverageScore:F1}")
                    .SetFont(smallFont)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(52, 73, 94))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);

            // VS section
            var vsCell = new Cell()
                .Add(new Paragraph("VS")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph("Match Points:")
                    .SetFont(smallFont)
                    .SetFontSize(8)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph("__ - __")
                    .SetFont(boldFont)
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(155, 89, 182))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);

            // Player B info
            var playerBCell = new Cell()
                .Add(new Paragraph($"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}")
                    .SetFont(boldFont)
                    .SetFontSize(12)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"Handicap: {matchup.PlayerB?.CurrentHandicap ?? 0}")
                    .SetFont(smallFont)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .Add(new Paragraph($"Avg: {matchup.PlayerB?.CurrentAverageScore:F1}")
                    .SetFont(smallFont)
                    .SetFontSize(9)
                    .SetFontColor(ColorConstants.WHITE))
                .SetBackgroundColor(new DeviceRgb(52, 73, 94))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8)
                .SetBorder(Border.NO_BORDER);

            headerTable.AddCell(playerACell);
            headerTable.AddCell(vsCell);
            headerTable.AddCell(playerBCell);

            // Blank scorecard table
            var scoreTable = new Table(new float[] { 1, 1, 2, 2, 2 }).UseAllAvailableWidth();
            scoreTable.SetMarginTop(5);

            // Headers
            scoreTable.AddHeaderCell(CreateHeaderCell("Hole", boldFont));
            scoreTable.AddHeaderCell(CreateHeaderCell("Par", boldFont));
            scoreTable.AddHeaderCell(CreateHeaderCell($"{matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}", boldFont));
            scoreTable.AddHeaderCell(CreateHeaderCell($"{matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}", boldFont));
            scoreTable.AddHeaderCell(CreateHeaderCell("Points", boldFont));

            // Add each hole with default par values
            int[] defaultPars = { 4, 4, 3, 4, 5, 4, 3, 4, 4 };
            
            for (int hole = 1; hole <= 9; hole++)
            {
                var par = defaultPars[hole - 1];

                scoreTable.AddCell(CreateDataCell(hole.ToString(), normalFont));
                scoreTable.AddCell(CreateDataCell(par.ToString(), normalFont));
                scoreTable.AddCell(CreateDataCell("", normalFont)); // Empty for player A
                scoreTable.AddCell(CreateDataCell("", normalFont)); // Empty for player B
                scoreTable.AddCell(CreateDataCell("", normalFont)); // Empty for points
            }

            // Totals row
            scoreTable.AddCell(CreateTotalCell("TOTAL", boldFont));
            scoreTable.AddCell(CreateTotalCell("36", boldFont));
            scoreTable.AddCell(CreateTotalCell("", boldFont));
            scoreTable.AddCell(CreateTotalCell("", boldFont));
            scoreTable.AddCell(CreateTotalCell("", boldFont));

            // Add header and score table to card
            cardTable.AddCell(new Cell().Add(headerTable).SetBorder(Border.NO_BORDER).SetPadding(0));
            cardTable.AddCell(new Cell().Add(scoreTable).SetBorder(Border.NO_BORDER).SetPadding(5));

            document.Add(cardTable);
        }

        private int CalculateTotalStrokes(int playerHandicap, int opponentHandicap, List<CourseHole> courseHoles, bool isPlayerA)
        {
            if (playerHandicap <= opponentHandicap) return 0;
            
            var handicapDiff = playerHandicap - opponentHandicap;
            var totalStrokes = 0;
            
            for (int hole = 1; hole <= 9; hole++)
            {
                var courseHole = courseHoles.FirstOrDefault(ch => ch.HoleNumber == hole);
                var holeHandicap = courseHole?.HandicapIndex ?? hole;
                totalStrokes += CalculateStrokesForHole(holeHandicap, playerHandicap, opponentHandicap);
            }
            
            return totalStrokes;
        }
    }
}
