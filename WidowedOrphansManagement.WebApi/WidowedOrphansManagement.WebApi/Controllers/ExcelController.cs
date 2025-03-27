using Microsoft.AspNetCore.Mvc;
using WidowedOrphansManagement.Services.ExcelService;

namespace WidowedOrphansManagement.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcelController : ControllerBase
    {
        private readonly IExcelService _excelService;
        private readonly ILogger<ExcelController> _logger;

        public ExcelController(IExcelService excelService, ILogger<ExcelController> logger)
        {
            _excelService = excelService;
            _logger = logger;
        }

        [HttpPost("upload-parents")]
        public async Task<IActionResult> UploadParentsExcel(IFormFile file)
        {
            try
            {
                var result = await _excelService.ProcessParentsExcelFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UploadParentsExcel, Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, "שגיאה בטעינת קובץ ההורים.");
            }
        }

        [HttpPost("upload-orphans")]
        public async Task<IActionResult> UploadOrphansExcel(IFormFile file)
        {
            try
            {
                var result = await _excelService.ProcessOrphansExcelFileAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in UploadOrphansExcel, Message: {ex.Message}, InnerException: {ex.InnerException}, StackTrace: {ex.StackTrace}");
                return StatusCode(500, "שגיאה בטעינת קובץ היתומים.");
            }
        }

        [HttpGet("download-parents-example")]
        public IActionResult DownloadParentsExample()
        {
            // יצירת קובץ אקסל לדוגמה להורים
            var file = _excelService.GenerateParentsExampleExcel();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ParentsExample.xlsx");
        }

        [HttpGet("download-orphans-example")]
        public IActionResult DownloadOrphansExample()
        {
            // יצירת קובץ אקסל לדוגמה לילדים
            var file = _excelService.GenerateOrphansExampleExcel();
            return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OrphansExample.xlsx");
        }
    }
}