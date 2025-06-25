using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Business;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PdfController : ControllerBase
    {
        private readonly PdfScorecardService _pdfService;

        public PdfController(PdfScorecardService pdfService)
        {
            _pdfService = pdfService;
        }

        /// <summary>
        /// Export scorecard PDF for a specific week
        /// </summary>
        [HttpGet("scorecard/week/{weekId}")]
        public async Task<IActionResult> ExportWeekScorecardPdf(Guid weekId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateWeekScorecardPdfAsync(weekId);
                
                var fileName = $"Scorecard_Week_{weekId}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Full PDF generation error: {ex}");
                return StatusCode(500, $"Error generating PDF: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }

        /// <summary>
        /// Export blank scorecard template for a week (for printing before the round)
        /// </summary>
        [HttpGet("scorecard/blank/week/{weekId}")]
        public async Task<IActionResult> ExportBlankScorecardPdf(Guid weekId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateWeekScorecardPdfAsync(weekId);
                
                var fileName = $"Blank_Scorecard_Week_{weekId}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Export summary report PDF for a specific week
        /// </summary>
        [HttpGet("summary/week/{weekId}")]
        public async Task<IActionResult> ExportWeekSummaryPdf(Guid weekId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateWeekSummaryPdfAsync(weekId);
                
                var fileName = $"Summary_Week_{weekId}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the full exception details
                Console.WriteLine($"Full PDF generation error: {ex}");
                return StatusCode(500, $"Error generating PDF: {ex.Message} - Inner: {ex.InnerException?.Message}");
            }
        }
    }
}
