using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Business;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ScoreImportService _importService;
        private readonly JsonImportService _jsonImportService;

        public ImportController(ScoreImportService importService, JsonImportService jsonImportService)
        {
            _importService = importService;
            _jsonImportService = jsonImportService;
        }

        [HttpPost("scores")]
        public async Task<ActionResult<ImportResult>> ImportScores([FromBody] ImportRequest request)
        {
            if (string.IsNullOrEmpty(request.CsvContent))
            {
                return BadRequest("CSV content is required");
            }

            var result = await _importService.ImportScoresFromCsvAsync(request.CsvContent);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("formatted-scores")]
        public async Task<ActionResult<ImportResult>> ImportFormattedScores([FromBody] ImportRequest request)
        {
            if (string.IsNullOrEmpty(request.CsvContent))
            {
                return BadRequest("CSV content is required");
            }

            var result = await _importService.ImportScoresFromFormattedCsvAsync(request.CsvContent);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpPost("json-scores")]
        public async Task<ActionResult<ImportResult>> ImportJsonScores([FromBody] JsonImportRequest request)
        {
            if (string.IsNullOrEmpty(request.JsonContent))
            {
                return BadRequest("JSON content is required");
            }

            var result = await _jsonImportService.ImportScoresFromJsonAsync(request.JsonContent);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }

    public class ImportRequest
    {
        public string CsvContent { get; set; } = string.Empty;
    }

    public class JsonImportRequest
    {
        public string JsonContent { get; set; } = string.Empty;
    }
}
